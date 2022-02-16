using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosmosWar.AIScripts;
using CosmosWar.Properties;

namespace CosmosWar
{
    internal static class Define
    {
        public const string GameName = "Cosmos War";

        public const string GameVersion = "1.0";

        public const string GameDescription = "made by Coder_Murphy[1mGames]";

        public const int GameWindowWidth = 800;

        public const int GameWindowHeight = 600;
        public static Brush WhiteBrush => brushWhite;

        public static Brush BlackBrush => brushBlack;
        public static Brush YellowBrush => brushYellow;
        public static Font FontSystem => fontSystem;
        public static Font FontAreaList => fontAreaList;
        public static Font FontYaHei22 => fontYaHei22;
        public static Font FontTitle => fontTitle;
        public static Pen FramePenWhite5 => framePenWhite5;
        public static Pen FramePenWhite2 => framePenWhite2;
        public static Font FontSystem14 => fontSystem14;

        public static Font FontUnitLevel => fontUnitLevel;
        public static Font FontSystem12 => fontSystem12;
        public static Font FontSystem16Bold => fontSystem16Bold;
        public static SizeF TitleSize => titleSize;
        /// <summary>
        /// 单位控制台头像大小
        /// </summary>
        public const int UnitIconSize = 64;

        /// <summary>
        /// 瓦片地图集
        /// </summary>
        public static readonly Dictionary<int, Image> TileMap = new Dictionary<int, Image>
        {
            {0,Resources._000 },
            {1,Resources._001 },
            {2,Resources._002 },
            {3,Resources._003 },
            {4,Resources._004 },
            {5,Resources._005 },
            {6,Resources._006 },
            {7,Resources._007 },
        };

        /// <summary>
        /// 单位图标类别集红方
        /// </summary>
        public static readonly Dictionary<int, Image> UnitTypesImageMapR = new Dictionary<int, Image>
        {
            {0, Resources.UnitR001 },
            {1, Resources.UnitR002 },
            {2, Resources.UnitR003 },
            {3, Resources.UnitR004 },
            {4, Resources.UnitR005 },
            {5, Resources.UnitR006 },
            {6, Resources.UnitR007 },
            {7, Resources.UnitR008 },
            {8, Resources.UnitR009 },
            {9, Resources.UnitR010 },
            {10, Resources.UnitR011 },
            {11, Resources.UnitR012 },
            {12, Resources.UnitR013 },
            {13, Resources.UnitR014 },
            {14, Resources.UnitR015 },
            {15, Resources.UnitR016 },
            {16, Resources.UnitR017 },
            {17, Resources.UnitR018 },
            {18, Resources.UnitR019 },
            {19, Resources.UnitR020 },
            {20, Resources.UnitR021 },
            {21, Resources.UnitR022 },
            {22, Resources.UnitR023 },
            {23, Resources.UnitR024 },
            {24, Resources.UnitR025 },
            {25, Resources.UnitR026 },
            {26, Resources.UnitR027 },
            {27, Resources.UnitR028 },
            {28, Resources.UnitR029 },
            {29, Resources.UnitR030 },
            {30, Resources.UnitR031 },
            {31, Resources.UnitR032 },
            {32, Resources.UnitR033 },//主机
            {96, Resources._098 },//工厂
        };

