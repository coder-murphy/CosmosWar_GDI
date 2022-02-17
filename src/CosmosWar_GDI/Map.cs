using CosmosWar.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosWar
{
    public class Map
    {
        /// <summary>
        /// 定义一个高为h 宽度为w的地图
        /// </summary>
        /// <param name="h"></param>
        /// <param name="w"></param>
        public Map(string name, byte h, byte w)
        {
            matrix = new byte[h,w];
            mapName = name;
            width = w;
            height = h;
        }

        /// <summary>
        /// 地图名
        /// </summary>
        public string MapName => mapName;

        public Point LocationForceRed => new Point(forceRStartLocX, forceRStartLocY);
        public Point LocationForceBlue => new Point(forceBStartLocX, forceBStartLocY);

        public Size MapSize => new Size(width, height);

        /// <summary>
        /// 通过x与y访问地图
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public byte this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= width || y < 0 || y >= height)
                    return 0;
                return matrix[y, x];
            }
            set
            {
                if(x >= 0 && y >= 0 && x < width && y < height)
                    matrix[y, x] = value;
            }
        }

        /// <summary>
        /// 初始化一个地图
        /// <para>eg:0,0,0,1,0\n1,0,0,0,1
        /// </para>
        /// </summary>
        public static Map FromFile(string map)
        {
            string[] lines = map.Split('\n');
            byte h = (byte)(lines.Length - 1);
            byte w = (byte)(lines[1].Split(',')).Length;
            Map m = new Map(new string(lines[0].Where(x => x != '[' && x != ']' && x != '\r').ToArray()), h, w);
            for (byte i = 0; i < h; i++)
            {
                string[] line = lines[i + 1].Split(',');
                for(byte j = 0; j < w; j++)
                {
                    var tile = byte.Parse(line[j]);
                    //Logger.Log(tile.ToString() + ',');
                    m[j, i] = tile;
                    if(tile == 98)
                    {
                        m.forceRStartLocX = j;
                        m.forceRStartLocY = i;
                    }
                    else if(tile == 99)
                    {
                        m.forceBStartLocX = j;
                        m.forceBStartLocY = i;
                    }
                }
            }
            return m;
        }

        public override string ToString()
        {
            return $"地图名：{mapName}\n" +
                $"宽：{MapSize.Width}\n高：{MapSize.Height}\n" +
                $"红方起始点位：{LocationForceRed}\n" +
                $"蓝方起始点位：{LocationForceBlue}";
        }

        private string mapName = null;
        private byte forceRStartLocX = 0;
        private byte forceRStartLocY = 0;
        private byte forceBStartLocX = 0;
        private byte forceBStartLocY = 0;
        private byte[,] matrix = null;
        private int width;
        private int height;
    }
}
