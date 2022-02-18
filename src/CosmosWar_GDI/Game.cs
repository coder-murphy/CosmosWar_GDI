using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Runtime.InteropServices;
using NVorbis;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using CosmosWar.AIScripts;

namespace CosmosWar
{
    internal static class Game
    {
        public static bool IsGameInit => _isGameInit;

        public static bool FPSEnabled { get; set; }

        /// <summary>
        /// 当前场景
        /// </summary>
        public static GameScene CurrentScene { get; set; }

        /// <summary>
        /// 绘图区大小
        /// </summary>
        public static Size ClientSize => gameWindow == null ? default : gameWindow.ClientSize;

        /// <summary>
        /// 游戏窗口
        /// </summary>
        public static Form GameWindow => gameWindow;

        /// <summary>
        /// 获取图形
        /// </summary>
        /// <returns></returns>
        public static Graphics GetGraphics() => graphics;

        /// <summary>
        /// 动画
        /// </summary>
        public static Anime Animes => Anime.Instance;

        /// <summary>
        /// 是否允许键盘事件
        /// </summary>
        public static bool AllowKeyEvent { get; set; }

        /// <summary>
        /// 是否展示警告信息
        /// </summary>
        public static bool WarningMessageShown => warningMsgShown;

        /// <summary>
        /// 警告信息
        /// </summary>
        public static string WarningMessage => warningMsg;

        public static int CurrentAttackDamage => currentAttackDamageNum;

        public static float DamageDisplayX => damageNumX;

        public static float DamageDisplayY => damageNumY;

        public static bool IsDamageDisplay => damageDisplayed;

        /// <summary>
        /// 游戏初始化
        /// </summary>
        /// <param name="hWnd"></param>
        public static void GameInit(IntPtr handle)
        {
            hWnd = handle;
            Logger.DebugWindowVisible = true;
            gameWindow = (Form)Control.FromHandle(hWnd);
            gameWindow.BackgroundImageLayout = ImageLayout.Stretch;
            gameWindow.Text = $"{Define.GameName} {Define.GameVersion}----{Define.GameDescription}";
            gameWindow.Size = new Size(Define.GameWindowWidth, Define.GameWindowHeight);
            gameWindow.MinimumSize = new Size(Define.GameWindowWidth, Define.GameWindowHeight);
            gameWindow.MaximumSize = new Size(Define.GameWindowWidth, Define.GameWindowHeight);
            //gameWindow.FormBorderStyle = FormBorderStyle.None;
            gameWindow.DesktopLocation = new Point((Screen.PrimaryScreen.WorkingArea.Width - Define.GameWindowWidth) / 2,
                (Screen.PrimaryScreen.WorkingArea.Height - Define.GameWindowHeight) / 2);
            CurrentScene = GameScene.Entry;
            gameWindow.KeyDown += GameWindowKeyDown;
            bgmDevice = new WaveOutEvent(); // Create device
            //Console.WriteLine();
            timer = new System.Timers.Timer
            {
                Interval = 1000
            };
            timer.Elapsed += (s, e) =>
            {
                Task.Run(() =>
                {
                    currentFPS = fpsCount;
                    fpsCount = 0;
                    FrequencyAction.FrequencyCount = 0;
                });
                Task.Run(() => {
                    if(warningMsgShown)
                    {
                        if(warningMsgSec == warningMsgSecMax)
                        {
                            warningMsgShown = false;
                            warningMsgSec = 0;
                        }
                        else
                            warningMsgSec++;
                    }
                });
            };
            periodTimer = new System.Timers.Timer
            {
                Interval = 20
            };
            periodTimer.Elapsed += (s, e) =>
            {
                Task.Run(() =>
                {
                    RunInnerActions();
                });
                FrequencyAction.FrequencyCount += 20;
            };
            periodTimer.Start();
            timer.Start();
            StagesDefine();
            PlayBGM(GameSound.Intro);
            Animes.Init();
            // 设置伤害显示效果
            SetActionByFrequency(() =>
            {
                if (damageDisplayed)
                {
                    if (damageDisplayedSec == damageDisplayedSecMax)
                    {
                        damageDisplayed = false;
                        damageDisplayedSec = 0;
                    }
                    else
                    {
                        damageNumY -= 0.58f;
                        damageDisplayedSec += 20;
                    }
                }
            }, 50);
            // 初始化AI脚本
            AIPool.Init(Define.DefineAIContexts);
            _isGameInit = true;
            AllowKeyEvent = true;
        }

        /// <summary>
        /// 设置一个根据频率执行的动作（频率只能只能设置50，25，10，5，2）
        /// </summary>
        /// <param name="action"></param>
        /// <param name="frequency"></param>
        public static void SetActionByFrequency(Action action,byte frequency)
        {
            if (!allowFrequency.Contains(frequency)) return;
            FrequencyAction frequencyAction = new FrequencyAction(action);
            frequencyAction.frequency = (short)(1000 / frequency);
            actionList.Add(frequencyAction);
        }