        /// <summary>
        /// 单位图标类别集蓝方
        /// </summary>
        public static readonly Dictionary<int, Image> UnitTypesImageMapB = new Dictionary<int, Image>
        {
            {0, Resources.UnitB001 },
            {1, Resources.UnitB002 },
            {2, Resources.UnitB003 },
            {3, Resources.UnitB004 },
            {4, Resources.UnitB005 },
            {5, Resources.UnitB006 },
            {6, Resources.UnitB007 },
            {7, Resources.UnitB008 },
            {8, Resources.UnitB009 },
            {9, Resources.UnitB010 },
            {10, Resources.UnitB011 },
            {11, Resources.UnitB012 },
            {12, Resources.UnitB013 },
            {13, Resources.UnitB014 },
            {14, Resources.UnitB015 },
            {15, Resources.UnitB016 },
            {16, Resources.UnitB017 },
            {17, Resources.UnitB018 },
            {18, Resources.UnitB019 },
            {19, Resources.UnitB020 },
            {20, Resources.UnitB021 },
            {21, Resources.UnitB022 },
            {22, Resources.UnitB023 },
            {23, Resources.UnitB024 },
            {24, Resources.UnitB025 },
            {25, Resources.UnitB026 },
            {26, Resources.UnitB027 },
            {27, Resources.UnitB028 },
            {28, Resources.UnitB029 },
            {29, Resources.UnitB030 },
            {30, Resources.UnitB031 },
            {31, Resources.UnitB032 },
            {32, Resources.UnitB033 },// 主机
            {96, Resources._099 },//工厂
        };

        /// <summary>
        /// 获取地图配置
        /// </summary>
        /// <param name="mapName"></param>
        /// <returns></returns>
        public static string GetMapSetting(string mapName)
        {
            if (mapName == "棒旋星系L11区域")
                return Resources.Area1_setting;
            return string.Empty;
        }

        /// <summary>
        /// 声明的单位种类
        /// </summary>
        public static readonly List<Unit> DefineUnitTypes = new List<Unit>
        {
            new Unit(0,"主机",1,1,1,0,1,32),
            new Unit(1,"制造车间",1,1,1,0,1,96),
            new Unit(2,"蓝-型机甲",20,1,10,4,100,8),
            new Unit(3,"叵-型机甲",25,1,11,4,150,1),
            new Unit(4,"钬-型机甲",30,2,10,4,150,2),
            new Unit(5,"旱-型机甲",36,2,12,4,200,10),
            new Unit(6,"钯-型机甲",30,1,13,4,200,9),
            new Unit(7,"狍-型机甲",34,1,12,5,200,11),
            new Unit(8,"傣-型机甲",36,2,14,5,250,3),
            new Unit(9,"库因-型战斗机",32,3,13,6,250,23),
            new Unit(10,"海-型战斗机械",40,1,14,6,300,13),
            new Unit(11,"夏-型战斗飞碟",40,0,15,6,300,3),
            new Unit(12,"西-型防御机甲",60,2,10,4,300,0),
            new Unit(13,"凯-型战斗机",42,3,14,7,350,6),
            new Unit(14,"利-型综合机甲",42,2,16,5,350,12),
            new Unit(15,"铕-型突击机甲",40,0,17,6,350,14),
            new Unit(16,"薩-型鹰式机甲",55,1,16,6,400,15),
            new Unit(17,"杜-型光学机甲",60,2,14,6,400,5),
            new Unit(18,"喀-型榴弹机甲",68,1,15,6,450,7),
            new Unit(19,"杰-型突击机甲",55,0,18,7,450,25),
            new Unit(20,"逑-型突击机甲",60,1,18,7,500,26),
            new Unit(21,"艾-型防御机甲",85,4,12,7,500,24),
            new Unit(22,"雷-型综合机甲",75,3,17,6,550,17),
            new Unit(23,"坦-型炮舰",130,3,12,4,600,22),
            new Unit(24,"麋-型激光舰",110,3,14,4,650,29),
            new Unit(25,"康-型空中堡垒",170,3,12,3,700,28),
            new Unit(26,"佳-型战斗舰",200,1,14,3,750,30),
            new Unit(27,"包-型战斗堡垒",200,4,15,3,800,20),
            new Unit(28,"秦-型战列舰",220,4,16,3,850,21),
            new Unit(29,"司马-型战列舰",240,5,16,3,900,31),
            new Unit(30,"吉-型重炮机甲",80,5,28,7,1000,19),
            new Unit(31,"古-型重型机甲",85,6,26,7,1000,27),
            new Unit(32,"茨-型重型光学机甲",85,6,32,7,1200,4),
            new Unit(33,"提坎-巨型战斗机甲",250,7,22,3,1500,18),
        };

