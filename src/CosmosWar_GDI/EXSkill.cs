using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosWar
{
    /// <summary>
    /// EX技能
    /// </summary>
    public class EXSkill : IEquatable<EXSkill>
    {
        public EXSkill() : this(x => {
            x.AttackType = AttackTypes.Normal;
            x.EXSkillName = "Nothing";
            x.EXSkillDesc = "No Desc";
            x.EXSkillDmgExtra = 0;
        })
        {

        }

        public EXSkill(Action<EXSkill> initializer)
        {
            initializer?.Invoke(this);
        }

        /// <summary>
        /// EX技能类型
        /// </summary>
        public AttackTypes AttackType { get; set; }

        public string EXSkillName { get; set; }

        public string EXSkillDesc { get; set; }

        public int EXSkillDmgExtra { get; set; }

        /// <summary>
        /// EX技攻击范围
        /// </summary>
        public int AttackRange
        {
            get
            {
                switch (AttackType)
                {
                    case AttackTypes.Laser:
                        return 5;
                    case AttackTypes.ElectronBeam:
                        return 6;
                    case AttackTypes.Rockets:
                        return 7;
                    case AttackTypes.DeadLight:
                        return 8;
                    case AttackTypes.Normal:
                        return 4;
                    case AttackTypes.Fire:
                        return 5;
                    case AttackTypes.GravityBurst:
                        return 6;
                    case AttackTypes.HugePulse:
                        return 7;
                    case AttackTypes.PionnerMissiles:
                        return 9;
                    default:
                        return 4;
                }
            }
        }


        /// <summary>
        /// 将技能转化为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return !Equals(None) ? 
                $"EXSkill:{EXSkillName}\r\nCastExDmg:{EXSkillDmgExtra}\r\nCastType:{AttackType}\r\nCastRange:{AttackRange}" : 
                "[EXSkill:None]";
        }

        /// <summary>
        /// 比较
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(EXSkill other)
        {
            return EXSkillName == other.EXSkillName && EXSkillDesc == other.EXSkillDesc;
        }


        /// <summary>
        /// 无EX技能
        /// </summary>
        public static EXSkill None => new EXSkill
        {
            AttackType = AttackTypes.Normal,
            EXSkillName = "无",
            EXSkillDesc = "该单位无EX技能",
            EXSkillDmgExtra = 0
        };
    }
}
