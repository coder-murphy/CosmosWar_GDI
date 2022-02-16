using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosWar
{
    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class Extensions
    {
        private static Unit tempUnit = null;
        /// <summary>
        /// 设置单位在控制台显示
        /// </summary>
        /// <param name="unit"></param>
        public static void ShowUnitInfoInConsole(this Unit unit)
        {
            if (unit == null || tempUnit.Equals(unit)) return;
            Scene.Instance.SetUnitPropertiesShown(unit, true);
        }
    }
}
