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
                if (_instance == null)
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
            electronBeamPenWhite = new Pen(solidBrushWhiteA80, 10);
            electronBeamPenDarkAqua = new Pen(solidDarkAquaA60, 10);
            laserPenOrange = new Pen(solidOrangeA60, 5);
            laserPenLightYellow = new Pen(solidLightYellowA80, 5);
            rocketPenSilver = new Pen(solidSilverA100, 10);
            Game.SetActionByFrequency(() =>
            {
                // 单位动画
                switch (moveActionProcess)
                {
                    case MoveActionProcess.Moving:
                        if (movePoints.Count > 0)
                        {
                            Point p = movePoints.Dequeue();
                            //Logger.Log($"移动单位{movingUnit}至{p}");
                            movingUnit.GridLocX = (byte)p.X;
                            movingUnit.GridLocY = (byte)p.Y;
                            if (movingUnit.Force == Scene.CurrentAI.Force)
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
                        AnimeFinished?.Invoke(new AnimeFinishedEventArgs
                        {
                            SrcUnit = movingUnit,
                            DstUnit = movingUnit,
                            AnimeType = AnimeTypes.Move,
                        });
                        moveActionProcess = MoveActionProcess.NotMove;
                        Game.AllowKeyEvent = true;
                        break;
                }
            }, 20);
            // EX技能动画
            Game.SetActionByFrequency(() =>
            {
                switch (eXSkillCastProcess)
                {
                    case EXSkillCastProcess.Casting:
                        if (exSkillType == AttackTypes.ElectronBeam)
                        {
                            if (electronBeamProcess == 0)
                            {
                                electronBeamWidth += 4;
                                if (electronBeamWidth >= electronBeamMaxWidth)
                                {
                                    electronBeamProcess = 1;
                                }
                            }
                            else if (electronBeamProcess == 1)
                            {
                                electronBeamWidth -= 4;
                                if (electronBeamWidth <= 10)
                                {
                                    electronBeamWidth = 10;
                                    electronBeamProcess = 2;
                                }
                            }
                            else if (electronBeamProcess == 2)
                            {
                                electronBeamWidth = electronBeamWidth < 10 ? 10 : --electronBeamWidth;
                                eXSkillCastProcess = EXSkillCastProcess.CastingEnd;
                            }
                        }
                        else if(exSkillType == AttackTypes.Rockets)
                        {
                            if (rocketGunProcess == 0)
                            {
                                rocketScrX += rocketSpeed;
                                rocketScrY += (float)(rocketSpeed * slope);
                                if (rocketScrX > pDst.X - (Scene.ScreenTileSize / 2) || rocketScrY > pDst.Y - (Scene.ScreenTileSize / 2))
                                {
                                    rocketGunProcess = 1;
                                }
                            }
                            else if (rocketGunProcess == 1)
                            {
                                //if (rocketExplosionRadius >= rocketExplosionRadiusMax)
                                //{
                                //    rocketGunProcess = 2;
                                //}
                                //rocketExplosionRadius += 3;
                                rocketGunProcess = 2;
                            }
                            else if (rocketGunProcess == 2)
                            {
                                eXSkillCastProcess = EXSkillCastProcess.CastingEnd;
                            }
                        }
                        else // 其他默认激光
                        {
                            if (laserProcess == 0)
                            {
                                laserWidth += 5;
                                if (laserWidth >= laserMaxWidth)
                                {
                                    laserProcess = 1;
                                }
                            }
                            else if (laserProcess == 1)
                            {
                                laserWidth -= 4;
                                if (laserWidth <= 10)
                                {
                                    laserWidth = 10;
                                    laserProcess = 2;
                                }
                            }
                            else if (laserProcess == 2)
                            {
                                laserWidth = laserWidth < 10 ? 10 : --laserWidth;
                                eXSkillCastProcess = EXSkillCastProcess.CastingEnd;
                            }
                        }
                        break;
                    case EXSkillCastProcess.CastingEnd:
                        electronBeamProcess = 0;
                        laserProcess = 0;
                        rocketGunProcess = 0;
                        AnimeFinished?.Invoke(new AnimeFinishedEventArgs
                        {
                            SrcUnit = castingUnit,
                            DstUnit = castTargetUnit,
                            AnimeType = AnimeTypes.Cast
                        });
                        Scene.Instance.SetSceneGraphicsAction(null, false);
                        eXSkillCastProcess = EXSkillCastProcess.NotCast;
                        Game.AllowKeyEvent = true;
                        Console.WriteLine($"{castingUnit.Name},{castTargetUnit.Name}");
                        break;
                }
            }, 25);
        }

        public void MoveUnit(Unit u, byte dstLocX, byte dstLocY)
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
            uMoveLocX = u.GridLocX;
            uMoveLocY = u.GridLocY;
            movingUnit = u;
            int maxGridTranslate = CWMath.GetGridLocMaxDelta(uMoveLocX, uMoveLocY, dstLocX, dstLocY, out bool isHorizontal);
            //Console.WriteLine($"移动最大步数:{maxGridTranslate} 移动单位 Name:{u.Name} src:[{uLocX},{uLocY}] dst:[{dstLocX},{dstLocY}]");
            moveActionProcess = MoveActionProcess.Moving;
            Game.AllowKeyEvent = false;
            // 0左 1右 0上 1下
            if (maxGridTranslate > 0)
            {
                int delX = uMoveLocX - dstLocX;
                int delY = uMoveLocY - dstLocY;
                //Console.WriteLine($"执行路径运算 xDel:{delX} yDel:{delY}");
                int dirX = delX <= 0 ? 1 : 0;
                int dirY = delY <= 0 ? 1 : 0;
                int gXDynamic = 0;
                int gYDynamic = 0;
                if (isHorizontal)
                {
                    //Console.WriteLine("水平先");
                    if (dirX == 0)
                    {
                        //Console.WriteLine("向左");
                        for (gXDynamic = uMoveLocX; gXDynamic >= dstLocX; gXDynamic--)
                        {
                            movePoints.Enqueue(new Point(gXDynamic, uMoveLocY));
                        }
                        gXDynamic++;
                    }
                    else if (dirX == 1)
                    {
                        //Console.WriteLine("向右");
                        for (gXDynamic = uMoveLocX; gXDynamic <= dstLocX; gXDynamic++)
                        {
                            movePoints.Enqueue(new Point(gXDynamic, uMoveLocY));
                        }
                        gXDynamic--;
                    }
                    if (dirY == 0)
                    {
                        //Console.WriteLine("向上");
                        for (gYDynamic = uMoveLocY; gYDynamic >= dstLocY; gYDynamic--)
                        {
                            movePoints.Enqueue(new Point(gXDynamic, gYDynamic));
                        }
                    }
                    else if (dirY == 1)
                    {
                        //Console.WriteLine("向下");
                        for (gYDynamic = uMoveLocY; gYDynamic <= dstLocY; gYDynamic++)
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
                        for (gYDynamic = uMoveLocY; gYDynamic >= dstLocY; gYDynamic--)
                        {
                            movePoints.Enqueue(new Point(uMoveLocX, gYDynamic));
                        }
                        gYDynamic++;
                    }
                    else if (dirY == 1)
                    {
                        //Console.WriteLine("向下");
                        for (gYDynamic = uMoveLocY; gYDynamic <= dstLocY; gYDynamic++)
                        {
                            movePoints.Enqueue(new Point(uMoveLocX, gYDynamic));
                        }
                        gYDynamic--;
                    }
                    if (dirX == 0)
                    {
                        //Console.WriteLine("向左");
                        for (gXDynamic = uMoveLocX; gXDynamic >= dstLocX; gXDynamic--)
                        {
                            movePoints.Enqueue(new Point(gXDynamic, gYDynamic));
                        }
                    }
                    else if (dirX == 1)
                    {
                        //Console.WriteLine("向右");
                        for (gXDynamic = uMoveLocX; gXDynamic <= dstLocX; gXDynamic++)
                        {
                            movePoints.Enqueue(new Point(gXDynamic, gYDynamic));
                        }
                    }
                }
            }
            else
            {
                movePoints.Enqueue(new Point(uMoveLocX, uMoveLocY));
            }
            Logger.Log($"移动路径个数：{movePoints.Count}");
        }

        /// <summary>
        /// 释放EX技能
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void CastEXSkill(Unit src, Unit dst)
        {
            Game.AllowKeyEvent = false;
            castingUnit = src;
            castTargetUnit = dst;
            exSkillType = castingUnit.EXSkill.AttackType;
            eXSkillCastProcess = EXSkillCastProcess.Casting;
            switch (exSkillType)
            {
                case AttackTypes.ElectronBeam:
                    Scene.Instance.SetSceneGraphicsAction(g =>
                    {
                        Console.WriteLine($"激光宽度：{electronBeamWidth}");
                        var sP = Scene.Instance.GetUnitScreenLoc(castingUnit.GridLocX, castingUnit.GridLocY);
                        var dP = Scene.Instance.GetUnitScreenLoc(castTargetUnit.GridLocX, castTargetUnit.GridLocY);
                        Console.WriteLine($"起始点：{sP.X},{sP.Y} 目标点：{ dP.X}，{dP.Y}");
                        electronBeamPenWhite.Width = electronBeamWidth;
                        electronBeamPenDarkAqua.Width = electronBeamWidth + 8;
                        g.DrawLine(electronBeamPenDarkAqua, sP.X, sP.Y, dP.X, dP.Y);
                        g.DrawLine(electronBeamPenWhite, sP.X, sP.Y, dP.X, dP.Y);
                        //g.DrawLine(laserPen, 10, 10, 120, 120);
                    });
                    break;
                case AttackTypes.Rockets:
                    //Console.WriteLine($"发射角：{launchAngle} 初始炮弹位置{pSrc}");
                    pSrc = Scene.Instance.GetUnitScreenLoc(castingUnit.GridLocX, castingUnit.GridLocY);
                    pDst = Scene.Instance.GetUnitScreenLoc(castTargetUnit.GridLocX, castTargetUnit.GridLocY);
                    launchAngle = (float)CWMath.GetAngleBetweenPoints(pSrc.X, pSrc.Y, pDst.X, pDst.Y);
                    rocketScrX = pSrc.X;
                    rocketScrY = pSrc.Y;
                    slope = CWMath.GetAbsSlope(pSrc.X, pSrc.Y, pDst.X, pDst.Y);
                    Scene.Instance.SetSceneGraphicsAction(g =>
                    {
                        g.FillEllipse(solidSilverA100, rocketScrX - 10, rocketScrY - 10, 20, 20);
                        g.FillEllipse(solidOrangeA60, rocketScrX - 14, rocketScrY - 14, 28, 28);
                        Console.WriteLine($"火箭坐标：{rocketScrX}，{rocketScrY}");
                    });
                    break;
                default:  //默认激光效果
                    Scene.Instance.SetSceneGraphicsAction(g =>
                    {
                        var sP = Scene.Instance.GetUnitScreenLoc(castingUnit.GridLocX, castingUnit.GridLocY);
                        var dP = Scene.Instance.GetUnitScreenLoc(castTargetUnit.GridLocX, castTargetUnit.GridLocY);
                        laserPenLightYellow.Width = electronBeamWidth;
                        laserPenOrange.Width = electronBeamWidth + 5;
                        g.DrawLine(laserPenOrange, sP.X, sP.Y, dP.X, dP.Y);
                        g.DrawLine(laserPenLightYellow, sP.X, sP.Y, dP.X, dP.Y);
                    });
                    break;
            }
        }

        private int uMoveLocX = 0;
        private int uMoveLocY = 0;
        private static Anime _instance = null;
        private Unit movingUnit = null;
        private Unit castingUnit = null;
        private Unit castTargetUnit = null;
        private readonly Queue<Point> movePoints = new Queue<Point>();
        private readonly Queue<Point> effectMovePoints = new Queue<Point>();
        public MoveActionProcess moveActionProcess = MoveActionProcess.NotMove;
        public EXSkillCastProcess eXSkillCastProcess = EXSkillCastProcess.NotCast;
        private AttackTypes exSkillType = AttackTypes.Normal;
        // special members(for skill)
        private double slope; // 斜率
        PointF pSrc;
        PointF pDst;
        // 激光
        private int laserProcess = 0; // 激光阶段
        private int laserWidth = 5;
        private int laserMaxWidth = 20;
        // 电子束
        private int electronBeamProcess = 0; // 电子束阶段
        private int electronBeamWidth = 10;
        private int electronBeamMaxWidth = 35;
        // 火箭炮
        private int rocketGunProcess = 0;
        private int rocketExplosionRadius = 10;
        private int rocketExplosionRadiusMax = 60;
        private float rocketScrX = 0;
        private float rocketScrY = 0;
        private float rocketSpeed = 5;
        private float launchAngle = 0;
        // 特效

        private Pen electronBeamPenWhite = null;
        private Pen electronBeamPenDarkAqua = null;
        private Pen laserPenOrange = null;
        private Pen laserPenLightYellow = null;
        private Pen rocketPenSilver = null;
        private SolidBrush solidBrushWhiteA80 = new SolidBrush(Color.FromArgb(200, 255, 255, 255));
        private SolidBrush solidDarkAquaA60 = new SolidBrush(Color.FromArgb(150, 152, 245, 255));
        private SolidBrush solidOrangeA60 = new SolidBrush(Color.FromArgb(150, 255, 99, 71));
        private SolidBrush solidLightYellowA80 = new SolidBrush(Color.FromArgb(200, 255, 255, 224));
        private SolidBrush solidSilverA100 = new SolidBrush(Color.FromArgb(255, 232 ,232 ,232));
    }

    public class AnimeFinishedEventArgs : EventArgs
    {
        public Unit SrcUnit { get; set; }

        public Unit DstUnit { get; set; }

        public AnimeTypes AnimeType { get; set; }
    }

    /// <summary>
    /// 动画完成委托
    /// </summary>
    /// <param name="eventArgs"></param>
    public delegate void AnimeFinishedEventHandler(AnimeFinishedEventArgs eventArgs);

    public enum AnimeTypes
    {
        /// <summary>
        /// 移动动画
        /// </summary>
        Move,
        /// <summary>
        /// 释放技能动画
        /// </summary>
        Cast,
    }

    /// <summary>
    /// 移动过程标志位
    /// </summary>
    public enum MoveActionProcess
    {
        NotMove = -1,
        Moving = 0,
        MoveEnd = 1,
    }

    /// <summary>
    /// 释放EX技能过程标志位
    /// </summary>
    public enum EXSkillCastProcess
    {
        NotCast = -1,
        Casting = 0,
        CastingEnd = 1,
    }
}
