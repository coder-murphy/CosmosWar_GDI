using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosWar.AIScripts
{
    /// <summary>
    /// 对战AI
    /// </summary>
    public abstract class AIBase
    {
        /// <summary>
        /// 新建一个AI
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="description"></param>
        public AIBase(string name,string version = "1.0",string description = "nothing")
        {
            Name = name;
            Version = version;
            Description = description;
            force = "R";
        }

        public string Name { get; set; }

        public string Version { get; set; }

        public string Description { get; set; }
        /// <summary>
        /// AI阵营
        /// </summary>
        public string Force => force;

        /// <summary>
        /// 注册回合开始金钱逻辑
        /// </summary>
        /// <param name="money"></param>
        public AIBase RegRoundBegin(Behaviour<int> money)
        {
            moneyBehaviourRoundBegin = money;
            return this;
        }

        /// <summary>
        /// 注册每次移动单位从单位组抽取单位移动策略
        /// </summary>
        /// <param name="selector"></param>
        public AIBase RegSelectUnit(ConvertBehaviour<IEnumerable<Unit>, Unit> selector)
        {
            unitsSelector = selector;
            return this;
        }

        /// <summary>
        /// 配置进攻敌方单位优先级
        /// </summary>
        /// <param name="enemyUnitPriority"></param>
        public AIBase RegAttackEnemyUnitPriority(EqualsCondition<Unit,Unit> enemyUnitPriorityCondition)
        {
            this.enemyUnitPriorityCondition = enemyUnitPriorityCondition;
            return this;
        }

        /// <summary>
        /// 设置建造策略
        /// </summary>
        /// <param name="buildStrategy"></param>
        /// <returns></returns>
        public AIBase SetFactoryBuildStrategy(AIFactoryBuildStrategy buildStrategy)
        {
            aIFactoryBuildStrategy = buildStrategy;
            return this;
        }

        #region ai behaviours
        /// <summary>
        /// 回合开始行为
        /// </summary>
        /// <param name="money"></param>
        public void RoundBeginBehaviour(ref int money)
        {
            moneyBehaviourRoundBegin?.Invoke(money);
        }

        /// <summary>
        /// 发动单位选择
        /// </summary>
        public void DoSelectUnit()
        {
            Console.WriteLine("电脑选取单位");
            Task.Run(() => {
                var ownUnits = Scene.Instance.Units.Where(x => x.Force == force && !x.IsHome && x.IsThisRoundMoved == false);
                if (ownUnits.Count() == 0)
                    return;
                var ownUnit = unitsSelector?.Invoke(ownUnits);
                Scene.Instance.SetUnitPropertiesShown(ownUnit, true);
                if (ownUnit.IsFactory)
                {
                    //Scene.Instance.GoldR
                    var canBuildUnits = Scene.Instance.ManufactureUnitTypesR.Where(x => x.Cost <= Scene.Instance.GoldR);
                    if(canBuildUnits.Count() == 0)
                    {
                        Scene.Instance.OrderFactoryBuildUnit(ownUnit, null);
                    }
                    else
                    {
                        var bUnit = canBuildUnits.OrderByDescending(x => x.Cost).FirstOrDefault();
                        Scene.Instance.OrderFactoryBuildUnit(ownUnit, bUnit);
                    }
                }
                else
                {
                    Scene.Instance.MoveSelectBoxToTarget(ownUnit.GridLocX, ownUnit.GridLocY);
                    // 获取敌方单位
                    var enemys = Scene.Instance.Units.Where(x => x.Force != force && x.IsFactory == false);
                    Unit priorityUnit = enemys.OrderByDescending(x => enemyUnitPriorityCondition?.Invoke(x, ownUnit)).FirstOrDefault();
                    if (priorityUnit == null)
                        return;
                    var moveGrids = CWMath.GetUnitRoundTiles(ownUnit);
                    foreach(var i in moveGrids)
                    {
                        Console.WriteLine(i);
                    }
                    Thread.Sleep(1000);
                    var p = CWMath.GetNearLocInMoveGrids(moveGrids, priorityUnit.GridLocX, priorityUnit.GridLocY);
                    Console.WriteLine($"AI获取近点：{p}");
                    Scene.Instance.MoveSelectBoxToTarget((byte)p.X, (byte)p.Y);
                    Game.Animes.MoveUnit(ownUnit, (byte)p.X, (byte)p.Y);
                }
            });
        }

        #endregion
        private Behaviour<int> moneyBehaviourRoundBegin = null;
        private EqualsCondition<Unit,Unit> enemyUnitPriorityCondition = null;
        private ConvertBehaviour<IEnumerable<Unit>, Unit> unitsSelector = null;
        private AIFactoryBuildStrategy aIFactoryBuildStrategy = AIFactoryBuildStrategy.Normal;
        private string force;
    }

    /// <summary>
    /// 一元条件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tSrc"></param>
    /// <returns></returns>
    public delegate bool Condition<in T>(T tSrc);

    /// <summary>
    /// 二元条件
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public delegate bool Condition<in T1,in T2>(T1 t1,T2 t2);

    /// <summary>
    /// 一元比较条件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public delegate double EqualsCondition<in T>(T t);

    /// <summary>
    /// 二元比较条件
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns></returns>
    public delegate double EqualsCondition<in T1,in T2>(T1 t1,T2 t2);

    /// <summary>
    /// 行为
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tSrc"></param>
    public delegate void Behaviour<in T>(T tSrc);

    /// <summary>
    /// 转换行为
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="t1"></param>
    /// <returns></returns>
    public delegate T2 ConvertBehaviour<in T1, T2>(T1 t1);

    /// <summary>
    /// 设置工厂建造策略
    /// </summary>
    public enum AIFactoryBuildStrategy
    {
        Pool = 0,
        Rich = 1,
        Normal = 2,
    }
}
