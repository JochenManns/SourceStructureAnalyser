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
		private Model m_model = new Model();

		private volatile bool m_busy;

		public string ScanText => m_busy ? "Abbrechen" : "Auswerten";

		public bool CanChange => !m_busy;

		public bool CanLoad => !m_busy;

		public bool CanSave => !m_busy && IsModified;

		public string RootFolder
		{
			get { return m_model.RootFolder; }
			set
			{
				if (value == m_model.RootFolder)
					return;

				m_model.RootFolder = value;

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
			OnPropertyChange( nameof( IsModified ) );
		}

		public Command Scan { get; }

		public AppViewModel()
		{
			Scan = new Command( OnScan, OnCanScan );
		}

		private bool OnCanScan() => !string.IsNullOrEmpty( m_model.RootFolder ) && Directory.Exists( m_model.RootFolder );

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
				m_busy = false;

				OnModify( null );
			}
		}


		public void Save( string path )
		{
			m_model.Save( path );

			IsModified = false;

			OnPropertyChange( nameof( IsModified ) );
		}

		public void Load( string path )
		{
			m_model = Model.Load( path );

			IsModified = false;

			OnPropertyChange( null );

			Scan.FireChange();
		}
	}
}
