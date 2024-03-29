﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosWar
{
    /// <summary>
    /// 数学类
    /// </summary>
    public static class CWMath
    {
        /// <summary>
        /// 获取距离的平方
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        public static double GetDistancePow(int x0, int y0, int x1, int y1)
        {
            return Math.Pow(x1 - x0, 2) * Math.Pow(y1 - y0, 2);
        }

        /// <summary>
        /// 计算两个坐标之间的网格路线距离
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        public static int GetGridRouteLen(int x0, int y0, int x1, int y1)
        {
            return Math.Abs(x0 - x1) + Math.Abs(y0 - y1);
        }

        /// <summary>
        /// 获取网格坐标x坐标之差与y坐标之差之中的最大一个
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="isHorizontal">是否为水平</param>
        /// <returns></returns>
        public static int GetGridLocMaxDelta(int x0,int y0,int x1,int y1,out bool isHorizontal)
        {
            int dX = Math.Abs(x0 - x1);
            int dY = Math.Abs(y0 - y1);
            if(dX >= dY)
                isHorizontal = true;
            else
                isHorizontal = false;
            return dX >= dY ? dX : dY;
        }

        /// <summary>
        /// 根据单位行动力获取所有单位能到达的格子
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static List<Point> GetUnitRoundTiles(Unit u)
        {
            return GetUnitRoundTiles(u.GridLocX, u.GridLocY, u.Move);
        }

        /// <summary>
        /// 获取单位周围指定格子范围内的格子坐标(不包含中间单位)
        /// </summary>
        /// <param name="u"></param>
        /// <param name="gridNum"></param>
        /// <returns></returns>
        public static List<Point> GetUnitRoundTilesByGridNum(Unit u, byte gridNum = 1)
        {
            List<Point> points = new List<Point>();
            //Console.WriteLine($"最大边界X:{Scene.SceneGridLeft + Scene.ScreenGridMaxWidth}");
            //Console.WriteLine($"最大边界Y:{Scene.SceneGridTop + Scene.ScreenGridMaxHeight}");
            for (int i = u.GridLocX - gridNum; i <= u.GridLocX + gridNum; i++)
            {
                if (i < 0 || i >= Scene.SceneGridLeft + Scene.ScreenGridMaxWidth) continue;
                for (int j = u.GridLocY - gridNum; j <= u.GridLocY + gridNum; j++)
                {
                    if (j < 0 || j >= Scene.SceneGridTop + Scene.ScreenGridMaxHeight) continue;
                    if (i == u.GridLocX && j == u.GridLocY) continue;
                    Console.WriteLine($"可用格子{i},{j}");
                    IEnumerable<Unit> fUnits = Scene.Instance.FindUnits((byte)i, (byte)j);
                    if (fUnits.Where(x => !x.IsHome && !x.IsFactory).Count() > 0)
                    {
                        Console.WriteLine($"{fUnits.Count()},{fUnits.First().Name}");
                        continue;
                    }
                    points.Add(new Point(i, j));
                }
            }
            Console.WriteLine($"可用格子数：{points.Count}");
            return points;
        }

        /// <summary>
        /// 获取可移动区域中与目标点最近的位置
        /// </summary>
        /// <param name="moveGrids"></param>
        /// <param name="gridLocX"></param>
        /// <param name="gridLocY"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static Point GetNearLocInMoveGrids(IEnumerable<Point> moveGrids, byte gridLocX, byte gridLocY)
        {
            if (moveGrids.Count() == 0) return default;
            return  moveGrids.OrderBy(x => Math.Abs(x.X - gridLocX) + Math.Abs(x.Y - gridLocY)).First();
        }

        /// <summary>
        /// 根据EX技能获取范围内所有敌方单位
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static IEnumerable<Unit> GetEnemysByEXSkillCastRange(Unit unit)
        {
            if(unit == null || unit.EXSkill.Equals(EXSkill.None))
                return Enumerable.Empty<Unit>();
            EXSkill ex = unit.EXSkill;
            var tiles = GetUnitRoundTiles(unit.GridLocX, unit.GridLocY, ex.AttackRange);
            List<Unit> outputs = new List<Unit>();
            foreach(var tile in tiles)
            {
                var tempUnits = Scene.Instance.FindUnits((byte)tile.X, (byte)tile.Y);
                if (tempUnits.Count() == 0)
                    continue;
                var tempUnit = tempUnits.FirstOrDefault(x => !x.IsHome && !x.IsFactory && x.Force != unit.Force);
                if (tempUnit == null)
                    continue;
                outputs.Add(tempUnit);
            }
            return outputs;
        }

        /// <summary>
        /// 根据基础攻击
        /// </summary>
        /// <param name="baseDamage"></param>
        /// <param name="acRatio"></param>
        /// <param name="ownUnitLv"></param>
        /// <param name="enemyDefense">敌方防御力</param>
        /// <param name="enemyUnitLv">敌方单位等级</param>
        /// <returns></returns>
        public static int GetDamage(int baseDamage,int enemyDefense,float acRatio = 0.8f,int ownUnitLv = 1,int enemyUnitLv = 1)
        {
            int dmg = 0;
            if(random.NextDouble() <= acRatio + ownUnitLv * 0.01 - enemyUnitLv * 0.01)
            {
                dmg = (int)Math.Round(baseDamage * (1 + ownUnitLv * 0.025) + random.Next(-ownUnitLv, ownUnitLv)) - enemyDefense;
            }
            dmg = dmg < 0 ? 0 : dmg;
            return dmg;
        }

        /// <summary>
        /// 根据坐标和行动点数（范围）获取可移动格子
        /// </summary>
        /// <param name="gX"></param>
        /// <param name="gY"></param>
        /// <param name="move"></param>
        /// <returns></returns>
        public static List<Point> GetUnitRoundTiles(byte gX, byte gY, int move)
        {
            List<Point> points = new List<Point>();
            Size size = Scene.CurrentMapSize;
            byte range = (byte)(move * 2 + 1);
            byte maxX = (byte)(gX + range);
            byte maxY = (byte)(gY + range);
            for (int i = gX - move; i < maxX; i++)
            {
                if (i < 0 || i >= size.Width) continue;
                for (int j = gY - move; j < maxY; j++)
                {
                    if (j < 0 || j >= size.Height) continue;
                    //if (Math.Abs(i - gX) + Math.Abs(j - gY) > u.Move) continue;
                    if (GetGridRouteLen(i, j, gX, gY) > move) continue;
                    points.Add(new Point(i, j));
                }
            }
            return points;
        }

        /// <summary>
        /// 获取两点间角度
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        public static double GetAngleBetweenPoints(float x0,float y0,float x1,float y1)
        {
            return Math.Atan2(y0 - y1, x1 - x0);
        }

        /// <summary>
        /// 获取绝对坐标比值
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        public static double GetAbsSlope(float x0,float y0,float x1,float y1)
        {
            return Math.Abs(y1 - y0) / Math.Abs(x1 - x0);
        }

        private static Random random = new Random();
    }
}
