using System.Linq;

namespace SourceStructureAnalyser
{
	public class FolderViewModel : ViewModel
	{
		public bool IsExcluded
		{
			get { return m_folder.IsExcluded; }
			set
			{
				if (value == m_folder.IsExcluded)
					return;

				m_folder.IsExcluded = value;

				OnPropertyChange( nameof( IsExcluded ) );

				m_model.IsModified = true;
			}
		}

		public string Description
		{
			get { return m_folder.Description; }
			set
			{
				if (value == m_folder.Description)
					return;

				m_folder.Description = value;

				OnPropertyChange( nameof( Description ) );

				m_model.IsModified = true;
			}
		}

		public int NumberOfFolders { get; }

		public int NumberOfFiles { get; private set; }

		public int NumberOfLines { get; private set; }

		private readonly Model.FolderInfo m_folder;

		private FolderViewModel[] m_folders;

		public string Name => m_folder.RelativeName;

		public FolderViewModel[] Folders => m_folders ?? (m_folders = m_folder.Folders.Select( f => new FolderViewModel( f, m_model ) ).ToArray());

		private readonly AppViewModel m_model;

		public ExtensionModel[] Extensions { get; }

		public FolderViewModel( Model.FolderInfo folder, AppViewModel model )
		{
			m_folder = folder;
			m_model = model;

			NumberOfFolders = m_folder.GetAllFolders().Count();

			Extensions = AppViewModel.FilesToExtensions( folder, c => NumberOfFiles = c, c => NumberOfLines = c, model );
		}
	}
}