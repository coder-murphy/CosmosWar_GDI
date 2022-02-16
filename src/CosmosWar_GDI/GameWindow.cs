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
    public partial class GameWindow : Form
    {
        public GameWindow()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnLoad(EventArgs e)
        {
            Game.GameInit(Handle);
            Game.FPSEnabled = true;
            base.OnLoad(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Game.Render();
            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            Game.Render();
            base.OnResize(e);
        }

    }
}
