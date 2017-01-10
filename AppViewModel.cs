using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceStructureAnalyser
{
    public class AppViewModel : ViewModel
    {
        public Command Load { get; }

        public Command Save { get; }

        public AppViewModel()
        {
            Load = new Command(OnLoad);
            Save = new Command(OnSave);
        }

        private void OnLoad()
        {
        }

        private void OnSave()
        {
        }
    }
}
