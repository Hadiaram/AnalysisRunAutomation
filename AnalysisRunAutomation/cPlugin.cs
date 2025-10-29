using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ETABSv1;

namespace ETABS_Plugin
{
    public class cPlugin
    {
        public void Main (cSapModel SapModel, cPluginCallback ISapPlugin)
        {
            Form1 form = new Form1(SapModel, ISapPlugin);
            form.Show();
        }

        public long Info(string Text)
        {
            Text = "ETABS plugin template";
            return 0;
        }
    }
}
