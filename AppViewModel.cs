using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SourceStructureAnalyser
{
    public class AppViewModel : ViewModel
    {
        private Model m_model = new Model();

        private volatile bool m_busy;

        public string ScanText => m_busy ? "Abbrechen" : "Auswerten";

        public bool CanChange => !m_busy;

        public bool CanLoad => !m_busy;

        public bool CanSave => !m_busy && m_modified;

        public string RootPath
        {
            get { return m_model.RootPath; }
            set
            {
                if (value == m_model.RootPath)
                    return;

                m_model.RootPath = value;

                OnModify(nameof(RootPath));

                Scan.FireChange();
            }
        }

        private FolderViewModel m_rootFolder;

        public FolderViewModel RootFolder => m_rootFolder ?? (m_rootFolder = new FolderViewModel(m_model.RootFolder, this));

        private bool m_modified;

        public bool IsModified
        {
            get { return m_modified; }
            set
            {
                if (value == m_modified)
                    return;

                m_modified = value;

                OnPropertyChange(nameof(IsModified));
                OnPropertyChange(nameof(CanSave));
            }
        }

        private CancellationTokenSource m_cancel;

        private void OnModify(string propertyName)
        {
            OnPropertyChange(propertyName);

            IsModified = true;
        }

        public ExtensionModel[] Extensions { get; private set; } = { };

        public int NumberOfFiles { get; private set; }

        public int NumberOfLines { get; private set; }

        public Command Scan { get; }

        public AppViewModel()
        {
            Scan = new Command(OnScan, OnCanScan);

            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
                Load(args[1]);
        }

        private bool OnCanScan() => !string.IsNullOrEmpty(m_model.RootPath) && Directory.Exists(m_model.RootPath);

        private async void OnScan()
        {
            if (m_busy)
            {
                m_cancel.Cancel();

                return;
            }

            m_busy = true;

            OnPropertyChange(nameof(CanLoad));
            OnPropertyChange(nameof(CanSave));
            OnPropertyChange(nameof(CanChange));
            OnPropertyChange(nameof(ScanText));

            try
            {
                m_cancel = new CancellationTokenSource();

                await m_model.Scan(m_cancel.Token);
            }
            catch (OperationCanceledException)
            {
                // Das dürfen wir!
            }
            finally
            {
                SyncExtensions();

                m_busy = false;

                OnModify(null);
            }
        }

        public void UpdateExtensions()
        {
            m_model.ExcludedExtensions = Extensions.Where(e => e.IsExcluded).Select(e => e.Name).ToArray();

            OnModify(nameof(IsModified));
        }

        public void Save(string path)
        {
            m_model.Save(path);

            IsModified = false;
        }

        public void Export(string path) =>
            m_model.Export(path);

        public static ExtensionModel[] FilesToExtensions(Model.FolderInfo folder, Action<int> reportFileCount, Action<int> reportLineCount, AppViewModel model, IEnumerable<string> allExcluded = null)
        {
            var excluded = new HashSet<string>(allExcluded ?? Enumerable.Empty<string>(), StringComparer.InvariantCultureIgnoreCase);

            var files = folder.GetAllFiles().ToArray();
            var filesMapped = files.GroupBy(f => Path.GetExtension(f.Name), excluded.Comparer).ToDictionary(g => g.Key, g => new { Count = g.Count(), Lines = g.Sum(f => f.NumberOfLines) });
            var scratch = filesMapped.Values.FirstOrDefault();

            reportFileCount(files.Length);
            reportLineCount(files.Sum(f => f.NumberOfLines));

            return
                excluded
                    .Concat(filesMapped.Keys)
                    .Distinct()
                    .OrderBy(e => e)
                    .Select(e =>
                   {
                       filesMapped.TryGetValue(e, out scratch);

                       return new ExtensionModel(e, scratch?.Count ?? 0, scratch?.Lines ?? 0, excluded.Contains(e), model);
                   })
                    .ToArray();

        }

        private void SyncExtensions()
        {
            Extensions = FilesToExtensions(m_model.RootFolder, c => NumberOfFiles = c, c => NumberOfLines = c, this, m_model.ExcludedExtensions);

            m_rootFolder = null;

            OnPropertyChange(nameof(RootFolder));
            OnPropertyChange(nameof(Extensions));
            OnPropertyChange(nameof(NumberOfFiles));
            OnPropertyChange(nameof(NumberOfLines));
        }

        public void Load(string path)
        {
            m_model = Model.Load(path);

            SyncExtensions();

            IsModified = false;

            OnPropertyChange(null);

            Scan.FireChange();
        }

    }
}
