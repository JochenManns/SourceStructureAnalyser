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

        public AppViewModel()
        {
        }

        public void Save(string path)
            => m_model.Save(path);
    }
}
