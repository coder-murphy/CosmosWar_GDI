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
        /// <summary>
        /// EX技能类型
        /// </summary>
        public AttackType AttackType { get; set; }

        public string EXSkillName { get; set; }

        public string EXSkillDesc { get; set; }

        public int EXSkillDmgExtra { get; set; }

        /// <summary>
        /// 将技能转化为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return !Equals(None) ? $"[EXSkill:{EXSkillName} DmgEx:{EXSkillDmgExtra} AttType:{AttackType}]" : "[EXSkill:None]";
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
            AttackType = AttackType.Normal,
            EXSkillName = "无",
            EXSkillDesc = "该单位无EX技能",
            EXSkillDmgExtra = 0
        };
    }
}
