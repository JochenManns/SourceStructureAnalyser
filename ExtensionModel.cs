namespace SourceStructureAnalyser
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
}