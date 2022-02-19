using CosmosWar.AIScripts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CosmosWar
{
    /// <summary>
    /// 地区系统
    /// </summary>
    public class Scene
    {
        public static Scene Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Scene();
                    _Instance.currentSelectedUnit = new CurrentSelectedUnitInfo();
                    _Instance.currentSelectedUnit.shown = false;
                }
                return _Instance;
            }
        }

        private Scene() 
        {
            Init();
        }

        /// <summary>
        /// 场景是否正在运行
        /// </summary>
        public bool IsSceneRunning => isSceneRunning;

        /// <summary>
        /// 最大单位数
        /// </summary>
        public const int MaxUnit = 60;

        public int GoldR { get; set; }
        public int GoldB { get; set; }

        public float ConsoleTop => csh - 105f;

        public float ConsoleLeft => 25f;

        public float ConsolePropLeft => csw - 235f;

        public float ConsolePropTop => ConsoleTop;

        public static byte SceneGridLeft => currentScreenGridLeft;

        public static byte SceneGridTop => currentScreenGridTop;

        /// <summary>
        /// 是否展示移动网格
        /// </summary>
        public static bool ShowMoveGrids { get; set; }

        /// <summary>
        /// 当前地图大小
        /// </summary>
        public static Size CurrentMapSize => Instance.currentMap.MapSize;

        /// <summary>
        /// 当前使用的AI
        /// </summary>
        public static AIBase CurrentAI => Instance.currentAI;

        /// <summary>
        /// 获取所有场景单位数组
        /// </summary>
        public Unit[] Units => sceneUnits.Where(x => !x.IsHome).ToArray();

        /// <summary>
        /// 工厂可建造的红方单位
        /// </summary>
        public IEnumerable<Unit> ManufactureUnitTypesR => manufactureUnitTypesR;

        /// <summary>
        /// 工厂可建造的蓝方单位
        /// </summary>
        public IEnumerable<Unit> ManufactureUnitTypesB => manufactureUnitTypesB;

        /// <summary>
        /// 读取地图
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public bool LoadMap(Map map)
        {
            currentMap = map;
            // 读取地图配置
            Logger.Log($"读取地图配置 地图名：{currentMap.MapName}");
            var setting = Define.GetMapSetting(currentMap.MapName);
            if (!string.IsNullOrWhiteSpace(setting))
            {
                LoadMapSetting(setting);
                LoadAI(AIPool.Instance["m_AIContext"]);
            }

            return true;
        }

        public void GetKeyDown(Keys keyCode, out bool renderFlag)
        {
            if (Game.AllowKeyEvent == false)
            {
                renderFlag = true;
                return;
            }
            bool isDirKey = false;
            bool unFocusUnitFlag = false;
            if (currentMap == null)
            {
                isSceneRunning = false;
                Clear();
                Game.CurrentScene = GameScene.Entry;
                Logger.Log("Error:未能加载地图");
                renderFlag = true;
                return;
            }
            if (keyCode == Keys.Escape)
            {
                if (!isGameMenuShowing)
                    isGameMenuShowing = true;
                else if(isVictory)
                {
                    isSceneRunning = false;
                    Clear();
                    Game.CurrentScene = GameScene.Entry;
                    isVictory = false;
                }
            }
            else if (keyCode == Keys.Enter)
            {
                if (isGameMenuShowing)
                {
                    isGameMenuShowing = false;
                    if (gameMenuOption == 1)
                    {
                        isSceneRunning = false;
                        Clear();
                        Game.CurrentScene = GameScene.Entry;
                    }
                }
                else if (isVictory)
                {
                    isSceneRunning = false;
                    Clear();
                    Game.CurrentScene = GameScene.Entry;
                    isVictory = false;
                }
            }
            else if(keyCode == Keys.F)
            {
                if (isGameMenuShowing)
                    gameMenuOption = gameMenuOption == 1 ? 0 : 1;
                renderFlag = true;
                return;
            }
            #region directions
            else if (keyCode == Keys.Left || keyCode == Keys.A)
            {
                if (isHumanRound == false)
                {
                    renderFlag = true;
                    return;
                }
                if(!isGameMenuShowing)
                {
                    isDirKey = true;
                    if (isFactoryRunning)
                    {
                        if (currentSelectedUnit.unit.IsThisRoundMoved || currentSelectedUnit.unit.Force != OurForce)
                        {
                            renderFlag = true;
                            return;
                        }
                        int max = isHumanRound ? manufactureUnitTypesB.Count : manufactureUnitTypesR.Count;
                        if (currentManufactureUnitIndex > 0)
                            currentManufactureUnitIndex--;
                        ChangeFactorySelectUnit(currentManufactureUnitIndex);
                    }
                    else if (isUnitSelected && currentSelectedUnit.unit.Force == OurForce && !isUnitMoving && !currentSelectedUnit.unit.IsThisRoundMoved)
                    {
                        if (isUnitCasting)
                        {
                            if (exSkillTargets.Count == 0)
                            {
                                renderFlag = false;
                                return;
                            }
                            currentExSkillCastTargetIndex = currentExSkillCastTargetIndex > 0 ? --currentExSkillCastTargetIndex : 0;
                            ChangeEXSkillCastTarget(currentExSkillCastTargetIndex);
                        }
                        Logger.Log("按下Left");
                        if(!currentSelectedUnit.unit.IsThisRoundMoved)
                            currentUnitAction = currentUnitAction > 0 ? --currentUnitAction : (byte)0;
                    }
                    else if (isUnitMoving)
                    {
                        if(unitMoveGrids.Exists(x => x.X == absCurrentTargetTileX - 1 && x.Y == absCurrentTargetTileY))
                            if (currentScreenGridLeft == 0)
                            {
                                if (currentScreenTargetTileX > 0)
                                {
                                    currentScreenTargetTileX--;
                                }
                            }
                            else
                            {
                                if (currentScreenTargetTileX == 0)
                                    currentScreenGridLeft--;
                                else
                                    currentScreenTargetTileX--;
                            }
                    }
                    else
                    {
                        if (currentScreenGridLeft == 0)
                        {
                            if (currentScreenTargetTileX > 0)
                            {
                                unFocusUnitFlag = true;
                                currentScreenTargetTileX--;
                            }
                        }
                        else
                        {
                            if (currentScreenTargetTileX == 0)
                                currentScreenGridLeft--;
                            else
                                currentScreenTargetTileX--;
                            unFocusUnitFlag = true;
                        }
                    }
                }
            }
            else if (keyCode == Keys.Up || keyCode == Keys.W)
            {
                if (isFactoryRunning)
                {
                    renderFlag = true;
                    return;
                }
                if (!isGameMenuShowing)
                {
                    isDirKey = true;
                    if (isHumanRound == false || isUnitCasting)
                    {
                        renderFlag = true;
                        return;
                    }
                    if (isUnitCommandPanelShowing && currentSelectedUnit.unit.Force == OurForce && !isUnitMoving)
                    {

                    }
                    else if (isUnitMoving)
                    {
                        if (unitMoveGrids.Exists(x => x.X == absCurrentTargetTileX && x.Y == absCurrentTargetTileY - 1))
                            if (currentScreenGridTop == 0)
                            {
                                if (currentScreenTargetTileY > 0)
                                {
                                    currentScreenTargetTileY--;
                                }
                            }
                            else
                            {
                                if (currentScreenTargetTileY == 0)
                                    currentScreenGridTop--;
                                else
                                    currentScreenTargetTileY--;
                            }
                    }
                    else
                    {
                        if (currentScreenGridTop == 0)
                        {
                            if (currentScreenTargetTileY > 0)
                            {
                                unFocusUnitFlag = true;
                                currentScreenTargetTileY--;
                            }
                        }
                        else
                        {
                            unFocusUnitFlag = true;
                            if (currentScreenTargetTileY == 0)
                                currentScreenGridTop--;
                            else
                                currentScreenTargetTileY--;
                        }
                    }
                }
                else
                {

                }
            }
            else if (keyCode == Keys.Right || keyCode == Keys.D)
            {
                if (isHumanRound == false)
                {
                    renderFlag = true;
                    return;
                }
                if (!isGameMenuShowing)
                {
                    isDirKey = true;
                    if (isFactoryRunning)
                    {
                        if (currentSelectedUnit.unit.IsThisRoundMoved || currentSelectedUnit.unit.Force != OurForce)
                        {
                            renderFlag = true;
                            return;
                        }
                        int max = isHumanRound ? manufactureUnitTypesB.Count : manufactureUnitTypesR.Count;
                        if (currentManufactureUnitIndex < max - 1)
                            currentManufactureUnitIndex++;
                        ChangeFactorySelectUnit(currentManufactureUnitIndex);
                    }
                    else if (isUnitSelected && currentSelectedUnit.unit.Force == OurForce && !isUnitMoving && !currentSelectedUnit.unit.IsThisRoundMoved)
                    {
                        if(isUnitCasting)
                        {
                            if (exSkillTargets.Count == 0)
                            {
                                renderFlag = false;
                                return;
                            }
                            currentExSkillCastTargetIndex = currentExSkillCastTargetIndex < exSkillTargets.Count - 1 ? ++currentExSkillCastTargetIndex : exSkillTargets.Count - 1;
                            ChangeEXSkillCastTarget(currentExSkillCastTargetIndex);
                        }
                        Logger.Log("按下Right");
                        if (!currentSelectedUnit.unit.IsThisRoundMoved)
                            currentUnitAction = (byte)(currentUnitAction < unitOrdersBuffers.Length - 1 ? ++currentUnitAction : unitOrdersBuffers.Length - 1);
                    }
                    else if (isUnitMoving)
                    {
                        if(unitMoveGrids.Exists(x => x.X == absCurrentTargetTileX + 1 && x.Y == absCurrentTargetTileY))
                            if (ScreenGridMaxWidth + currentScreenGridLeft == currentMap.MapSize.Width)
                            {
                                if (currentScreenTargetTileX < ScreenGridMaxWidth - 1)
                                    currentScreenTargetTileX++;
                            }
                            else
                            {
                                if (currentScreenTargetTileX == ScreenGridMaxWidth - 1)
                                    currentScreenGridLeft++;
                                else
                                    currentScreenTargetTileX++;
                            }
                    }
                    else
                    {
                        if (ScreenGridMaxWidth + currentScreenGridLeft == currentMap.MapSize.Width)
                        {
                            if (currentScreenTargetTileX < ScreenGridMaxWidth - 1)
                            {
                                unFocusUnitFlag = true;
                                currentScreenTargetTileX++;
                            }
                        }
                        else
                        {
                            unFocusUnitFlag = true;
                            if (currentScreenTargetTileX == ScreenGridMaxWidth - 1)
                                currentScreenGridLeft++;
                            else
                                currentScreenTargetTileX++;
                        }
                    }
                }
            }
            else if (keyCode == Keys.Down || keyCode == Keys.S)
            {
                if(isFactoryRunning)
                {
                    renderFlag = true;
                    return;
                }
                if (!isGameMenuShowing)
                {
                    if (isHumanRound == false || isUnitCasting)
                    {
                        renderFlag = true;
                        return;
                    }
                    isDirKey = true;
                    if (isUnitCommandPanelShowing && currentSelectedUnit.unit.Force == OurForce && !isUnitMoving)
                    {

                    }
                    else if (isUnitMoving)
                    {
                        if (unitMoveGrids.Exists(x => x.X == absCurrentTargetTileX && x.Y == absCurrentTargetTileY + 1))
                            if (ScreenGridMaxHeight + currentScreenGridTop == currentMap.MapSize.Height)
                            {
                                if (currentScreenTargetTileY < ScreenGridMaxHeight - 1)
                                    currentScreenTargetTileY++;
                            }
                            else
                            {
                                if (currentScreenTargetTileY == ScreenGridMaxHeight - 1)
                                    currentScreenGridTop++;
                                else
                                    currentScreenTargetTileY++;
                            }
                    }
                    else
                    {
                        if (ScreenGridMaxHeight + currentScreenGridTop == currentMap.MapSize.Height)
                        {
                            if (currentScreenTargetTileY < ScreenGridMaxHeight - 1)
                            {
                                unFocusUnitFlag = true;
                                currentScreenTargetTileY++;
                            }
                        }
                        else
                        {
                            unFocusUnitFlag = true;
                            if (currentScreenTargetTileY == ScreenGridMaxHeight - 1)
                                currentScreenGridTop++;
                            else
                                currentScreenTargetTileY++;
                        }
                    }
                }
                else
                {

                }
                isDirKey = true;
            }
            #endregion
            else if ((keyCode == Keys.NumPad2 || keyCode == Keys.K) && !isVictory)
            {
                if (!isGameMenuShowing)
                {
                    if (isHumanRound == false)
                    {
                        renderFlag = true;
                        return;
                    }
                    if (isFactoryRunning)
                    {
                        if (currentSelectedUnit.unit.Force == OurForce)
                        {
                            if (!currentSelectedUnit.unit.IsThisRoundMoved)
                                OrderFactoryBuildUnit();
                            else
                            {
                                SetFactoryModeDisabled();
                            }
                        }
                        renderFlag = true;
                    }
                    else if (isUnitMoving)
                    {
                        MoveUnit(currentSelectedUnit.unit, absCurrentTargetTileX, absCurrentTargetTileY);
                    }
                    else if(isUnitCasting)
                    {
                        CastEXSkill(currentSelectedUnit.unit, exSkillCastTarget);
                    }
                    else if(!isUnitSelected)
                    {
                        SelectTile(absCurrentTargetTileX, absCurrentTargetTileY);// 进行块选择
                    }
                    if (isUnitSelected && !isUnitCommandPanelShowing && !isFactoryRunning)
                    {
                        Logger.Log($"选择 结果{isUnitSelected}");
                        isUnitCommandPanelShowing = true;
                    }
                    else if(isUnitCommandPanelShowing)
                    {
                        //currentUnitAction = 0;
                        if (currentSelectedUnit.unit.Force == OurForce && !isFactoryRunning && !currentSelectedUnit.unit.IsThisRoundMoved)
                        {
                            OrderUnit(currentUnitAction);
                            isUnitCommandPanelShowing = false;
                        }
                        else
                        {
                            isUnitCommandPanelShowing = false;
                            isUnitMoving = false;
                            isUnitCasting = false;
                            //isUnitSelected = false;
                        }
                    }
                }
                else
                {
                }
            }
            else if ((keyCode == Keys.NumPad1 || keyCode == Keys.J) && !isVictory) //取消生产/指令
            {
                if (isHumanRound == false)
                {
                    renderFlag = true;
                    return;
                }
                if (isUnitCasting)
                {
                    isUnitCommandPanelShowing = true;
                    isUnitCasting = false;
                }
                if (isFactoryRunning)
                {
                    SetFactoryModeDisabled();
                }
            }
            if (currentSelectedUnit.shown && isDirKey)
            {
                if(currentSelectedUnit.unit.Force != OurForce)
                {
                    isUnitMoving = false;
                    isUnitCasting = false;
                    isUnitSelected = false;
                }
            }
            if (unFocusUnitFlag)
            {
                isUnitSelected = false;
                isUnitMoving = false;
                isUnitCasting = false;
                isUnitSelected = false;
                isUnitCommandPanelShowing = false;
                currentUnitAction = 0;
            }
            renderFlag = true;
        }

        /// <summary>
        /// 寻找单位
        /// </summary>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <returns></returns>
        public Unit FindUnit(byte gridX, byte gridY)
        {
            IEnumerable<Unit> units = sceneUnits.Where(x => x.GridLocX == gridX && x.GridLocY == gridY);
            if(units.Count() > 0)
            {
                if (units.Count() == 1)
                    return units.First();
                else
                    return units.First(x => !x.IsFactory && !x.IsHome);
            }
            else
                return null;
        }

        /// <summary>
        /// 寻找格子内所有单位
        /// </summary>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <returns></returns>
        public IEnumerable<Unit> FindUnits(byte gridX,byte gridY)
        {
            return sceneUnits.Where(x => x.GridLocX == gridX && x.GridLocY == gridY);
        }

        /// <summary>
        /// 设置单位属性显示
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="flag"></param>
        public void SetUnitPropertiesShown(Unit unit,bool flag)
        {
            currentSelectedUnit.shown = flag;
            if (unit == null) return;
            isUnitSelected = flag;
            currentSelectedUnit.unit = unit;
            currentSelectedUnit.Icon = unit.Force == "R" ? Define.UnitTypesImageMapR[unit.Icon] : Define.UnitTypesImageMapB[unit.Icon];
            currentSelectedUnit.infos = new string[]
            {
                unit.Name,
                unit.Life.ToString(),
                unit.Damage.ToString(),
                unit.Armor.ToString(),
                unit.Move.ToString(), 
            };
            currentSelectedUnit.IsActiveUnit = !(unit.IsFactory || unit.IsHome);
            currentSelectedUnit.EXSkillInfo = unit.EXSkill.ToString();
        }

        /// <summary>
        /// 加载AI
        /// </summary>
        /// <param name="ai"></param>
        public void LoadAI(AIBase ai)
        {
            currentAI = ai;
        }

        /// <summary>
        /// 摄像机跳转到目标点
        /// </summary>
        /// <param name="locX"></param>
        /// <param name="locY"></param>
        public void FocusTo(byte locX,byte locY)
        {
            if(locX >= 0 && locX <= ScreenGridMaxWidth / 2)
            {
                currentScreenGridLeft = 0;
            }
            else if(locX >= CurrentMapSize.Width - ScreenGridMaxWidth / 2 && locX <= CurrentMapSize.Width - 1)
            {
                currentScreenGridLeft = (byte)(CurrentMapSize.Width - ScreenGridMaxWidth);
            }
            else
            {
                currentScreenGridLeft = (byte)(locX - ScreenGridMaxWidth / 2);
            }
            if (locY >= 0 && (locY <= ScreenGridMaxHeight / 2))
            {
                currentScreenGridTop = 0;
            }
            else if (locY >= CurrentMapSize.Height - ScreenGridMaxHeight / 2 && (locY <= CurrentMapSize.Height - 1))
            {
                currentScreenGridTop = (byte)(CurrentMapSize.Height - ScreenGridMaxHeight);
            }
            else
            {
                currentScreenGridTop = (byte)(locY - ScreenGridMaxHeight / 2);
            }
        }

        /// <summary>
        /// 移动选择框至指定区域
        /// </summary>
        /// <param name="locX"></param>
        /// <param name="locY"></param>
        public void MoveSelectBoxToTarget(byte locX,byte locY)
        {
            FocusTo(locX, locY);
            currentScreenTargetTileX = (byte)(locX - currentScreenGridLeft);
            currentScreenTargetTileY = (byte)(locY - currentScreenGridTop);
        }

        /// <summary>
        /// 设置场景图形操作
        /// </summary>
        /// <param name="action"></param>
        public void SetSceneGraphicsAction(Action<Graphics> action,bool displayFlag = true)
        {
            graphicsAction = action;
            displayGraphicsActionFlag = displayFlag;
        }

        Action<Graphics> graphicsAction = null;
        bool displayGraphicsActionFlag = true;

        /// <summary>
        /// 获取网格坐标对应的屏幕坐标
        /// </summary>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <param name="screenX"></param>
        /// <param name="screenY"></param>
        public PointF GetUnitScreenLoc(int gridX,int gridY)
        {
            float screenX = gapX + (gridX - currentScreenGridLeft) * ScreenTileSize + ScreenTileSize / 2;
            float screenY = gapY + (gridY - currentScreenGridTop) * ScreenTileSize + ScreenTileSize / 2;
            return new PointF(screenX, screenY);
        }

        internal void Run()
        {
            if(isSceneRunning == false)
            {
                isSceneRunning = true;
                clientSize = Game.ClientSize;
                csw = clientSize.Width;
                csh = clientSize.Height;
                rectUnitIcon.Width = Define.UnitIconSize;
                rectUnitIcon.Height = Define.UnitIconSize;
                rectUnitIcon.Location = new PointF(ConsoleLeft + 10f, ConsoleTop + 10f);

            }
            Graphics g = Game.GetGraphics();
            if (isGameMenuShowing)
                DrawGameMenu(g);
            else
            {
                DrawConsole(g);
                DrawScene(g);
            }
        }

        private void DrawConsole(Graphics g)
        {
            if (isVictory)
            {
                return;
            }
            // main frame
            g.DrawLine(Define.FramePenWhite5, cLeft, cTop, cRight, cTop);
            g.DrawLine(Define.FramePenWhite5, cLeft, cBottom, cRight, cBottom);
            g.DrawLine(Define.FramePenWhite5, cLeft, cTop, cLeft, cBottom);
            g.DrawLine(Define.FramePenWhite5, cRight, cTop, cRight, cBottom);
            // split line
            if(!isFactoryRunning)
                g.DrawLine(Define.FramePenWhite5, cSpLeft, cTop, cSpLeft, cBottom);
            else
                g.DrawLine(Define.FramePenWhite5, factorySplitLineLeft, cTop, factorySplitLineLeft, cBottom);
            // g.DrawLine(Define.FramePenWhite5, 20f, csh - 90f, csw - 240f, csh - 90f);
            // golds
            g.DrawString($"红方金钱：{GoldR} 蓝方金钱：{GoldB} 红方部队残余：{rCount} 蓝方部队残余：{bCount}", Define.FontSystem, Define.WhiteBrush, 26f, csh + 8f);
            // unitInfos
            if(currentSelectedUnit != null && currentSelectedUnit.shown)
            {
                string [] infos = currentSelectedUnit.infos;
                Image icon = currentSelectedUnit.Icon;
                float infoX = rectUnitIcon.Right + 10f;
                g.DrawImage(icon, rectUnitIcon);
                g.DrawString($"Name:{infos[0]}", Define.FontSystem12, Define.WhiteBrush, infoX, ConsolePropTop + 5f);
                if(currentSelectedUnit.unit.IsFactory)
                {
                    if(currentSelectedUnit.unit.IsThisRoundMoved)
                    {
                        g.DrawString($"生产单位\r\n(不可生产)", Define.FontSystem12, Define.YellowBrush, infoX, ConsolePropTop + 25f);
                    }
                    else
                    {
                        g.DrawString($"生产单位\r\n(可生产)", Define.FontSystem12, Define.YellowBrush, infoX, ConsolePropTop + 25f);
                    }
                }
                if(currentSelectedUnit.IsActiveUnit)
                {
                    g.DrawString($"HP:{infos[1]}", Define.FontSystem12, Define.WhiteBrush, infoX, ConsolePropTop + 30f);
                    g.DrawString($"Attack:{infos[2]}", Define.FontSystem12, Define.WhiteBrush, infoX + 100f, ConsolePropTop + 30f);
                    g.DrawString($"Armor:{infos[3]}", Define.FontSystem12, Define.WhiteBrush, infoX, ConsolePropTop + 55f);
                    g.DrawString($"Move:{infos[4]}", Define.FontSystem12, Define.WhiteBrush, infoX + 100f, ConsolePropTop + 55f);
                    // EX
                    if (!currentSelectedUnit.EXSkillInfo.Equals(EXSkill.None))
                        g.DrawRectangle(Define.FramePenWhite2, infoX + 230f, ConsolePropTop + 5f, 190f, 80f);
                    g.DrawString($"{currentSelectedUnit.EXSkillInfo}", Define.FontSystem, Define.YellowBrush, infoX + 240f, ConsolePropTop + 15f);
                }
            }
            if(Game.WarningMessageShown)
            {
                g.DrawString(Game.WarningMessage, Define.FontSystem12, Define.WhiteBrush, ConsolePropLeft + 25f, ConsolePropTop + 15f);
            }
            else if (isFactoryRunning && !currentSelectedUnit.unit.IsThisRoundMoved)
            {
                // 绘制工厂列表
                if(currentSelectedUnit.unit.Force == OurForce)
                    DrawFactoryList(g);
                else
                {
                    isFactoryRunning = false;
                }
                //g.DrawString("敌方单位", Define.FontSystem16Bold, Define.WhiteBrush, ConsolePropLeft + 25f, ConsolePropTop + 40f);
            }
            else if (isUnitMoving)
            {
                g.DrawString("选择目标区域或单位以\r\n进行或攻击占领。", Define.FontSystem12, Define.WhiteBrush, ConsolePropLeft + 25f, ConsolePropTop + 15f);
            }
            else if (isUnitCasting)
            {
                g.DrawString("选择目标单位以进行\r\nEX技能攻击。", Define.FontSystem12, Define.WhiteBrush, ConsolePropLeft + 25f, ConsolePropTop + 15f);
            }
            else if (currentSelectedUnit.shown && !isFactoryRunning)
            {
                Unit cu = currentSelectedUnit.unit;
                if (cu.Force == OurForce)
                {
                    if(cu.IsThisRoundMoved)
                    {

                    }
                    else
                    {
                        for (int i = 0; i < unitOrdersBuffers.Length; i++)
                        {
                            bool isSelectedAct = i == currentUnitAction;
                            Brush brush = isSelectedAct ? Define.BlackBrush : Define.WhiteBrush;
                            if (isSelectedAct)
                                g.DrawLine(Define.RectLinePenWhite, ConsolePropLeft + 34f + i * 30f, ConsolePropTop + 15f, ConsolePropLeft + 34f + i * 30f, ConsolePropTop + 75f);
                            g.DrawString(unitOrdersBuffers[i], Define.FontSystem12, brush, ConsolePropLeft + 25f + i * 30f, ConsolePropTop + 30f);
                        }
                    }
                }
                else
                {
                    g.DrawString("敌方单位", Define.FontSystem16Bold, Define.WhiteBrush, ConsolePropLeft + 25f, ConsolePropTop + 40f);
                }
            }
        }

        private void DrawFactoryList(Graphics g)
        {
            if (isVictory)
            {
                return;
            }
            float flLeft = factorySplitLineLeft + 15f;
            float flTop = ConsoleTop + 10f;
            g.DrawString("可制造单位列表【使用左右键切换建造单位类型】", Define.FontSystem, Define.YellowBrush, flLeft - 5f, flTop - 5f);
            g.DrawString("◀", Define.FontSystem16Bold, Define.YellowBrush, flLeft, flTop + 32f);
            g.DrawString("▶", Define.FontSystem16Bold, Define.YellowBrush, flLeft + 480f, flTop + 35f);
            if(currentSelectedManufactureUnit == null)
                currentSelectedManufactureUnit = isHumanRound ? manufactureUnitTypesB.First() : manufactureUnitTypesR.First();
            Image icon = isHumanRound ? Define.UnitTypesImageMapB[currentSelectedManufactureUnit.Icon] : Define.UnitTypesImageMapR[currentSelectedManufactureUnit.Icon];
            g.DrawImage(icon, flLeft + 20f, flTop + 28f, factoryIconSize, factoryIconSize);
            //g.DrawRectangle(Define.FramePenWhite2, rectFactoryList);
            Unit fu = currentSelectedManufactureUnit;
            g.DrawString($"Cost:{fu.Cost}", Define.FontSystem12, Define.YellowBrush, flLeft + 370, flTop - 5f);
            g.DrawString($"Name:{fu.Name}", Define.FontSystem, Define.WhiteBrush, flLeft + 55f, flTop + 21f);
            g.DrawString($"EX:{fu.EXSkill}", Define.FontSystem, Define.WhiteBrush, flLeft + 250f, flTop + 21f);
            g.DrawString($"HPMax:{fu.Life}", Define.FontSystem, Define.WhiteBrush, flLeft + 55f, flTop + 36f);
            g.DrawString($"Attack:{fu.Damage}", Define.FontSystem, Define.WhiteBrush, flLeft + 250f, flTop + 36f);
            g.DrawString($"Armor:{fu.Armor}", Define.FontSystem, Define.WhiteBrush, flLeft + 55f, flTop + 51f);
            g.DrawString($"Move:{fu.Move}", Define.FontSystem, Define.WhiteBrush, flLeft + 250f, flTop + 51f);
        }

        private void ChangeFactorySelectUnit(int index)
        {
            Task.Run(() =>
            {
                List<Unit> units = isHumanRound ? manufactureUnitTypesB : manufactureUnitTypesR;
                currentSelectedManufactureUnit = units[index];
            });
        }

        private string[] unitOrdersBuffers = new string[] { "移\n动", "攻\n击", "取\n消" };
        private bool mainRanderThreadRunning = false;
        private void DrawScene(Graphics g)
        {
            if (currentMap == null) return;
            if (isVictory)
            {
                int sLen = humanGameOverTitle.Length;
                g.DrawString(humanGameOverTitle,
                    Define.FontSystem16Bold, Brushes.AliceBlue,
                    new PointF((clientSize.Width - sLen * 16f) / 2, clientSize.Height / 2));
                return;
            }
            // scene frame
            g.DrawLine(Define.FramePenWhite2, 20f, csh - 125f, csw - 10f, csh - 125f);
            g.DrawLine(Define.FramePenWhite2, 20f, 15f, csw - 10f, 15f);
            g.DrawLine(Define.FramePenWhite2, 20f, 15f, 20f, csh - 125f);
            g.DrawLine(Define.FramePenWhite2, csw - 10f, 15f, csw - 10f, csh - 125f);
            int maxL = currentScreenGridLeft + ScreenGridMaxWidth;
            int maxT = currentScreenGridTop + ScreenGridMaxHeight;
            int minL = currentScreenGridLeft;
            int minT = currentScreenGridTop;
            for (int i = minL; i < maxL; i++)
            {
                for (int j = minT; j < maxT; j++)
                {
                    float x = i - minL;
                    float y = j - minT;
                    tileRect.X = gapX + x * ScreenTileSize;
                    tileRect.Y = gapY + y * ScreenTileSize;
                    tileRect.Width = ScreenTileSize + 1;
                    tileRect.Height = ScreenTileSize + 1;
                    g.DrawImage(Define.TileMap[currentMap[i, j]], tileRect);
                }
            }
            // 绘制单位
            foreach(Unit u in sceneUnits)
            {
                if (u.GridLocX < currentScreenGridLeft || u.GridLocY < currentScreenGridTop)
                    continue;
                if (u.GridLocX >= currentScreenGridLeft + ScreenGridMaxWidth || u.GridLocY >= currentScreenGridTop + ScreenGridMaxHeight)
                    continue;
                int uGridX = u.GridLocX - currentScreenGridLeft;
                int uGridY = u.GridLocY - currentScreenGridTop;
                if(u.Force == "R")
                {
                    g.DrawImage(Define.UnitTypesImageMapR[u.Icon], new RectangleF(
                        gapX + uGridX * ScreenTileSize,
                        gapY + uGridY * ScreenTileSize,
                        ScreenTileSize, ScreenTileSize));
                }
                else
                {
                    g.DrawImage(Define.UnitTypesImageMapB[u.Icon], new RectangleF(
                        gapX + uGridX * ScreenTileSize,
                        gapY + uGridY * ScreenTileSize,
                        ScreenTileSize, ScreenTileSize));
                }
                g.DrawString($"Lv:{u.Level}", Define.FontUnitLevel, Brushes.Yellow, gapX + uGridX * ScreenTileSize, gapY + uGridY * ScreenTileSize - 2f);
                if (u.IsThisRoundMoved)
                {
                    g.DrawString("E", Define.FontSystem16Bold, Brushes.Yellow, gapX + uGridX * ScreenTileSize + 19f, gapY + uGridY * ScreenTileSize + 16f);
                }
            }
            if (isUnitMoving && Game.CurrentFrameSec % 2 == 0 && ShowMoveGrids)
            {
                //Logger.Log($"绘制可移动格子{gapX + 20f + (unitMoveGrids[0].X - currentScreenLeft) * 40f},{gapY + 20f + (unitMoveGrids[0].Y - currentScreenTop) * 40f}");
                //Logger.Log($"绘制可移动格子{gapX + 20f + (unitMoveGrids[1].X - currentScreenLeft) * 40f},{gapY + 20f + (unitMoveGrids[1].Y - currentScreenTop) * 40f}");
                // 绘制可移动区域
                foreach (var i in unitMoveGrids)
                {
                    if (i.Y >= currentScreenGridTop + ScreenGridMaxHeight ||
                        i.X >= currentScreenGridLeft + ScreenGridMaxWidth ||
                        i.Y < currentScreenGridTop ||
                        i.X < currentScreenGridLeft)
                        continue;

                    g.DrawLine(Define.penMoveGrid,
                        gapX + (i.X - currentScreenGridLeft) * 40f, gapY + 20f + (i.Y - currentScreenGridTop) * 40f,
                        gapX + (i.X - currentScreenGridLeft + 1) * 40f, gapY + 20f + (i.Y - currentScreenGridTop) * 40f);
                }
            }

            if (isUnitCasting)
            {
                if (exSkillCastTarget == null) return;
                Unit u = exSkillCastTarget;
                byte tX = (byte)(u.GridLocX - currentScreenGridLeft);
                byte tY = (byte)(u.GridLocY - currentScreenGridTop);
                // up
                g.DrawLine(Define.FramePenRed5,
                    gapX + tX * ScreenTileSize,
                    gapY + tY * ScreenTileSize,
                    gapX + (tX + 1) * ScreenTileSize,
                    gapY + tY * ScreenTileSize);
                // down
                g.DrawLine(Define.FramePenRed5,
                    gapX + tX * ScreenTileSize,
                    gapY + (tY + 1) * ScreenTileSize,
                    gapX + (tX + 1) * ScreenTileSize,
                    gapY + (tY + 1) * ScreenTileSize);
                // left
                g.DrawLine(Define.FramePenRed5,
                    gapX + tX * ScreenTileSize,
                    gapY + tY * ScreenTileSize,
                    gapX + tX * ScreenTileSize,
                    gapY + (tY + 1) * ScreenTileSize);
                // right
                g.DrawLine(Define.FramePenRed5,
                    gapX + (tX + 1) * ScreenTileSize,
                    gapY + tY * ScreenTileSize,
                    gapX + (tX + 1) * ScreenTileSize,
                    gapY + (tY + 1) * ScreenTileSize);
            }
            // 绘制选择框
            byte cstX = currentScreenTargetTileX;
            byte cstY = currentScreenTargetTileY;
            // up
            g.DrawLine(Define.FramePenWhite5, 
                gapX + cstX * ScreenTileSize, 
                gapY + cstY * ScreenTileSize,
                gapX + (cstX + 1) * ScreenTileSize,
                gapY + cstY * ScreenTileSize);
            // down
            g.DrawLine(Define.FramePenWhite5,
                gapX + cstX * ScreenTileSize,
                gapY + (cstY + 1) * ScreenTileSize,
                gapX + (cstX + 1) * ScreenTileSize,
                gapY + (cstY + 1) * ScreenTileSize);
            // left
            g.DrawLine(Define.FramePenWhite5,
                gapX + cstX * ScreenTileSize,
                gapY + cstY * ScreenTileSize,
                gapX + cstX * ScreenTileSize,
                gapY + (cstY + 1) * ScreenTileSize);
            // right
            g.DrawLine(Define.FramePenWhite5,
                gapX + (cstX + 1) * ScreenTileSize,
                gapY + cstY * ScreenTileSize,
                gapX + (cstX + 1) * ScreenTileSize,
                gapY + (cstY + 1) * ScreenTileSize);
            // efforts
            if (displayGraphicsActionFlag)
                graphicsAction?.Invoke(g);
            if (Game.IsDamageDisplay)
            {
                //Console.WriteLine($"显示伤害:{Game.CurrentDamage}");
                string dmgText = Game.CurrentAttackDamage == 0 ? "未命中" : $"敌方HP -{Game.CurrentAttackDamage}";
                g.DrawString(dmgText, Define.FontSystem, Brushes.Orange, Game.DamageDisplayX, Game.DamageDisplayY);
            }
            if(!mainRanderThreadRunning)
                mainRanderThreadRunning = true;
        }



        private void DrawGameMenu(Graphics g)
        {
            int left = 100;
            int top = 70;
            int width = csw - 2 * left;
            int height = csh - 2 * top;
            g.DrawRectangle(Define.FramePenWhite5, left, top, width, height);
            g.DrawString("游戏菜单", Define.FontYaHei22, Define.WhiteBrush, new RectangleF(left + 240f, top + 30f, 200f, 60f));
            g.DrawString("返回游戏", Define.FontYaHei22, Define.WhiteBrush, new RectangleF(left + 240f, top + 150f, 220f, 60f));
            g.DrawString("退出游戏", Define.FontYaHei22, Define.WhiteBrush, new RectangleF(left + 240f, top + 270f, 220f, 60f));
            switch (gameMenuOption)
            {
                case 0:
                    g.DrawString("➡", Define.FontYaHei22, Define.WhiteBrush, new RectangleF(left + 190f, top + 150f, 40f, 40f));
                    break;
                case 1:
                    g.DrawString("➡", Define.FontYaHei22, Define.WhiteBrush, new RectangleF(left + 190f, top + 270f, 40f, 40f));
                    break;
            }
        }

        /// <summary>
        /// 放置单位
        /// </summary>
        /// <param name="u"></param>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        private void PushUnit(Unit u,byte gridX,byte gridY)
        {
            if(!u.IsFactory && !u.IsHome)
            {
                if (u.Force == "R")
                {
                    rCount++;
                }
                else
                {
                    bCount++;
                }
            }
            u.GridLocX = gridX;
            u.GridLocY = gridY;
            sceneUnits.Add(u);
        }

        /// <summary>
        /// 摧毁单位
        /// </summary>
        /// <param name="u"></param>
        private void DestroyUnit(Unit u)
        {
            lock (sceneUnits)
            {
                Console.WriteLine($"{u.Name}被消灭");
                sceneUnits.Remove(u);
                int reward = (u.Cost / 2);
                if (u.Force == "R")
                {
                    rCount--;
                    GoldB += reward;
                    Console.WriteLine($"我方奖励 {reward}  金钱");
                }
                else
                {
                    bCount--;
                    GoldR += reward;
                    Console.WriteLine($"敌方奖励 {reward}  金钱");
                }
            }
        }

        /// <summary>
        /// 绘制移动单位的格子
        /// </summary>
        /// <param name="u"></param>
        private void GenUnitMoveGrids(Unit u)
        {
            unitMoveGrids = CWMath.GetUnitRoundTiles(u);
            isUnitMoving = true;
        }

        /// <summary>
        /// 移动单位
        /// </summary>
        /// <param name="u"></param>
        /// <param name="dstGridX"></param>
        /// <param name="dstGridY"></param>
        private void MoveUnit(Unit u,byte dstGridX,byte dstGridY)
        {
            ShowMoveGrids = true;
            Task.Run(() => Game.Animes.MoveUnit(u, dstGridX, dstGridY));
            //Game.Animes.MoveUnit(u,dstGridX,dstGridY);
        }

        /// <summary>
        /// 生成目标群以选择单位释放Ex技
        /// </summary>
        /// <param name="u"></param>
        private void GenEXSkillTargets(Unit u)
        {
            IEnumerable<Unit> targets = CWMath.GetEnemysByEXSkillCastRange(u);
            if(targets.Count() == 0)
            {
                Game.SetWarningMessage("附近无EX技能可以\r\n攻击到的目标。", 2);
                isUnitCommandPanelShowing = true;
                isUnitCasting = false;
                return;
            }
            Console.WriteLine($"获取到EX技攻击目标数：{targets.Count()}");
            exSkillTargets.AddRange(targets);
            currentExSkillCastTargetIndex = 0;
            ChangeEXSkillCastTarget(currentExSkillCastTargetIndex);
            isUnitCasting = true;
        }

        /// <summary>
        /// 改变EX技能攻击目标
        /// </summary>
        /// <param name="targetIndex"></param>
        private void ChangeEXSkillCastTarget(int targetIndex)
        {
            if (exSkillTargets.Count == 0 || targetIndex < 0 || targetIndex >= exSkillTargets.Count)
                return;
            exSkillCastTarget = exSkillTargets[targetIndex];
            FocusTo(exSkillCastTarget.GridLocX,exSkillCastTarget.GridLocY);
            Console.WriteLine($"切换EX技能攻击目标：{exSkillCastTarget.Name}");
        }

        private Unit exSkillCastTarget = null;

        /// <summary>
        /// 释放ex技能
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        private void CastEXSkill(Unit src,Unit dst)
        {
            Task.Run(() => Anime.Instance.CastEXSkill(src, dst));
            castStep = 0;
        }

        private void OrderUnit(int option)
        {
            bool isActiveUnit = currentSelectedUnit.IsActiveUnit;
            switch (currentUnitAction)
            {
                case 0:
                    if (isActiveUnit)
                        GenUnitMoveGrids(currentSelectedUnit.unit);
                    break;
                case 1:
                    if(isActiveUnit && !currentSelectedUnit.unit.EXSkill.Equals(EXSkill.None))
                        GenEXSkillTargets(currentSelectedUnit.unit);
                    break;
                case 2:
                    SetUnitPropertiesShown(currentSelectedUnit.unit, false);
                    break;
            }
            if (!isActiveUnit || option == 2)
            {
                currentUnitAction = 0;
                isUnitCommandPanelShowing = false;
            }
            //isGameMenuShowing = false;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Clear()
        {
            sceneUnits.Clear();
            rCount = 0;
            bCount = 0;
            currentScreenTargetTileX = 0;
            currentScreenTargetTileY = 0;
            currentScreenGridTop = 0;
            currentScreenGridLeft = 0;
            currentSelectedUnit.shown = false;
            isUnitCasting = false;
            isUnitCommandPanelShowing = false;
            isGameMenuShowing = false;
            isUnitMoving = false;
            currentUnitAction = 0;
            isUnitSelected = false;
            Game.AllowKeyEvent = true;
            SetUnitPropertiesShown(null, false);
            manufactureUnitTypesR.Clear();
            manufactureUnitTypesB.Clear();
            isHumanRound = true;
            isVictory = false;
            victoryForce = string.Empty;
            exSkillTargets.Clear();
        }

        private void LoadMapSetting(string settingText)
        {
            var settingLines = settingText.Split('\n');
            if (settingLines.Length < 3) return;
            for (int i = 0; i < settingLines.Length; i++)
            {
                string line = settingLines[i];
                if (line.Contains("BEGIN")) continue;
                else if (line.Contains("END")) break;
                string[] words = line.Split(' ');
                if (words[0] == "SETGOLD")
                {
                    if (words[1] == "R")
                        GoldR = int.Parse(words[2]);
                    else
                        GoldB = int.Parse(words[2]);
                    Logger.Log($"配置金钱 势力：{words[1]} 数量：{words[2]}");
                }
                else if (words[0] == "SETSTART")
                {
                    string[] loc = words[2].Split(',');
                    Unit cu = CloneUnitById(0);
                    cu.Force = words[1];
                    PushUnit(cu, byte.Parse(loc[0]), byte.Parse(loc[1]));
                    Logger.Log($"设置主机 势力：{words[1]} 位置：[{loc[0]},{loc[1]}]");
                }
                else if(words[0] == "SETFACTORY")
                {
                    string[] loc = words[2].Split(',');
                    Unit cu = CloneUnitById(1);
                    cu.Force = words[1];
                    PushUnit(cu, byte.Parse(loc[0]), byte.Parse(loc[1]));
                    Logger.Log($"设置制造车间 势力：{words[1]} 位置：[{loc[0]},{loc[1]}]");
                }
                else if (words[0] == "SETUNIT")
                {
                    string[] loc = words[3].Split(',');
                    int id = int.Parse(words[2]);
                    Unit cu = CloneUnitById(id);
                    cu.Force = words[1];
                    if(Define.DefineUnitEXSkills.ContainsKey(id))
                        cu.EXSkill = Define.DefineUnitEXSkills[id];
                    cu.Level = int.Parse(words[4]);
                    PushUnit(cu, byte.Parse(loc[0]), byte.Parse(loc[1]));
                    Logger.Log($"设置单位 单位名{cu.Name} 势力：{words[1]} 位置：[{loc[0]},{loc[1]}] 等级：[{words[4]}]");
                }
                else if(words[0] == "DEFINEMANUFACTUREUNITTYPES")
                {
                    string force = words[1];
                    IEnumerable<byte> types = words[2].Split(',').Select(x => byte.Parse(x));
                    if (force == "R")
                    {
                        manufactureUnitTypesR.AddRange(types.Select(x => Define.DefineUnitTypes[x]));
                    }
                    else
                    {
                        manufactureUnitTypesB.AddRange(types.Select(x => Define.DefineUnitTypes[x]));
                    }
                    Logger.Log($"加载地图可生产单位类型 {force} {types}");
                }
                else if (words[0] == "SETROUNDGOLDGAIN")
                {
                    string force = words[1];
                    if (force == "R")
                        roundGoldGainR = int.Parse(words[2]);
                    else
                        roundGoldGainB = int.Parse(words[2]);
                    Logger.Log($"加载每回合结束金钱收益 {force} Gold:{words[2]}");
                }
                else if(words[0] == "SETUNITBUILDLEVEL")
                {
                    string force = words[1];
                    if (force == "R")
                        buildUnitLevelR = int.Parse(words[2]) * 2 + 1;
                    else
                        buildUnitLevelB = int.Parse(words[2]) * 2 + 1;
                    Logger.Log($"加载初始单位等级 {force} 单位品阶:{words[2]}");
                }
            }
            isHumanRound = true;
        }

        private Unit CloneUnitById(int unitId)
        {
            return Unit.Clone(Define.DefineUnitTypes[unitId]);
        }

        private void SelectTile(byte absCurrentTargetTileX, byte absCurrentTargetTileY)
        {
            Unit u = FindUnit(absCurrentTargetTileX, absCurrentTargetTileY);
            if (u != null)
            {
                Logger.Log(u.Name);
                if (u.IsFactory)
                {
                    if (manufactureUnitTypesB.Min(x => x.Cost) > GoldB)
                    {
                        Game.SetWarningMessage("金钱不足以生产最低\r\n价格的单位，无法生产。", 2);
                        u.IsThisRoundMoved = true;
                        SetFactoryModeDisabled();
                        CheckRoundEnd();
                    }
                    else if (u.IsThisRoundMoved)
                    {
                        Game.SetWarningMessage("无法生产。", 2);
                        isFactoryRunning = true;
                    }
                    else
                    {
                        isFactoryRunning = true;
                    }
                }
                SetUnitPropertiesShown(u, true);
            }
            else
                SetUnitPropertiesShown(null,false);
        }

        private void Init()
        {
            ShowMoveGrids = true;
            Anime.AnimeFinished += args =>
            {
                isUnitSelected = false;
                currentSelectedUnit.shown = false;
                isUnitMoving = false;
                isUnitCasting = false;
                isUnitCommandPanelShowing = false;
                Unit u = args.SrcUnit;
                Unit dU = args.DstUnit;
                switch (args.AnimeType)
                {
                    #region Move
                    case AnimeTypes.Move:
                        isUnitMoving = false;
                        ShowMoveGrids = true;
                        currentSelectedUnit.unit.IsThisRoundMoved = true ; //测试的话就无限移动
                        IEnumerable<Unit> tUnits = sceneUnits.Where(x => x.GridLocX == u.GridLocX && x.GridLocY == u.GridLocY && x.Force != u.Force);
                        int dur = 0;
                        if(tUnits.Count() > 0) // 如果移动到的地方有敌方单位
                        {
                            IEnumerable<Unit> sUnits = tUnits.Where(x => !x.IsFactory && !x.IsHome);
                            if(tUnits.Count() - sUnits.Count() >= 1)// 说明有单位叠在建筑上
                            {
                                Unit home = tUnits.FirstOrDefault(x => x.IsHome);
                                if(sUnits.Count() > 0)
                                {
                                    Unit dst = sUnits.First();
                                    int beforeCount = dst.Force == "B" ? bCount : rCount;
                                    AttackUnitNormal(currentSelectedUnit.unit, dst);
                                    int afterCount = dst.Force == "B" ? bCount : rCount;
                                    if (home != null && beforeCount > afterCount)
                                    {
                                        Victory(u.Force);
                                    }
                                }
                            }
                            else
                            {
                                Unit fUnit = tUnits.First();
                                if(!fUnit.IsBackGround)
                                    AttackUnitNormal(currentSelectedUnit.unit, tUnits.First());
                                else if(fUnit.IsHome)
                                    Victory(u.Force);
                            }
                            dur = 3;
                        }
                        else
                        {
                            // 工厂覆盖检测
                            Unit factory = sceneUnits.FirstOrDefault(x => x.IsFactory && u.Force == x.Force && x.GridLocX == u.GridLocX && x.GridLocY == u.GridLocY);
                            if (factory != null)
                            {
                                //如果工厂被压住了则工厂本回合不再生产
                                factory.IsThisRoundMoved = true;
                            }
                        }
                        CheckRoundEnd(dur);
                        break;
                    #endregion
                    case AnimeTypes.Cast:
                        currentSelectedUnit.unit.IsThisRoundMoved = true; //测试的话就无限移动
                        CastDamage(u, dU);
                        isUnitCasting = false;
                        exSkillTargets.Clear();
                        Unit factory1 = sceneUnits.FirstOrDefault(x => x.IsFactory && u.Force == x.Force && x.GridLocX == u.GridLocX && x.GridLocY == u.GridLocY);
                        if (factory1 != null)
                        {
                            //如果工厂被压住了则工厂本回合不再生产
                            factory1.IsThisRoundMoved = true;
                        }
                        CheckRoundEnd(1);
                        break;
                }
            };
        }

        /// <summary>
        /// 常规攻击单位
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void AttackUnitNormal(Unit src,Unit dst)
        {
            int dmg = CWMath.GetDamage(src.Damage, dst.Armor, 0.8f, src.Level, dst.Level);
            dst.Life -= dmg;
            byte gridX = dst.GridLocX;
            byte gridY = dst.GridLocY;
            int actuallX = (int)(gapX + (gridX - currentScreenGridLeft) * ScreenTileSize);
            int actuallY = (int)(gapY + (gridY - currentScreenGridTop) * ScreenTileSize + 20f);
            Game.DisplayDamage(dmg, actuallX - 1, actuallY, 2);
            if (dst.Life < 0)
            {
                src.Level = src.Level < 25 ? src.Level += 1 : 25;
                DestroyUnit(dst);
            }
            else
            {
                var rounds = CWMath.GetUnitRoundTilesByGridNum(dst, 1);
                Console.WriteLine($"获取撤退格子数{rounds.Count}");
                if (rounds.Count > 0)
                {
                    var p = rounds.First();
                    src.GridLocX = (byte)p.X;
                    src.GridLocY = (byte)p.Y;
                    if (FindUnits(src.GridLocX, src.GridLocY).Where(x => x.IsHome && x.Force != src.Force).Count() > 0)
                        Victory(src.Force);
                }
                else
                {
                    Console.WriteLine("没有地方放置撤退单位，单位死亡");
                    DestroyUnit(src);
                }
            }
            Console.WriteLine($"{src.Name}[LV:{src.Level}] 对 {dst.Name}[LV:{dst.Level}] 造成伤害{dmg}");
        }

        /// <summary>
        /// 释放技能伤害
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void CastDamage(Unit src,Unit dst)
        {
            int dmg = CWMath.GetDamage(src.EXSkill.EXSkillDmgExtra + src.Damage, dst.Armor, 0.6f, src.Level, dst.Level);
            dst.Life -= dmg;
            byte gridX = dst.GridLocX;
            byte gridY = dst.GridLocY;
            int actuallX = (int)(gapX + (gridX - currentScreenGridLeft) * ScreenTileSize);
            int actuallY = (int)(gapY + (gridY - currentScreenGridTop) * ScreenTileSize + 20f);
            Game.DisplayDamage(dmg, actuallX - 1, actuallY, 2);
            if (dst.Life < 0)
            {
                src.Level = src.Level < 25 ? src.Level += 1 : 25;
                DestroyUnit(dst);
            }
            Console.WriteLine($"{src.Name}[LV:{src.Level}] 对 {dst.Name}[LV:{dst.Level}] 施放技能：{src.EXSkill.EXSkillName} 造成伤害{dmg}");
        }


        /// <summary>
        /// 检测用户/电脑回合是否结束
        /// </summary>
        private void CheckRoundEnd(int waitSeconds = 0)
        {
            Task.Run(() =>
            {
                if (waitSeconds > 0)
                    Thread.Sleep(waitSeconds * 1000);
                int count = 0;
                int movedUnitCount = 0;
                string tokenForce = isHumanRound ? "B" : "R";
                IEnumerable<Unit> unitList = sceneUnits.Where(x => x.Force == tokenForce && !x.IsHome);
                count = unitList.Count();
                movedUnitCount = unitList.Where(x => x.IsThisRoundMoved).Count();
                if (movedUnitCount == count)
                {
                    SetUnitPropertiesShown(null, false);
                    GoldB += roundGoldGainB;
                    GoldR += roundGoldGainR;
                    if (isHumanRound)
                    {
                        var g = GoldR;
                        currentAI?.RoundBeginBehaviour(ref g);
                        GoldR = g;
                        Console.WriteLine("轮次换为电脑出战");
                        currentAI?.DoSelectUnit();
                    }
                    else
                    {
                        SetUnitPropertiesShown(null, false);
                        Console.WriteLine("轮次换为玩家出战");
                    }
                    foreach (var unit in unitList)
                    {
                        if(unit.IsFactory && FindUnits(unit.GridLocX,unit.GridLocY).Where(x => x.Force != unit.Force).Count() > 0)
                            continue;
                        unit.IsThisRoundMoved = false;
                    }
                    isHumanRound = isHumanRound != true;
                }
                else
                {
                    if (isHumanRound == false) 
                        currentAI?.DoSelectUnit();
                }
                ShowMoveGrids = true;
            });
        }

        /// <summary>
        /// 关闭工厂
        /// </summary>
        private void SetFactoryModeDisabled()
        {
            isUnitSelected = false;
            currentSelectedUnit.shown = false;
            isUnitMoving = false;
            isUnitCasting = false;
            isFactoryRunning = false;
            currentManufactureUnitIndex = 0;
            SetUnitPropertiesShown(null, false);
            Console.WriteLine("取消工厂生产");
        }

        /// <summary>
        /// 命令工厂建造单位(AI需要填参数)
        /// </summary>
        /// <param name="aiBuildUnit"></param>
        /// <param name="factory"></param>
        public void OrderFactoryBuildUnit(Unit factory = null,Unit aiBuildUnit = null)
        {
            Task.Run(() =>
            {
                int money = isHumanRound ? GoldB : GoldR;
                Unit mU = null;
                if (isHumanRound)
                {
                    mU = currentSelectedManufactureUnit;
                    if (money < mU.Cost)
                    {
                        Game.SetWarningMessage("你没有足够的金钱\r\n来建造单位。", 2);
                        return;
                    }
                    GoldB -= mU.Cost;
                }
                else if (factory != null)
                {
                    if(aiBuildUnit != null)
                    {
                        mU = aiBuildUnit;
                        GoldR -= mU.Cost;
                    }
                    else
                    {
                        factory.IsThisRoundMoved = true;
                        SetFactoryModeDisabled();
                        CheckRoundEnd();
                        return;
                    }
                }
                else
                {
                    return;
                }
                Unit dU = isHumanRound ? currentSelectedUnit.unit : factory;
                int index = currentManufactureUnitIndex;
                dU.IsThisRoundMoved = true; //测试期间移除
                Unit nU = Unit.Clone(mU);
                nU.Force = isHumanRound ? "B" : "R";
                nU.Level = isHumanRound ? buildUnitLevelR : buildUnitLevelR;
                if (Define.DefineUnitEXSkills.ContainsKey(nU.Id))
                    nU.EXSkill = Define.DefineUnitEXSkills[nU.Id];
                Console.WriteLine(nU.Force);
                //nU.GridLocX = currentSelectedUnit.unit.GridLocX;
                //nU.GridLocY = currentSelectedUnit.unit.GridLocY;
                PushUnit(nU, dU.GridLocX, dU.GridLocY);
                lock (sceneUnits)
                {
                    sceneUnits.Add(nU);
                }
                // TODO Logic for build
                SetFactoryModeDisabled();
                CheckRoundEnd();
            });
        }

        /// <summary>
        /// 胜利
        /// </summary>
        /// <param name="force"></param>
        private void Victory(string force)
        {
            isVictory = true;
            victoryForce = force;
            humanGameOverTitle = victoryForce == "R" ? defautTitle : victoryTitle;
        }

        // scene parameters
        // =====BEGIN=====
        public const byte ScreenGridMaxWidth = 18;// 视野最大横向网格数
        public const byte ScreenGridMaxHeight = 10;// 视野最大纵向网格数
        public const byte ScreenTileSize = 40;
        private static Point currentScreenTargetTile => new Point(currentScreenTargetTileX, currentScreenTargetTileY);
        private static byte currentScreenTargetTileX = 0;
        private static byte currentScreenTargetTileY = 0;
        private static byte absCurrentTargetTileX => (byte)(currentScreenGridLeft + currentScreenTargetTileX);
        private static byte absCurrentTargetTileY => (byte)(currentScreenGridTop + currentScreenTargetTileY);
        private static byte currentScreenGridLeft = 0;
        private static byte currentScreenGridTop = 0;
        // =====END=====

        private bool isSceneRunning = false;
        private bool isGameMenuShowing = false;
        private Size clientSize;
        private int csw;
        private int csh;
        private int gameMenuOption = 0;
        private Map currentMap = null;
        private static Scene _Instance;
        private const float gapX = 37f;
        private const float gapY = 23f;
        private static RectangleF tileRect = new RectangleF();
        private float cTop => csh - 110f;
        private float cLeft => 20f;
        private float cRight => csw - 10f;
        private float cBottom => csh - 10f;
        private float cSpLeft => csw - 240f;

        /// <summary>
        /// 是否为玩家回合
        /// </summary>
        public static bool IsHumanRound => Instance.isHumanRound;

        // =====Units=====
        private static bool isUnitSelected = false;
        private static readonly List<Unit> sceneUnits = new List<Unit>();
        private static byte rCount = 0;
        private static byte bCount = 0;
        // =====End=====
        private CurrentSelectedUnitInfo currentSelectedUnit;
        private RectangleF rectUnitIcon = new RectangleF();
        private const string OurForce = "B";
        // ======2022 2 10======
        /// <summary>
        /// 单位行动指令索引（移动，EX技，取消）
        /// </summary>
        private byte currentUnitAction = 0;
        private List<Point> unitMoveGrids = new List<Point>();
        private bool isUnitMoving;
        private bool isUnitCasting;
        private bool isUnitCommandPanelShowing = false;
        private bool isHumanRound = false;
        // 2022.2.13 
        // 工厂配置
        private List<Unit> manufactureUnitTypesR = new List<Unit>();
        private List<Unit> manufactureUnitTypesB = new List<Unit>();
        private bool isFactoryRunning = false;
        private byte currentManufactureUnitIndex = 0;
        private const float factorySplitLineLeft = 250f;
        //DEFINEMANUFACTUREUNITTYPES
        private const byte factoryIconSize = 32;
        private static Unit currentSelectedManufactureUnit = null;
        //private static Rectangle rectFactoryUnit = new Rectangle(260,);
        // 2022.2.15
        private static bool isVictory = false;
        private static string victoryForce = string.Empty;
        private const string victoryTitle = "恭喜你，你获得了最终的胜利！";
        private const string defautTitle = "Game Over";
        private static string humanGameOverTitle = string.Empty;
        // 2022.2.16
        private AIBase currentAI;
        private int roundGoldGainB = 50;
        private int roundGoldGainR = 50;
        private int buildUnitLevelR = 0;
        private int buildUnitLevelB = 0;
        // 2022.2.19
        private List<Unit> exSkillTargets = new List<Unit>();
        private int currentExSkillCastTargetIndex;
        private int castStep = 0;

        #region AI
        private void RunAIScript()
        {

        }

        #endregion

        private class CurrentSelectedUnitInfo
        {
            internal Unit unit;

            internal bool shown;

            internal Image Icon { get; set; }

            /// <summary>
            /// 获取 长度为5的单位属性数组[名称,HP,攻击,护甲,移动力]
            /// </summary>
            internal string[] infos;

            /// <summary>
            /// EX技能
            /// </summary>
            internal string EXSkillInfo { get; set; }
            /// <summary>
            /// 是否为动态单位（可以显示额外属性
            /// </summary>
            public bool IsActiveUnit { get; internal set; }
        }
    }
}
