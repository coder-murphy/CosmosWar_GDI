using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CosmosWar
{
    /// <summary>
    /// 地区选择
    /// </summary>
    public static class AreaPanel
    {

        public static byte CurrentAreaIndex => _currentAreaIndex;

        public static void Show()
        {
            //Image img = Properties.Resources.BackGround;
            //Graphics g = Graphics.FromImage(img);
            Game.CurrentScene = GameScene.AreaList;
            Size size = Game.ClientSize;
            Graphics g = Game.GetGraphics();
            g.DrawString("地区选择", Define.FontTitle, Define.WhiteBrush, new RectangleF((size.Width - 280f) / 2, 10f, 280f, 70f));
            g.DrawLine(Define.FramePenWhite5, 10, 100, size.Width - 10, 100);
            g.DrawLine(Define.FramePenWhite5, size.Width - 10, 100, size.Width - 10, size.Height - 10);
            g.DrawLine(Define.FramePenWhite5, 10, 100, 10, size.Height - 10);
            g.DrawLine(Define.FramePenWhite5, 10, size.Height - 10, size.Width - 10, size.Height - 10);
            float delta = listTop;
            float listElemLeft = (size.Width - 200f) / 2;
            RectangleF listRect = new RectangleF(listElemLeft, delta, 200f, 40f);
            foreach (var area in AreaList)
            {
                g.DrawString(area, Define.FontSystem16Bold, Define.WhiteBrush, listRect);
                delta += 70f;
                listRect.Location = new PointF(listElemLeft, delta);
            }
            g.DrawString("◆", Define.FontSystem16Bold, Define.WhiteBrush,
                new RectangleF(
                    listElemLeft - 60f, listTop + 70f * _currentAreaIndex,
                    40f, 40f));
            g.DrawString("Press key [Esc] back to mainmenu.", Define.FontSystem,
                new SolidBrush(Color.White), size.Width - 260f, size.Height - 30f);
            //Game.GameWindow.BackgroundImage = img;
            //Logger.Log("配置图片");
        }

        public static readonly List<string> AreaList = new List<string>
        {
            "棒旋星系L11区域",
            "大麦哲伦云B7区域",
            "稀疏星云G7区域",
            "天琴座M8区域"
        };

        internal static void GetKeyDown(Keys keyCode,out bool renderFlag)
        {
            switch (keyCode)
            {
                case Keys.F:
                    if (_currentAreaIndex < 3)
                    {
                        _currentAreaIndex++;
                    }
                    else
                    {
                        _currentAreaIndex = 0;
                    }
                    renderFlag = true;
                    break;
                //case Keys.Down:
                //    if(_currentAreaIndex == 3)
                //        renderFlag = false;
                //    else
                //    {
                //        _currentAreaIndex++;
                //        renderFlag = true;
                //    }
                //    break;
                case Keys.Escape:
                    Game.CurrentScene = GameScene.Entry;
                    Game.PlayBGM(GameSound.Intro);
                    renderFlag = true;
                    break;
                case Keys.Enter:// 选择地图
                    // 省略资源配置
                    Game.CurrentScene = GameScene.Scene;
                    Scene.Instance.LoadMap(Game.FindMap(CurrentAreaIndex));
                    renderFlag = true;
                    break;
                default:
                    renderFlag = false;
                    break;
            }
        }

        private static byte _currentAreaIndex;
        private const float listTop = 160f;
    }
}
