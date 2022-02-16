using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CosmosWar
{
    public partial class DebugForm : Form
    {
        public DebugForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            LocationChanged += (s, e) => Refresh();
            SizeChanged +=(s ,e) => Refresh();
        }

        public static DebugForm Instance
        {
            get
            {
                if (instance == null || instance.IsDisposed)
                    instance = new DebugForm();
                return instance;
            }
        }

        public void PutLog(string text)
        {
            list.Add(text);
            textBox1.Lines = list.ToArray();
            Refresh();
        }

        private static DebugForm instance;
        private List<string> list = new List<string>();
    }
}
