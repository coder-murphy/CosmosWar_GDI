using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosWar
{
    /// <summary>
    /// 操作类
    /// </summary>
    public class Unit
    {
        /// <summary>
        /// 根据属性新建一个单位
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="life"></param>
        /// <param name="armor"></param>
        /// <param name="damage"></param>
        /// <param name="move"></param>
        /// <param name="cost">建造花费</param>
        public Unit(byte id, string name, int life, int armor, int damage, byte move,int cost,int iconId)
        {
            Id = id;
            Name = name;
            Life = life;
            Armor = armor;
            Damage = damage;
            Move = move;
            Cost = cost;
            Icon = iconId;
            if (Name == "主机")
            {
                IsHome = true; IsBackGround = true;
            }
            else if (Name == "制造车间")
            {
                IsFactory = true; IsBackGround = true;
            }
        }

        /// <summary>
        /// 新建一个单位
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="life"></param>
        /// <param name="armor"></param>
        /// <param name="damage"></param>
        /// <param name="move"></param>
        /// <param name="cost"></param>
        /// <returns></returns>
        public static Unit Create(byte id, string name, int life, int armor, int damage, byte move, int cost,int iconId)
        {
            return new Unit(id, name, life, armor, damage, move, cost,iconId);
        }

        /// <summary>
        /// 根据单位克隆一个单位
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static Unit Clone(Unit u)
        {
            if (u == null)return null;
            var nu = new Unit(u.Id, u.Name, u.Life, u.Armor, u.Damage, u.Move, u.Cost, u.Icon);
            nu.Force = u.Force;
            nu.EXSkill = u.EXSkill;
            return nu;
        }

        public byte Id { get; set; }

        public string Name { get; set; }

        public byte GridLocX { get; set; }

        public byte GridLocY { get; set; }

        public byte ScreenGridLocX { get; set; }

        public byte ScreenGridLocY { get; set; }

        public int Life { get; set; }

        public int Armor { get; set; }

        public int Damage { get; set; }

        /// <summary>
        /// 图标id
        /// </summary>
        public int Icon { get; set; }

        /// <summary>
        /// 是否为背景(可通行)
        /// </summary>
        public bool IsBackGround { get; set; }

        /// <summary>
        /// 能移动的格子数
        /// </summary>
        public byte Move { get; set; }

        /// <summary>
        /// 花费
        /// </summary>
        public int Cost { get; set; }

        /// <summary>
        /// 阵营
        /// </summary>
        public string Force { get; set; }

        /// <summary>
        /// 是否为家
        /// </summary>
        public bool IsHome { get; set; }

        /// <summary>
        /// 是否为工厂
        /// </summary>
        public bool IsFactory { get; set; }

        /// <summary>
        /// 终极技能
        /// </summary>
        public EXSkill EXSkill
        {
            get
            {
                if(defEx == null)
                    defEx = EXSkill.None;
                return defEx;
            }
            set
            {
                defEx = value;
            }
        }
        private EXSkill defEx = null;

        /// <summary>
        /// 当前回合是否移动结束
        /// </summary>
        public bool IsThisRoundMoved { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public int Level
        {
            get => level;
            set
            {
                if (value < 1)
                    level = 1;
                else
                    level = value;
            }
        }

        private int level = 1;
    }
}
