using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosWar
{
    /// <summary>
    /// 动画
    /// </summary>
    public sealed class Anime
    {
        private Anime() { }

        /// <summary>
        /// 动画结束事件
        /// </summary>
        public static event AnimeFinishedEventHandler AnimeFinished;

        /// <summary>
        /// 获取动画的唯一实例
        /// </summary>
        public static Anime Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new Anime();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// 初始化动画系统
        /// </summary>
        internal void Init()
        {
            Game.SetActionByFrequency(() =>
            {
                // 单位动画
                switch (moveActionProcess)
                {
                    case MoveActionProcess.Moving:
                        if(movePoints.Count > 0)
                        {
                            Point p = movePoints.Dequeue();
                            //Logger.Log($"移动单位{movingUnit}至{p}");
                            movingUnit.GridLocX = (byte)p.X;
                            movingUnit.GridLocY = (byte)p.Y;
                            if(movingUnit.Force == Scene.CurrentAI.Force)
                            {
                                Scene.Instance.FocusTo(movingUnit.GridLocX, movingUnit.GridLocY);
                            }
                        }
                        else
                        {
                            moveActionProcess = MoveActionProcess.MoveEnd;
                        }
                        break;
                    case MoveActionProcess.MoveEnd:
                        //Logger.Log($"移动单位{movingUnit}结束");
                        AnimeFinished?.Invoke(movingUnit, AnimeType.Move);
                        moveActionProcess = MoveActionProcess.NotMove;
                        Game.AllowKeyEvent = true;
                        break;
                }
            }, 20);
        }

        public void MoveUnit(Unit u,byte dstLocX,byte dstLocY)
        {
            // 判断目标区域是否有单位
            var tU = Scene.Instance.FindUnit(dstLocX, dstLocY);
            if (tU != null && !tU.IsBackGround && tU.Force == u.Force && !tU.Equals(u))
            {
                Game.SetWarningMessage("无法移动到该目标位置", 2);
                return;
            }
            Game.WarningMessageDisabled();
            Scene.ShowMoveGrids = false;
            uLocX = u.GridLocX;
            uLocY = u.GridLocY;
            movingUnit = u;
            int maxGridTranslate = CWMath.GetGridLocMaxDelta(uLocX, uLocY, dstLocX, dstLocY, out bool isHorizontal);
            //Console.WriteLine($"移动最大步数:{maxGridTranslate} 移动单位 Name:{u.Name} src:[{uLocX},{uLocY}] dst:[{dstLocX},{dstLocY}]");
            moveActionProcess = MoveActionProcess.Moving;
            Game.AllowKeyEvent = false;
            // 0左 1右 0上 1下
            if(maxGridTranslate > 0)
            {
                int delX = uLocX - dstLocX;
                int delY = uLocY - dstLocY;
                //Console.WriteLine($"执行路径运算 xDel:{delX} yDel:{delY}");
                int dirX = delX <= 0 ? 1 : 0;
                int dirY = delY <= 0 ? 1 : 0;
                int gXDynamic = 0;
                int gYDynamic = 0;
                if (isHorizontal)
                {
                    //Console.WriteLine("水平先");
                    if(dirX == 0)
                    {
                        //Console.WriteLine("向左");
                        for (gXDynamic = uLocX; gXDynamic >= dstLocX; gXDynamic--)
                        {
                            movePoints.Enqueue(new Point(gXDynamic, uLocY));
                        }
                        gXDynamic++;
                    }
                    else if(dirX == 1)
                    {
                        //Console.WriteLine("向右");
                        for (gXDynamic = uLocX; gXDynamic <= dstLocX; gXDynamic++)
                        {
                            movePoints.Enqueue(new Point(gXDynamic, uLocY));
                        }
                        gXDynamic--;
                    }
                    if (dirY == 0)
                    {
                        //Console.WriteLine("向上");
                        for (gYDynamic = uLocY; gYDynamic >= dstLocY; gYDynamic--)
                        {
                            movePoints.Enqueue(new Point(gXDynamic, gYDynamic));
                        }
                    }
                    else if(dirY == 1)
                    {
                        //Console.WriteLine("向下");
                        for (gYDynamic = uLocY; gYDynamic <= dstLocY; gYDynamic++)
                        {
                            movePoints.Enqueue(new Point(gXDynamic, gYDynamic));
                        }
                    }
                }
                else
                {
                    //Console.WriteLine("垂直先");
                    if (dirY == 0)
                    {
                        //Console.WriteLine("向上");
                        for (gYDynamic = uLocY; gYDynamic >= dstLocY; gYDynamic--)
                        {
                            movePoints.Enqueue(new Point(uLocX, gYDynamic));
                        }
                        gYDynamic++;
                    }
                    else if (dirY == 1)
                    {
                        //Console.WriteLine("向下");
                        for (gYDynamic = uLocY; gYDynamic <= dstLocY; gYDynamic++)
                        {
                            movePoints.Enqueue(new Point(uLocX, gYDynamic));
                        }
                        gYDynamic--;
                    }
                    if (dirX == 0)
                    {
                        //Console.WriteLine("向左");
                        for (gXDynamic = uLocX; gXDynamic >= dstLocX; gXDynamic--)
                        {
                            movePoints.Enqueue(new Point(gXDynamic, gYDynamic));
                        }
                    }
                    else if (dirX == 1)
                    {
                        //Console.WriteLine("向右");
                        for (gXDynamic = uLocX; gXDynamic <= dstLocX; gXDynamic++)
                        {
                            movePoints.Enqueue(new Point(gXDynamic, gYDynamic));
                        }
                    }
                }
            }
            else
            {
                movePoints.Enqueue(new Point(uLocX, uLocY));
            }
            Logger.Log($"移动路径个数：{movePoints.Count}");
        }

        private int uLocX = 0;
        private int uLocY = 0;
        private static Anime _instance = null;
        private Unit movingUnit = null;
        private readonly Queue<Point> movePoints = new Queue<Point>();
        public MoveActionProcess moveActionProcess = MoveActionProcess.NotMove;
    }

    /// <summary>
    /// 动画完成委托
    /// </summary>
    /// <param name="dstUnit"></param>
    /// <param name="animeType"></param>
    public delegate void AnimeFinishedEventHandler(Unit dstUnit, AnimeType animeType);

    public enum AnimeType
    {
        /// <summary>
        /// 移动动画
        /// </summary>
        Move,
        Effort,
    }
}
