using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosWar
{
    public static class Logger
    {
        public static void Log(object obj)
        {
            //Console.WriteLine(obj?.ToString());
            if (!DebugWindowVisible)
                return;
            loggerActObj = obj;
            Game.GameWindow?.Invoke(loggerAct);
        }

        private static bool gwIsInit = false;

        /// <summary>
        /// 设置调试窗口可见程度
        /// </summary>
        public static bool DebugWindowVisible { get; set; }

        private static Action loggerAct = new Action(() => {
            if (!DebugForm.Instance.Visible && DebugWindowVisible)
                DebugForm.Instance.Show();
            DebugForm.Instance.PutLog(loggerActObj?.ToString());
            if (!gwIsInit)
            {
                gwIsInit = true;
                Game.GameWindow.SizeChanged += (s, e) => DebugForm.Instance.Refresh();
                Game.GameWindow.LocationChanged += (s, e) => DebugForm.Instance.Refresh();
            }
            Game.GameWindow.Select();
        });

        private static object loggerActObj = null;
    }
}
