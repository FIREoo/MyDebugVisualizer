using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.DebuggerVisualizers;
using System.Windows.Forms;

    [assembly:System.Diagnostics.DebuggerVisualizer(
typeof(MyVisualizer.DebuggerSide),
typeof(VisualizerObjectSource),
Target = typeof(System.String),
Description = "My Visualizer")]

namespace MyVisualizer
{

    public class DebuggerSide : DialogDebuggerVisualizer
    {
        Form1 form1 = new Form1();
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            Form1 form1 = new Form1();// new Form1(objectProvider.GetObject().ToString());
            form1.Show();
            //MessageBox.Show(objectProvider.GetObject().ToString());1
        }

        public static void TestShowVisualizer(object objectToVisualize)
        {
            VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(DebuggerSide));
            visualizerHost.ShowVisualizer();
        }
    }
}