        private static void RunInnerActions()
        {
            foreach (var action in actionList)
            {
                if (FrequencyAction.FrequencyCount % action.frequency == 0)
                    action.Invoke();
            }
        }

        public static void Exit()
        {
            gameWindow.Close();
        }

        public static void Render()
        {
            if (!IsGameInit) return;
            frame = Properties.Resources.BackGround;
            graphics = Graphics.FromImage(frame);
            switch(CurrentScene)
            {
                case GameScene.Entry:
                    EntryScene();
                    break;
                case GameScene.AreaList:
                    AreaPanel.Show();
                    break;
                case GameScene.Scene:
                    Scene.Instance.Run();
                    break;
            }
            fpsCount++;
            ShowFPS();
            GameWindow.BackgroundImage = frame;
        }

        private static void EntryScene()
        {
            var g = graphics;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            g.Clear(Color.Black);
            float top = (gameWindow.ClientSize.Height - Define.TitleSize.Height) / 2 - Define.TitleSize.Height;
            //g.DrawString(Define.GameName, Define.FontTitle, Define.WhiteBrush,
            //    new RectangleF(
            //        new PointF((gameWindow.ClientSize.Width - Define.TitleSize.Width) / 2, top),
            //        Define.TitleSize));
            g.DrawImage(Properties.Resources.title, new RectangleF(new PointF((gameWindow.ClientSize.Width - Define.TitleSize.Width) / 2, top), Define.TitleSize));
            //g.DrawString("素材来源：FC《第四次机器人大战》.", Define.FontSystem, 
            //    new SolidBrush(Color.White), gameWindow.ClientSize.Width - 200f, gameWindow.ClientSize.Height - 20f);
            g.DrawString("check option <Start Game> to start game.", Define.FontSystem,
                new SolidBrush(Color.White), gameWindow.ClientSize.Width - 300f, gameWindow.ClientSize.Height - 20f);
            g.DrawString("Start Game", Define.FontYaHei22, Define.WhiteBrush,
                new RectangleF(
                    (gameWindow.ClientSize.Width - 220f) / 2, top + 350f,
                    198f, 40f));
            g.DrawString("Exit Game", Define.FontYaHei22, Define.WhiteBrush,
                new RectangleF(
                    (gameWindow.ClientSize.Width - 198f) / 2, top + 420f,
                    198f, 40f));
            g.DrawImage(Properties.Resources._1mgameslogo, new RectangleF(20f, gameWindow.ClientSize.Height - 40f, 100f, 40f));
            if (gameOption == 0)
            {
                g.DrawString("◆", Define.FontYaHei22, Define.WhiteBrush,
                new RectangleF(
                    (gameWindow.ClientSize.Width - 220f) / 2 - 60f , top + 350f,
                    40f, 40f));
            }
            else if(gameOption == 1)
            {
                g.DrawString("◆", Define.FontYaHei22, Define.WhiteBrush,
                new RectangleF(
                    (gameWindow.ClientSize.Width - 220f) / 2 - 60f, top + 420f,
                    40f, 40f));
            }

        }

        #region privates
        private static void GameWindowKeyDown(object sender, KeyEventArgs args)
        {
            bool renderFlag = false;
            if (CurrentScene == GameScene.Entry)
            {
                if (args.KeyCode == Keys.Enter)
                {
                    switch (gameOption)
                    {
                        case 0:
                            renderFlag = true;
                            AreaPanel.Show();
                            break;
                        case 1:
                            Exit();
                            break;
                    }
                }
                else if (args.KeyCode == Keys.F)
                {
                    gameOption = (byte)(gameOption == 0 ? 1 : 0);
                }
            }
            else if (CurrentScene == GameScene.AreaList)
            {
                AreaPanel.GetKeyDown(args.KeyCode, out renderFlag);
            }
            else if (CurrentScene == GameScene.Scene)
            {
                areaSystem.GetKeyDown(args.KeyCode, out renderFlag);
            }
        }

        /// <summary>
        /// 返回开始界面
        /// </summary>
        public static void BackToEntry()
        {
            CurrentScene = GameScene.Entry;
            Render();
            PlayBGM(GameSound.Intro);
        }


        public static Map FindMap(byte currentAreaIndex)
        {
            switch (currentAreaIndex)
            {
                case 0:
                    return map1;
                case 1:
                    return map2;
                case 2:
                    return map3;
                case 3:
                    return map4;
                default:
                    return map1;
            }
        }

