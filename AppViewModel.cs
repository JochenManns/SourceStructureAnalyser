using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceStructureAnalyser
{
	public class AppViewModel : ViewModel
	{
		private Model m_model = new Model();

		public string RootFolder
		{
			get { return m_model.RootFolder; }
			set
			{
				if (value == m_model.RootFolder)
					return;

				m_model.RootFolder = value;

				OnModify( nameof( RootFolder ) );
			}
		}

		private bool m_modified;

		public bool IsModified => m_modified;

		private void OnModify( string propertyName )
		{
			m_modified = true;

			OnPropertyChange( propertyName );
			OnPropertyChange( nameof( IsModified ) );
		}

		public AppViewModel()
		{
		}

		public void Save( string path )
		{
			m_model.Save( path );

			m_modified = false;

			OnPropertyChange( nameof( IsModified ) );
		}

		public void Load( string path )
			=> m_model = Model.Load( path );
	}
}
