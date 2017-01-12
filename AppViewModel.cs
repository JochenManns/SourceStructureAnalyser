using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SourceStructureAnalyser
{
	public class AppViewModel : ViewModel
	{
		public class ExtensionModel : ViewModel
		{
			public string Name { get; }

			public int NumberOfFiles { get; }

			public int NumberOfLines { get; }

			private bool m_excluded;

			public bool IsExcluded
			{
				get { return m_excluded; }
				set
				{
					if (value == m_excluded)
						return;

					m_excluded = value;

					OnPropertyChange( nameof( m_excluded ) );

					m_model.UpdateExtensions();
				}
			}

			private readonly AppViewModel m_model;

			public ExtensionModel( string name, int count, int totalLines, bool excluded, AppViewModel model )
			{
				NumberOfLines = totalLines;
				m_excluded = excluded;
				NumberOfFiles = count;
				m_model = model;
				Name = name;
			}
		}

		private Model m_model = new Model();

		private volatile bool m_busy;

		public string ScanText => m_busy ? "Abbrechen" : "Auswerten";

		public bool CanChange => !m_busy;

		public bool CanLoad => !m_busy;

		public bool CanSave => !m_busy && IsModified;

		public string RootFolder
		{
			get { return m_model.RootPath; }
			set
			{
				if (value == m_model.RootPath)
					return;

				m_model.RootPath = value;

				OnModify( nameof( RootFolder ) );

				Scan.FireChange();
			}
		}

		public bool IsModified { get; private set; }

		private CancellationTokenSource m_cancel;

		private void OnModify( string propertyName )
		{
			IsModified = true;

			OnPropertyChange( propertyName );

			OnPropertyChange( nameof( CanSave ) );
			OnPropertyChange( nameof( IsModified ) );
		}

		public ExtensionModel[] Extensions { get; private set; } = { };

		public int NumberOfFiles { get; private set; }

		public int NumberOfLines { get; private set; }

		public Command Scan { get; }

		public AppViewModel()
		{
			Scan = new Command( OnScan, OnCanScan );
		}

		private bool OnCanScan() => !string.IsNullOrEmpty( m_model.RootPath ) && Directory.Exists( m_model.RootPath );

		private async void OnScan()
		{
			if (m_busy)
			{
				m_cancel.Cancel();

				return;
			}

			m_busy = true;

			OnPropertyChange( nameof( CanLoad ) );
			OnPropertyChange( nameof( CanSave ) );
			OnPropertyChange( nameof( CanChange ) );
			OnPropertyChange( nameof( ScanText ) );

			try
			{
				m_cancel = new CancellationTokenSource();

				await m_model.Scan( m_cancel.Token );
			}
			catch (OperationCanceledException)
			{
				// Das dürfen wir!
			}
			finally
			{
				SyncExtensions();

				m_busy = false;

				OnModify( null );
			}
		}

		private void UpdateExtensions()
		{
			m_model.ExcludedExtensions = Extensions.Where( e => e.IsExcluded ).Select( e => e.Name ).ToArray();

			OnModify( nameof( IsModified ) );
		}

		public void Save( string path )
		{
			m_model.Save( path );

			IsModified = false;

			OnPropertyChange( nameof( IsModified ) );
		}

		private void SyncExtensions()
		{
			var excluded = new HashSet<string>( m_model.ExcludedExtensions, StringComparer.InvariantCultureIgnoreCase );

			var files = m_model.RootFolder.GetAllFiles().ToArray();
			var filesMapped = files.GroupBy( f => Path.GetExtension( f.Name ), excluded.Comparer ).ToDictionary( g => g.Key, g => new { Count = g.Count(), Lines = g.Sum( f => f.NumberOfLines ) } );
			var scratch = filesMapped.Values.FirstOrDefault();

			Extensions =
				excluded
					.Concat( filesMapped.Keys )
					.Distinct()
					.OrderBy( e => e )
					.Select( e =>
					{
						filesMapped.TryGetValue( e, out scratch );

						return new ExtensionModel( e, scratch?.Count ?? 0, scratch?.Lines ?? 0, excluded.Contains( e ), this );
					} )
					.ToArray();

			NumberOfFiles = files.Length;
			NumberOfLines = files.Sum( f => f.NumberOfLines );

			OnPropertyChange( nameof( Extensions ) );
			OnPropertyChange( nameof( NumberOfFiles ) );
			OnPropertyChange( nameof( NumberOfLines ) );
		}

		public void Load( string path )
		{
			m_model = Model.Load( path );

			SyncExtensions();

			IsModified = false;

			OnPropertyChange( null );

			Scan.FireChange();
		}
	}
}
