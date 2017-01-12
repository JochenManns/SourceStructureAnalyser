using System;
using System.Drawing;
using System.Linq;
using System.Windows.Media;

namespace SourceStructureAnalyser
{
	public class FolderViewModel : ViewModel
	{
		private static readonly Model.FolderColors[] _Colors = (Model.FolderColors[]) Enum.GetValues( typeof( Model.FolderColors ) );

		public Model.FolderColors[] Colors => _Colors;

		public Model.FolderColors Color
		{
			get { return m_folder.Color; }
			set
			{
				if (value == m_folder.Color)
					return;

				m_folder.Color = value;

				OnPropertyChange( nameof( Color ) );
				OnPropertyChange( nameof( UiColor ) );

				m_model.IsModified = true;
			}
		}

		public Brush UiColor
		{
			get
			{
				switch (Color)
				{
					case Model.FolderColors.Grün:
						return new SolidColorBrush( System.Windows.Media.Color.FromRgb( 128, 128, 0 ) );
					case Model.FolderColors.Orange:
						return new SolidColorBrush( System.Windows.Media.Color.FromRgb( 255, 128, 0 ) );
					case Model.FolderColors.Rot:
						return new SolidColorBrush( System.Windows.Media.Color.FromRgb( 255, 0, 0 ) );
					default:
						return new SolidColorBrush( System.Windows.Media.Colors.Black );
				}
			}
		}

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