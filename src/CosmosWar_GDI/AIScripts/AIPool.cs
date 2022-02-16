using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosWar.AIScripts
{
    /// <summary>
    /// AI池子
    /// </summary>
    public class AIPool
    {
        public static AIPool Instance
        {
            get
            {
                if(instance == null)
                    instance = new AIPool();
                return instance;
            }
        }

        /// <summary>
        /// 根据名称寻找AI
        /// </summary>
        /// <param name="aiName"></param>
        /// <returns></returns>
        public AIBase this[string aiName] => ais.FirstOrDefault(x => x.Name == aiName);

        /// <summary>
        /// 通过AI数组初始化AI池
        /// </summary>
        /// <param name="aiArray"></param>
        public static void Init(AIBase[] aiArray)
        {
            Instance.ais.Clear();
            Instance.ais.AddRange(aiArray);
        }


        private AIPool() { }
        private List<AIBase> ais = new List<AIBase>();
        private static AIPool instance = null; 
    }
}