        public static void PlayBGM(GameSound gameSound)
        {
            switch(gameSound)
            {
                case GameSound.Intro:
                    //audioPlayer.FileName = "Data/BGM/Robot4screenloop.ogg";
                    //audioPlayer.Play();
                    FileInfo fi = new FileInfo("Data/BGM/Robot4screenloop.ogg");
                    //using(MemoryStream ms = new MemoryStream(Properties.Resources.Robot4screenloop))
                    //FileInfo fi = new FileInfo("Data/BGM/cosmoswar.wav");
                    //using (var vorbis = new NAudio.Vorbis.VorbisWaveReader(fi.FullName))
                    //{
                    //    Console.WriteLine($"播放：[{fi.FullName}]");
                    //    using (var waveOut = new NAudio.Wave.WaveOutEvent())
                    //    {
                    //        waveOut.Init(vorbis);
                    //        waveOut.Play();
                    //    }
                    //}
                    break;
                default:
                    break;
            }
        }

        public static void StagesDefine()
        {
            map1 = Map.FromFile(Properties.Resources.Area1);
            //Logger.Log(map1);
        }

        /// <summary>
        /// 设置警告提示消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="seconds"></param>
        public static void SetWarningMessage(string message,int seconds)
        {
            warningMsg = message;
            warningMsgSecMax = seconds;
            warningMsgShown = true;
        }

        /// <summary>
        /// 关闭警告
        /// </summary>
        public static void WarningMessageDisabled()
        {
            warningMsg = String.Empty;
            warningMsgSec = 0;
            warningMsgSecMax = 0;
            warningMsgShown = false;
        }

        /// <summary>
        /// 在指定网格区域显示伤害
        /// </summary>
        /// <param name="dmg"></param>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <param name="sec"></param>
        public static void DisplayDamage(int dmg,int x,int y,int sec)
        {
            damageDisplayedSecMax = sec * 1000;
            currentAttackDamageNum = dmg;
            damageDisplayedSec = 0;
            damageNumX = x;
            damageNumY = y;
            Console.WriteLine($"展示伤害 坐标{damageNumX},{damageNumY} 值：{CurrentAttackDamage}");
            damageDisplayed = true;
        }

        /// <summary>
        /// 当前秒已经走过的帧数
        /// </summary>
        public static short CurrentFrameSec => fpsCount;

        private static IntPtr hWnd = IntPtr.Zero;
        private static Graphics graphics;
        private static bool _isGameInit = false;
        private static byte gameOption = 0;
        private static Form gameWindow = null;
        //private static readonly CSAudioPlayer.AudioPlayer audioPlayer = new CSAudioPlayer.AudioPlayer();
        private static IWavePlayer bgmDevice = null;
        private static AudioFileReader _reader = null;
        private static VolumeSampleProvider _volumeProvider = null;
        private static Scene areaSystem => Scene.Instance;

        private static Map map1 = null;
        private static Map map2 = null;
        private static Map map3 = null;
        private static Map map4 = null;
        // FPS计算
        private static Image frame = null;
        private static short fpsCount = 0;
        private static short currentFPS = 0;
        private static System.Timers.Timer timer = null;
        private static System.Timers.Timer periodTimer = null;
        private static readonly List<FrequencyAction> actionList = new List<FrequencyAction>();
        private static readonly int[] allowFrequency = new int[] { 50, 25, 20, 10, 5, 2 };
        // warningMsg
        private static int warningMsgSec = 0;
        private static string warningMsg = string.Empty;
        private static int warningMsgSecMax = 0;
        private static bool warningMsgShown = false;
        // damage
        private static bool damageDisplayed = false;
        private static int damageDisplayedSecMax = 0;
        private static int damageDisplayedSec = 0;
        private static int currentAttackDamageNum = 0;
        private static float damageNumX = 0;
        private static float damageNumY = 0;

        public static void ShowFPS()
        {
            if (graphics == null || !FPSEnabled)
                return;
            graphics?.DrawString($"FPS:{currentFPS}", Define.FontSystem, Define.YellowBrush,
                new RectangleF(
                    gameWindow.ClientSize.Width - 60f, 5f,
                    60f, 30f));
        }

        /// <summary>
        /// 频率动作
        /// </summary>
        private class FrequencyAction
        {
            internal FrequencyAction(Action action)
            {
                this.action = action;
            }

            internal static short FrequencyCount { get; set; }

            internal Action action;

            internal short frequency;

            internal void Invoke()
            {
                action?.Invoke();
            }
        }

        [DllImport("winmm.dll")]
        private static extern int mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);

        #endregion
    }

    /// <summary>
    /// 警告消息事件
    /// </summary>
    /// <param name="message"></param>
    public delegate void WarningMessageEventHandler(string message);

    public enum GameSound
    {
        Intro = 0,
        AreaList = 1,
        Map1 = 2,
        Map2 = 3,
        Map3 = 4,
        Map4 = 5,
    }

    /// <summary>
    /// 游戏场景枚举
    /// </summary>
    public enum GameScene
    {
        None = 0,
        Entry = 1,
        AreaList = 2,
        Scene = 3,
        GameMenu = 4,
    }
}
