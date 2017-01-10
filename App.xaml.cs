using System.Windows;
using System.Windows.Threading;

namespace SourceStructureAnalyser
{
    partial class App
    {
        public App()
        {
            DispatcherUnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception?.Message ?? "Unerwarter Fehler", "Da hat was nicht funktioniert!");

            e.Handled = true;
        }
    }
}
