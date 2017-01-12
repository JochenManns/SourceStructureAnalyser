using System;
using System.Windows;
using Microsoft.Win32;

namespace SourceStructureAnalyser
{
	partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private AppViewModel ViewModel => (AppViewModel) DataContext;

		private void OnSave( object sender, RoutedEventArgs e )
		{
			var dialog = new SaveFileDialog
			{
				Filter = "Konfigurationsdateien|*.ssa|Alle Dateien|*.*",
				FileName = Properties.Settings.Default.LastConfigPath,
				Title = "Konfigurationsdatei speichern",
				OverwritePrompt = true,
				CheckPathExists = true,
				AddExtension = true,
				DefaultExt = "ssa"
			};

			if (dialog.ShowDialog( this ) != true)
				return;

			ViewModel.Save( dialog.FileName );

			Properties.Settings.Default.LastConfigPath = dialog.FileName;
			Properties.Settings.Default.Save();
		}

		private void OnLoad( object sender, RoutedEventArgs e )
		{
			if (ViewModel.IsModified)
				if (MessageBox.Show( "Änderungen verwerfen?", "Modell laden", MessageBoxButton.YesNo ) != MessageBoxResult.Yes)
					return;

			var dialog = new OpenFileDialog
			{
				Filter = "Konfigurationsdateien|*.ssa|Alle Dateien|*.*",
				FileName = Properties.Settings.Default.LastConfigPath,
				Title = "Konfigurationsdatei laden",
				CheckFileExists = true,
				AddExtension = true,
				DefaultExt = "ssa"
			};

			if (dialog.ShowDialog( this ) != true)
				return;

			ViewModel.Load( dialog.FileName );

			Properties.Settings.Default.LastConfigPath = dialog.FileName;
			Properties.Settings.Default.Save();
		}

		private void OnSelectFolder( object sender, RoutedEventArgs e )
		{
			var dialog = new System.Windows.Forms.FolderBrowserDialog
			{
				Description = "Bitte das Wurzelverzeichnis der Quellen auswählen",
				RootFolder = Environment.SpecialFolder.Desktop,
				SelectedPath = ViewModel.RootPath,
				ShowNewFolderButton = false
			};

			using (dialog)
				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					ViewModel.RootPath = dialog.SelectedPath;
		}
	}
}