        /// <summary>
        /// 声明所有AI上下文
        /// </summary>
        public static readonly AIBase[] DefineAIContexts = new AIBase[]
        {
            new AICommon("m_AIContext").
            RegRoundBegin(x => x += 500).// 设置AI回合额外福利
            RegAttackEnemyUnitPriority((src,dst) => {
                // 使用距离的倒数使AI攻击最近的敌人
                return 1 / CWMath.GetDistancePow(src.GridLocX,src.GridLocY,dst.GridLocX,dst.GridLocY);
            }).
            RegSelectUnit(units => {
                // 默认找没移动的首个单位进行移动
                return units.FirstOrDefault(x => !x.IsThisRoundMoved);
            }).
            SetFactoryBuildStrategy(AIFactoryBuildStrategy.Normal),// 使用常规工厂策略
        };

    /// <summary>
    /// 跟据声明EX技能
    /// </summary>
    public static readonly Dictionary<int,EXSkill> DefineUnitEXSkills = new Dictionary<int, EXSkill>
        {
            {17,new EXSkill{EXSkillName = "初级HML广域攻击",EXSkillDmgExtra = 10,AttackType = AttackType.Laser} }
        };

        private static Brush brushWhite = new SolidBrush(Color.White);
        private static Brush brushYellow = new SolidBrush(Color.Yellow); 
        private static Brush brushBlack = new SolidBrush(Color.Black);
        private static Font fontYaHei22 = new Font("微软雅黑", 22f);
        private static Font fontSystem = new Font("宋体", 9f);
        private static Font fontSystem14 = new Font("宋体", 14f);
        private static Font fontSystem12 = new Font("宋体", 12f);
        private static Font fontSystem16Bold = new Font("微软雅黑", 16f, FontStyle.Bold);
        private static Font fontTitle = new Font("微软雅黑", titleFontSize);
        private static Font fontAreaList = new Font("宋体", 12f, FontStyle.Bold);
        private static Pen framePenWhite5 = new Pen(Color.White, 5f);
        private static Pen framePenWhite10 = new Pen(Color.White, 10f);
        private static Pen framePenWhite2 = new Pen(Color.White, 2f);
        internal static Pen RectPenWhite = new Pen(Brushes.White);
        internal static Pen RectPenBlack = new Pen(Brushes.Black);
        internal static Pen RectLinePenWhite = new Pen(Color.White, 34f);
        internal static Pen penMoveGrid = new Pen(Color.FromArgb(128, 255, 255, 255), 40f);
        private const float titleFontSize = 40.0f;
        private static SizeF titleSize = new SizeF(titleFontSize * GameName.Length, titleFontSize * 4);
        private static Font fontUnitLevel = new Font("微软雅黑", 9f, FontStyle.Bold);
    }

    /// <summary>
    /// 攻击方式
    /// </summary>
    public enum AttackType
    {
        /// <summary>
        /// 常规攻击
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 火箭炮攻击
        /// </summary>
        Rockets = 1,
        /// <summary>
        /// 先驱者导弹攻击
        /// </summary>
        PionnerMissiles = 2,
        /// <summary>
        /// 激光炮攻击
        /// </summary>
        Laser = 3,
        /// <summary>
        /// 火焰喷射攻击
        /// </summary>
        Fire = 4,
        /// <summary>
        /// 电子束攻击
        /// </summary>
        ElectronBeam = 5,
        /// <summary>
        /// 引力场攻击
        /// </summary>
        GravityBurst = 6,
        /// <summary>
        /// 死光攻击
        /// </summary>
        DeadLight = 7,
        /// <summary>
        /// 大型脉冲束攻击
        /// </summary>
        HugePulse = 8,
    }
}
