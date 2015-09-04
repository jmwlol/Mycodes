﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace D_RekSai
{
    internal class Program
    {
        private const string ChampionName = "RekSai";

        private static Orbwalking.Orbwalker _orbwalker;

        private static Spell _q, _bq, _w, _e, _be, _r;

        private static bool _qcreated = false;

        private static Menu _config;

        private static Menu TargetSelectorMenu;

        private static Obj_AI_Hero _player;

        public static readonly List<Spell> SpellList = new List<Spell>();

        private static SpellSlot _igniteSlot;

        private static Items.Item _tiamat, _hydra, _blade, _bilge, _rand, _lotis;

        private static bool _burrowed;

        private static bool _unburrowed;

        private static SpellSlot _smiteSlot = SpellSlot.Unknown;

        private static Spell _smite;

        //Credits to Kurisu
        private static readonly int[] SmitePurple = {3713, 3726, 3725, 3726, 3723};
        private static readonly int[] SmiteGrey = {3711, 3722, 3721, 3720, 3719};
        private static readonly int[] SmiteRed = {3715, 3718, 3717, 3716, 3714};
        private static readonly int[] SmiteBlue = {3706, 3710, 3709, 3708, 3707};

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            _q = new Spell(SpellSlot.Q);
            _bq = new Spell(SpellSlot.Q, 1450);
            _w = new Spell(SpellSlot.W, 200f);
            _e = new Spell(SpellSlot.E, 250f);
            _be = new Spell(SpellSlot.E, 700);
            _r = new Spell(SpellSlot.R);

            _bq.SetSkillshot(0.5f, 60, 1950, true, SkillshotType.SkillshotLine);

            SpellList.Add(_q);
            SpellList.Add(_bq);
            SpellList.Add(_w);
            SpellList.Add(_e);
            SpellList.Add(_be);
            SpellList.Add(_r);

            _bilge = new Items.Item(3144, 475f);
            _blade = new Items.Item(3153, 425f);
            _hydra = new Items.Item(3074, 250f);
            _tiamat = new Items.Item(3077, 250f);
            _rand = new Items.Item(3143, 490f);
            _lotis = new Items.Item(3190, 590f);


            _igniteSlot = _player.GetSpellSlot("SummonerDot");
            SetSmiteSlot();

            //D Diana
            _config = new Menu("D-RekSai", "D-RekSai", true);

            //TargetSelector
            TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            _config.AddSubMenu(TargetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Combo
            _config.AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("Combo").AddItem(new MenuItem("UseIgnitecombo", "Use Ignite(rush for it)")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("smitecombo", "Use Smite in target")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            _config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
            _config.SubMenu("Combo")
                .AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));


            //Items public static Int32 Tiamat = 3077, Hydra = 3074, Blade = 3153, Bilge = 3144, Rand = 3143, lotis = 3190;
            _config.AddSubMenu(new Menu("items", "items"));
            _config.SubMenu("items").AddSubMenu(new Menu("Offensive", "Offensive"));
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Tiamat", "Use Tiamat")).SetValue(true);
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Hydra", "Use Hydra")).SetValue(true);
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Bilge", "Use Bilge")).SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BilgeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Bilgemyhp", "Or your Hp < ").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Blade", "Use Blade")).SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BladeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Blademyhp", "Or Your  Hp <").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items").AddSubMenu(new Menu("Deffensive", "Deffensive"));
            _config.SubMenu("items").SubMenu("Deffensive").AddSubMenu(new Menu("Cleanse", "Cleanse"));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("useqss", "Use QSS/Mercurial Scimitar/Dervish Blade"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("blind", "Blind"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("charm", "Charm"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("fear", "Fear"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("flee", "Flee"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("snare", "Snare"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("taunt", "Taunt"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("suppression", "Suppression"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("stun", "Stun"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("polymorph", "Polymorph"))
                .SetValue(false);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("silence", "Silence"))
                .SetValue(false);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("zedultexecute", "Zed Ult"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .SubMenu("Cleanse")
                .AddItem(new MenuItem("Cleansemode", "Use Cleanse"))
                .SetValue(new StringList(new string[2] {"Always", "In Combo"}));
            //_config.SubMenu("items").SubMenu("Deffensive").AddItem(new MenuItem("zhonyas", "Use Zhonyas")).SetValue(true); 
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omen", "Use Randuin Omen"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omenenemys", "Randuin if enemys>").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotis", "Use Iron Solari"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotisminhp", "Solari if Ally Hp<").SetValue(new Slider(35, 1, 100)));

            _config.SubMenu("items").AddSubMenu(new Menu("Potions", "Potions"));
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usehppotions", "Use Healt potion/Flask/Biscuit"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionhp", "If Health % <").SetValue(new Slider(35, 1, 100)));


            _config.AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E")).SetValue(true);
            _config.SubMenu("Harass").AddItem(new MenuItem("UseItemsharass", "Use Items")).SetValue(true);

            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("ActiveHarass", "Harass key").SetValue(new KeyBind("C".ToCharArray()[0],
                        KeyBindType.Press)));
            _config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("harasstoggle", "Harass(toggle)").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Toggle)));


            _config.AddSubMenu(new Menu("Farm", "Farm"));
            _config.SubMenu("Farm").AddSubMenu(new Menu("Lane", "Lane"));
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("UseQLane", "Use Q")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("UseWLane", "Use W")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("UseELane", "Use E")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("UseItemslane", "Use Items")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("Lane")
                .AddItem(
                    new MenuItem("ActiveLane", "Farm key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));


            //jungle
            _config.SubMenu("Farm").AddSubMenu(new Menu("Jungle", "Jungle"));
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("UseQJungle", "Use Q")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("UseWJungle", "Use W")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("UseEJungle", "Use E")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("Jungle")
                .AddItem(new MenuItem("UseItemsjungle", "Usem Items"))
                .SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("Jungle")
                .AddItem(
                    new MenuItem("ActiveJungle", "Jungle key").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));


            //Smite 
            _config.AddSubMenu(new Menu("Smite", "Smite"));
            _config.SubMenu("Smite")
                .AddItem(
                    new MenuItem("Usesmite", "Use Smite(toggle)").SetValue(new KeyBind("H".ToCharArray()[0],
                        KeyBindType.Toggle)));
            _config.SubMenu("Smite").AddItem(new MenuItem("Usered", "Smite Red Early ")).SetValue(true);
            _config.SubMenu("Smite")
                .AddItem(new MenuItem("healthJ", "Smite Red Early if HP% <").SetValue(new Slider(35, 1, 100)));

            //Extra
            _config.AddSubMenu(new Menu("Misc", "Misc"));
            _config.SubMenu("Misc").AddItem(new MenuItem("AutoW", "Auto W")).SetValue(true);
            _config.SubMenu("Misc")
                .AddItem(new MenuItem("AutoWHP", "Use W if HP is <= "))
                .SetValue(new Slider(25, 1, 100));
            _config.SubMenu("Misc")
                .AddItem(new MenuItem("AutoWMP", "Use W if Fury is >= "))
                .SetValue(new Slider(100, 1, 100));
            _config.SubMenu("Misc").AddItem(new MenuItem("Inter_W", "Use W to Interrupter")).SetValue(true);
           // _config.SubMenu("Misc").AddItem(new MenuItem("Gap_W", "GapClosers W")).SetValue(true);
            _config.SubMenu("Misc")
                .AddItem(new MenuItem("escapeterino", "Escape!!!"))
                .SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Press));
            //Kill Steal
            _config.AddSubMenu(new Menu("KillSteal", "Ks"));
            _config.SubMenu("Ks").AddItem(new MenuItem("ActiveKs", "Use KillSteal")).SetValue(true);
            _config.SubMenu("Ks").AddItem(new MenuItem("UseQKs", "Use Q")).SetValue(true);
            _config.SubMenu("Ks").AddItem(new MenuItem("UseEKs", "Use E")).SetValue(true);
            _config.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);

            //Damage after combo:
            MenuItem dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

            //HitChance
            _config.AddSubMenu(new Menu("HitChance", "HitChance"));
            _config.SubMenu("HitChance").AddSubMenu(new Menu("Combo", "Combo"));
            _config.SubMenu("HitChance")
                .SubMenu("Combo")
                .AddItem(
                    new MenuItem("BQchange", "burrowed Q HitChance").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance")
                .SubMenu("Combo")
                .AddItem(
                    new MenuItem("Echange", "burrowed E HitChance").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance").AddSubMenu(new Menu("Harass", "Harass"));
            _config.SubMenu("HitChance")
                .SubMenu("Harass")
                .AddItem(
                    new MenuItem("BQchangeharass", "burrowed Q HitChance").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"})));
            _config.SubMenu("HitChance").AddSubMenu(new Menu("KillSteal", "KillSteal"));
            _config.SubMenu("HitChance")
                .SubMenu("KillSteal")
                .AddItem(
                    new MenuItem("Qchangekill", "burrowed Q HitChance").SetValue(
                        new StringList(new[] {"Low", "Medium", "High", "Very High"})));

            //Drawings
            _config.AddSubMenu(new Menu("Drawings", "Drawings"));
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            _config.SubMenu("Drawings").AddItem(new MenuItem("Drawsmite", "Draw smite")).SetValue(true);
            _config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            _config.SubMenu("Drawings")
                .AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            _config.AddToMainMenu();

            //new AssassinManager();
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat("<font color='#87f21d'>RekSai By Diabaths</font> Loaded!");
            Game.PrintChat(
                "<font color='#f2f21d'>If You like my work and want to support me,  plz donate via paypal in </font> <font color='#00e6ff'>ssssssssssmith@hotmail.com</font> (10) S");

            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            //AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            //Orbwalking.AfterAttack += Orbwalking_AfterAttack;

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            CheckSpells();
            _player = ObjectManager.Player;

            _orbwalker.SetAttack(true);
            if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
            {
                Smiteuse();
            }
            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if ((_config.Item("ActiveHarass").GetValue<KeyBind>().Active ||
                 _config.Item("harasstoggle").GetValue<KeyBind>().Active) &&
                !_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Harass();
            }
            if (_config.Item("ActiveLane").GetValue<KeyBind>().Active)
            {
                Farm();
            }
            if (_config.Item("ActiveJungle").GetValue<KeyBind>().Active)
            {
                JungleClear();
            }
            Usepotion();
            if (_config.Item("ActiveKs").GetValue<bool>())
            {
                KillSteal();
            }
            Usecleanse();
            if (_config.Item("AutoW").GetValue<bool>() && !_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                AutoW();
            }
            if (_config.Item("escapeterino").GetValue<KeyBind>().Active)
            {
                Escapeterino();
            }
        }

        public static Vector2? GetFirstWallPoint(Vector3 from, Vector3 to, float step = 25)
        {
            return GetFirstWallPoint(from.To2D(), to.To2D(), step);
        }

        public static Vector2? GetFirstWallPoint(Vector2 from, Vector2 to, float step = 25)
        {
            var direction = (to - from).Normalized();

            for (float d = 0; d < from.Distance(to); d = d + step)
            {
                var testPoint = from + d*direction;
                var flags = NavMesh.GetCollisionFlags(testPoint.X, testPoint.Y);
                if (flags.HasFlag(CollisionFlags.Wall) || flags.HasFlag(CollisionFlags.Building))
                {
                    return from + (d - step)*direction;
                }
            }

            return null;
        }

        private static void Escapeterino()
        {
            // Walljumper credits to Hellsing

            if (!_burrowed && _w.IsReady() && _be.IsReady())
                _w.Cast();

            // We need to define a new move position since jumping over walls
            // requires you to be close to the specified wall. Therefore we set the move
            // point to be that specific piont. People will need to get used to it,
            // but this is how it works.
            var wallCheck = GetFirstWallPoint(_player.Position, Game.CursorPos);

            // Be more precise
            if (wallCheck != null)
                wallCheck = GetFirstWallPoint((Vector3) wallCheck, Game.CursorPos, 5);

            // Define more position point
            var movePosition = wallCheck != null ? (Vector3) wallCheck : Game.CursorPos;

            // Update fleeTargetPosition
            var tempGrid = NavMesh.WorldToGrid(movePosition.X, movePosition.Y);
            var fleeTargetPosition = NavMesh.GridToWorld((short) tempGrid.X, (short) tempGrid.Y);

            // Also check if we want to AA aswell
            Obj_AI_Base target = null;

            // Reset walljump indicators
            var wallJumpPossible = false;

            // Only calculate stuff when our Q is up and there is a wall inbetween
            if (_burrowed && _be.IsReady() && wallCheck != null)
            {
                // Get our wall position to calculate from
                var wallPosition = movePosition;

                // Check 300 units to the cursor position in a 160 degree cone for a valid non-wall spot
                Vector2 direction = (Game.CursorPos.To2D() - wallPosition.To2D()).Normalized();
                float maxAngle = 80;
                float step = maxAngle/20;
                float currentAngle = 0;
                float currentStep = 0;
                bool jumpTriggered = false;
                while (true)
                {
                    // Validate the counter, break if no valid spot was found in previous loops
                    if (currentStep > maxAngle && currentAngle < 0)
                        break;

                    // Check next angle
                    if ((currentAngle == 0 || currentAngle < 0) && currentStep != 0)
                    {
                        currentAngle = (currentStep)*(float) Math.PI/180;
                        currentStep += step;
                    }

                    else if (currentAngle > 0)
                        currentAngle = -currentAngle;

                    Vector3 checkPoint;

                    // One time only check for direct line of sight without rotating
                    if (currentStep == 0)
                    {
                        currentStep = step;
                        checkPoint = wallPosition + _be.Range*direction.To3D();
                    }
                    // Rotated check
                    else
                        checkPoint = wallPosition + _be.Range*direction.Rotated(currentAngle).To3D();

                    // Check if the point is not a wall
                    if (!checkPoint.IsWall())
                    {
                        // Check if there is a wall between the checkPoint and wallPosition
                        wallCheck = GetFirstWallPoint(checkPoint, wallPosition);
                        if (wallCheck != null)
                        {
                            // There is a wall inbetween, get the closes point to the wall, as precise as possible
                            Vector3 wallPositionOpposite =
                                (Vector3) GetFirstWallPoint((Vector3) wallCheck, wallPosition, 5);

                            // Check if it's worth to jump considering the path length
                            if (_player.GetPath(wallPositionOpposite).ToList().To2D().PathLength() -
                                _player.Distance(wallPositionOpposite) > 200) //200
                            {
                                // Check the distance to the opposite side of the wall
                                if (_player.Distance(wallPositionOpposite, true) <
                                    Math.Pow(_be.Range - _player.BoundingRadius/2, 2))
                                {
                                    // Make the jump happen
                                    _be.Cast(wallPositionOpposite);

                                    // Update jumpTriggered value to not orbwalk now since we want to jump
                                    jumpTriggered = true;

                                    break;
                                }
                                // If we are not able to jump due to the distance, draw the spot to
                                // make the user notice the possibliy
                                else
                                {
                                    // Update indicator values
                                    wallJumpPossible = true;
                                }
                            }

                            else
                            {
                                // yolo
                                Render.Circle.DrawCircle(Game.CursorPos, 35, Color.Red, 2);
                            }
                        }
                    }
                }

                // Check if the loop triggered the jump, if not just orbwalk
                if (!jumpTriggered)
                    Orbwalking.Orbwalk(target, Game.CursorPos, 90f, 0f, false, false);
            }

            // Either no wall or W on cooldown, just move towards to wall then
            else
            {
                Orbwalking.Orbwalk(target, Game.CursorPos, 90f, 0f, false, false);
                if (_burrowed && _be.IsReady())
                    _be.Cast(Game.CursorPos);
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (_w.IsReady() && gapcloser.Sender.IsValidTarget(_w.Range) && _config.Item("Gap_W").GetValue<bool>())
            {
                _w.Cast();
            }
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (_burrowed && _w.IsReady() && unit.IsValidTarget(_q.Range) && _config.Item("Inter_W").GetValue<bool>())
                _w.Cast(unit);
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spell = args.SData;
            if (!sender.IsMe)
            {
                return;
            }
            if (spell.Name.ToLower().Contains("reksaiq"))
            {
                Utility.DelayAction.Add(450, Orbwalking.ResetAutoAttackTimer);
            }
            /*if (sender.IsMe)
            {
                 Game.PrintChat("Spell name: " + args.SData.Name.ToString());
            }*/
        }

        private static void Usecleanse()
        {
            if (_player.IsDead ||
                (_config.Item("Cleansemode").GetValue<StringList>().SelectedIndex == 1 &&
                 !_config.Item("ActiveCombo").GetValue<KeyBind>().Active)) return;
            if (Cleanse(_player) && _config.Item("useqss").GetValue<bool>())
            {
                if (_player.HasBuff("zedulttargetmark"))
                {
                    if (Items.HasItem(3140) && Items.CanUseItem(3140))
                        Utility.DelayAction.Add(500, () => Items.UseItem(3140));
                    else if (Items.HasItem(3139) && Items.CanUseItem(3139))
                        Utility.DelayAction.Add(500, () => Items.UseItem(3139));
                    else if (Items.HasItem(3137) && Items.CanUseItem(3137))
                        Utility.DelayAction.Add(500, () => Items.UseItem(3137));
                }
                else
                {
                    if (Items.HasItem(3140) && Items.CanUseItem(3140))
                        Utility.DelayAction.Add(100, () => Items.UseItem(3140));
                    else if (Items.HasItem(3139) && Items.CanUseItem(3139))
                        Utility.DelayAction.Add(100, () => Items.UseItem(3139));
                    else if (Items.HasItem(3137) && Items.CanUseItem(3137))
                        Utility.DelayAction.Add(100, () => Items.UseItem(3137));
                }
            }
        }

        private static bool Cleanse(Obj_AI_Hero hero)
        {
            bool cc = false;
            if (_config.Item("blind").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Blind))
                {
                    cc = true;
                }
            }
            if (_config.Item("charm").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Charm))
                {
                    cc = true;
                }
            }
            if (_config.Item("fear").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Fear))
                {
                    cc = true;
                }
            }
            if (_config.Item("flee").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Flee))
                {
                    cc = true;
                }
            }
            if (_config.Item("snare").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Snare))
                {
                    cc = true;
                }
            }
            if (_config.Item("taunt").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Taunt))
                {
                    cc = true;
                }
            }
            if (_config.Item("suppression").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Suppression))
                {
                    cc = true;
                }
            }
            if (_config.Item("stun").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Stun))
                {
                    cc = true;
                }
            }
            if (_config.Item("polymorph").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Polymorph))
                {
                    cc = true;
                }
            }
            if (_config.Item("silence").GetValue<bool>())
            {
                if (hero.HasBuffOfType(BuffType.Silence))
                {
                    cc = true;
                }
            }
            if (_config.Item("zedultexecute").GetValue<bool>())
            {
                if (_player.HasBuff("zedulttargetmark"))
                {
                    cc = true;
                }
            }
            return cc;
        }

        private static void Smiteontarget()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var smiteDmg = _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Smite);
                var usesmite = _config.Item("smitecombo").GetValue<bool>();
                if (SmiteBlue.Any(i => Items.HasItem(i)) && usesmite &&
                    ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready &&
                    hero.IsValidTarget(_smite.Range))
                {
                    if (!hero.HasBuffOfType(BuffType.Stun) || !hero.HasBuffOfType(BuffType.Slow))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                    }
                    else if (smiteDmg >= hero.Health)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                    }
                }
                if (SmiteRed.Any(i => Items.HasItem(i)) && usesmite &&
                    ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready &&
                    hero.IsValidTarget(_smite.Range))
                {
                    ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                }
            }
        }

        private static HitChance BQchange()
        {
            switch (_config.Item("BQchange").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static HitChance Echange()
        {
            switch (_config.Item("Echange").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static HitChance BQchangeharass()
        {
            switch (_config.Item("BQchangeharass").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static HitChance Qchangekill()
        {
            switch (_config.Item("Qchangekill").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static void Combo()
        {
            var t = TargetSelector.GetTarget(_bq.Range, TargetSelector.DamageType.Physical);

            var ignitecombo = _config.Item("UseIgnitecombo").GetValue<bool>();
            var reksaifury = Equals(_player.Mana, _player.MaxMana);

            Smiteontarget();
            if (_igniteSlot != SpellSlot.Unknown && ignitecombo && t.IsValidTarget(600) &&
                _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                if (t.Health <= ComboDamage(t))
                {
                    _player.Spellbook.CastSpell(_igniteSlot, t);
                }
            }
            if (_burrowed)
            {
                if (_config.Item("UseQCombo").GetValue<bool>() && _bq.IsReady() && t.IsValidTarget(_bq.Range))
                {
                    _bq.CastIfHitchanceEquals(t, BQchange());
                }
                if (_config.Item("UseECombo").GetValue<bool>() && _be.IsReady() && t.IsValidTarget(_be.Range))
                {
                    if (Prediction.GetPrediction(t, 10).UnitPosition
                        .Distance(_player.Position) > t.Distance(_player.Position))
                    {
                        var x = Prediction.GetPrediction(t, 500).UnitPosition;
                        var y = _player.Position.Extend(x, _be.Range);
                        _be.Cast(x);
                    }
                }
                if (_config.Item("UseWCombo").GetValue<bool>() && _w.IsReady() && t.IsValidTarget(_w.Range) &&
                        !_bq.IsReady())
                {
                    _w.Cast();
                }

            }
            if (_unburrowed)
            {
                if (_config.Item("UseQCombo").GetValue<bool>() && _q.IsReady() && t.IsValidTarget(_q.Range))
                {
                    _q.Cast(t);
                }
                if (t.IsValidTarget(_e.Range) && _config.Item("UseECombo").GetValue<bool>() && _e.IsReady())
                {
                    if (reksaifury)
                    {
                        _e.Cast(t);
                    }
                    else if (_player.Mana < 100 && t.Health <= EDamage(t))
                    {
                        _e.Cast(t);
                    }
                    else if (_player.Mana == 100 && t.Health <= EDamagetrue(t))
                    {
                        _e.Cast(t);
                    }
                    else if (t.Health <= ComboDamage(t))
                    {
                        _e.Cast(t);
                    }
                }
                if (_config.Item("UseWCombo").GetValue<bool>() && _w.IsReady())
                {
                    if (!_q.IsReady() && _be.IsReady()
                       && !(t as Obj_AI_Base).HasBuff("reksaiknockupimmune") && t.IsValidTarget(_be.Range))
                    _w.Cast();
                    /*if (_player.Distance(t) >= 500)
                    {
                        _w.Cast();
                    }*/
                }
            }
            UseItemes();
        }

        private static void UseItemes()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var iBilge = _config.Item("Bilge").GetValue<bool>();
                var iBilgeEnemyhp = hero.Health <=
                                    (hero.MaxHealth*(_config.Item("BilgeEnemyhp").GetValue<Slider>().Value)/100);
                var iBilgemyhp = _player.Health <=
                                 (_player.MaxHealth*(_config.Item("Bilgemyhp").GetValue<Slider>().Value)/100);
                var iBlade = _config.Item("Blade").GetValue<bool>();
                var iBladeEnemyhp = hero.Health <=
                                    (hero.MaxHealth*(_config.Item("BladeEnemyhp").GetValue<Slider>().Value)/100);
                var iBlademyhp = _player.Health <=
                                 (_player.MaxHealth*(_config.Item("Blademyhp").GetValue<Slider>().Value)/100);
                var iOmen = _config.Item("Omen").GetValue<bool>();
                var iOmenenemys = hero.CountEnemiesInRange(450) >= _config.Item("Omenenemys").GetValue<Slider>().Value;
                var iTiamat = _config.Item("Tiamat").GetValue<bool>();
                var iHydra = _config.Item("Hydra").GetValue<bool>();

                if (hero.IsValidTarget(450) && iBilge && (iBilgeEnemyhp || iBilgemyhp) && _bilge.IsReady())
                {
                    _bilge.Cast(hero);

                }
                if (hero.IsValidTarget(450) && iBlade && (iBladeEnemyhp || iBlademyhp) && _blade.IsReady())
                {
                    _blade.Cast(hero);
                }
                if (iTiamat && _tiamat.IsReady() && hero.IsValidTarget(_tiamat.Range))
                {
                    _tiamat.Cast();

                }
                if (iHydra && _hydra.IsReady() && hero.IsValidTarget(_hydra.Range))
                {
                    _hydra.Cast();
                }
                if (iOmenenemys && iOmen && _rand.IsReady() && hero.IsValidTarget(450))
                {
                    _rand.Cast();
                }
            }
            var ilotis = _config.Item("lotis").GetValue<bool>();
            if (ilotis)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly || hero.IsMe))
                {
                    if (hero.Health <= (hero.MaxHealth*(_config.Item("lotisminhp").GetValue<Slider>().Value)/100) &&
                        hero.Distance(_player.ServerPosition) <= _lotis.Range && _lotis.IsReady())
                        _lotis.Cast();
                }
            }
        }

        private static void Usepotion()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var iusehppotion = _config.Item("usehppotions").GetValue<bool>();
            var iusepotionhp = _player.Health <=
                               (_player.MaxHealth*(_config.Item("usepotionhp").GetValue<Slider>().Value)/100);

            if (_player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (Utility.CountEnemiesInRange(800) > 0 ||
                (mobs.Count > 0 && _config.Item("ActiveJungle").GetValue<KeyBind>().Active && (Items.HasItem(1039) ||
                                                                                               SmiteBlue.Any(
                                                                                                   i => Items.HasItem(i)) ||
                                                                                               SmiteRed.Any(
                                                                                                   i => Items.HasItem(i)) ||
                                                                                               SmitePurple.Any(
                                                                                                   i => Items.HasItem(i)) ||
                                                                                               SmiteBlue.Any(
                                                                                                   i => Items.HasItem(i)) ||
                                                                                               SmiteGrey.Any(
                                                                                                   i => Items.HasItem(i))
                    )))
            {
                if (iusepotionhp && iusehppotion &&
                    !(ObjectManager.Player.HasBuff("RegenerationPotion", true) ||
                      ObjectManager.Player.HasBuff("ItemCrystalFlask", true) ||
                      ObjectManager.Player.HasBuff("ItemMiniRegenPotion", true)))
                {
                    if (Items.HasItem(2041) && Items.CanUseItem(2041))
                    {
                        Items.UseItem(2041);
                    }
                    else if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }
                    else if (Items.HasItem(2003) && Items.CanUseItem(2003))
                    {
                        Items.UseItem(2003);
                    }
                }
            }
        }

        private static float ComboDamage(Obj_AI_Hero hero)
        {
            var dmg = 0d;

            if (_q.IsReady() && _unburrowed)

                dmg += QDamage(hero);

            if (_burrowed)
                dmg += BqDamage(hero);
            if (_w.IsReady())
                dmg +=WDamage(hero);
            if (_e.IsReady())
                if (_player.Mana < 100)
                {
                    dmg += EDamage(hero);
                }
            if (_player.Mana == 100)
            {
                dmg += EDamagetrue(hero);
            }
            if (ObjectManager.Player.GetSpellSlot("SummonerIgnite") != SpellSlot.Unknown)
            {
                dmg += _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            if (Items.HasItem(3077) && Items.CanUseItem(3077))
                dmg += _player.GetItemDamage(hero, Damage.DamageItems.Tiamat);
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
                dmg += _player.GetItemDamage(hero, Damage.DamageItems.Hydra);
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                dmg += _player.GetItemDamage(hero, Damage.DamageItems.Botrk);
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                dmg += _player.GetItemDamage(hero, Damage.DamageItems.Bilgewater);
            dmg += _player.GetAutoAttackDamage(hero, true)*2;
            return (float) dmg;
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_bq.Range, TargetSelector.DamageType.Magical);
            var targetq = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
            var targete = TargetSelector.GetTarget(_e.Range, TargetSelector.DamageType.True);
            var reksaifury = Equals(_player.Mana, _player.MaxMana);
            var useItemsH = _config.Item("UseItemsharass").GetValue<bool>();
            if (_config.Item("UseQHarass").GetValue<bool>())
            {
                if (target.IsValidTarget(_bq.Range) && _bq.IsReady() && _burrowed)
                {
                    _bq.CastIfHitchanceEquals(target, BQchangeharass());
                }
                if (targetq.IsValidTarget(_q.Range) && _q.IsReady() && _unburrowed)
                {
                    _q.Cast();
                }
            }
            if (targete.IsValidTarget(_e.Range) && _config.Item("UseEHarass").GetValue<bool>() && _e.IsReady() &&
                _unburrowed && reksaifury)
            {
                _e.Cast(targete);
            }
            if (useItemsH && _tiamat.IsReady() && target.IsValidTarget(_tiamat.Range))
            {
                _tiamat.Cast();
            }
            if (useItemsH && _hydra.IsReady() && target.IsValidTarget(_hydra.Range))
            {
                _hydra.Cast();
            }
        }

        private static double EDamage(Obj_AI_Base unit)
        {

            return _e.IsReady()
                ? _player.CalcDamage(unit, Damage.DamageType.Physical,
                    new double[] {0.8, 0.9, 1, 1.1, 1.2}[_e.Level - 1]*_player.TotalAttackDamage
                    *(1 + (_player.Mana/_player.MaxMana)))
                : 0d;
        }

        private static double EDamagetrue(Obj_AI_Base unit)
        {

            return _e.IsReady()
                ? _player.CalcDamage(unit, Damage.DamageType.True,
                    new double[] {1.6, 1.8, 2, 2.2, 2.4}[_e.Level - 1]*_player.TotalAttackDamage)
                : 0d;
        }

        private static double QDamage(Obj_AI_Base unit)
        {
            return _q.IsReady()
                ? _player.CalcDamage(unit, Damage.DamageType.Physical,
                    new double[] {45, 75, 105, 135, 165}[_q.Level - 1] +
                    _player.TotalAttackDamage*0.6)
                : 0d;
        }

        private static double BqDamage(Obj_AI_Base unit)
        {
            return _bq.IsReady()
                ? _player.CalcDamage(unit, Damage.DamageType.Magical,
                    new double[] {60, 90, 120, 150, 180}[_bq.Level - 1] + 0.7*_player.FlatMagicDamageMod)
                : 0d;
        }

        private static double WDamage(Obj_AI_Base unit)
        {
            return _bq.IsReady()
                ? _player.CalcDamage(unit, Damage.DamageType.Physical,
                    new double[] {40, 80, 120, 160, 200}[_bq.Level - 1] + 0.4*_player.TotalAttackDamage)
                : 0d;

        }

        private static void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _bq.Range, MinionTypes.All);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _q.Range, MinionTypes.All);
            var useQ = _config.Item("UseQLane").GetValue<bool>();
            var useW = _config.Item("UseWLane").GetValue<bool>();
            var useE = _config.Item("UseELane").GetValue<bool>();
            var useItemsl = _config.Item("UseItemslane").GetValue<bool>();
            if (_q.IsReady() && useQ && _unburrowed)
            {
                if (allMinions.Count >= 3)
                {
                    _q.Cast();
                }
                else
                    foreach (var minion in allMinions)
                        if (minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.Q))
                            _q.Cast();
            }
            if (_bq.IsReady() && useQ && _burrowed)
            {
                var fl2 = _q.GetCircularFarmLocation(allMinions, 400);

                if (fl2.MinionsHit >= 3 && _bq.IsReady())
                {
                    _bq.Cast(fl2.Position);
                }
                else
                    foreach (var minion in allMinions)
                        if (minion.Health < 0.75*_player.GetSpellDamage(minion, SpellSlot.Q))
                            _bq.Cast(minion);
            }
            if (_e.IsReady() && useE && _unburrowed)
            {
                foreach (var minione in allMinions)

                    if (minione.Health < EDamage(minione))
                        _e.Cast(minione);
            }

            if (useW && _unburrowed && !_q.IsReady() && !_e.IsReady())
            {
                _w.Cast();
            }
            foreach (var minion in allMinionsQ)
            {
                if (useItemsl && _tiamat.IsReady() && minion.IsValidTarget(_tiamat.Range))
                {
                    _tiamat.Cast();
                }
                if (useItemsl && _hydra.IsReady() && minion.IsValidTarget(_tiamat.Range))
                {
                    _hydra.Cast();
                }
            }
        }

        //Credits to Kurisu
        private static string Smitetype()
        {
            if (SmiteBlue.Any(i => Items.HasItem(i)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(i => Items.HasItem(i)))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(i => Items.HasItem(i)))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(i => Items.HasItem(i)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }


        //Credits to metaphorce
        private static void SetSmiteSlot()
        {
            foreach (
                var spell in
                    ObjectManager.Player.Spellbook.Spells.Where(
                        spell => String.Equals(spell.Name, Smitetype(), StringComparison.CurrentCultureIgnoreCase)))
            {
                _smiteSlot = spell.Slot;
                _smite = new Spell(_smiteSlot, 700);
                return;
            }
        }

        private static int GetSmiteDmg()
        {
            int level = _player.Level;
            int index = _player.Level/5;
            float[] dmgs = {370 + 20*level, 330 + 30*level, 240 + 40*level, 100 + 50*level};
            return (int) dmgs[index];
        }

        //New map Monsters Name By SKO
        private static void Smiteuse()
        {
            var jungle = _config.Item("ActiveJungle").GetValue<KeyBind>().Active;
            if (ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) != SpellState.Ready) return;
            var usered = _config.Item("Usered").GetValue<bool>();
            var health = (100*(_player.Health/_player.MaxHealth)) < _config.Item("healthJ").GetValue<Slider>().Value;
            string[] jungleMinions;
            if (Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline)
            {
                jungleMinions = new string[] {"TT_Spiderboss", "TT_NWraith", "TT_NGolem", "TT_NWolf"};
            }
            else
            {
                jungleMinions = new string[]
                {
                    "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red", "SRU_Krug", "SRU_Dragon",
                    "SRU_Baron"
                };
            }
            var minions = MinionManager.GetMinions(_player.Position, 1000, MinionTypes.All, MinionTeam.Neutral);
            if (minions.Count() > 0)
            {
                int smiteDmg = GetSmiteDmg();

                foreach (Obj_AI_Base minion in minions)
                {
                    if (Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline &&
                        minion.Health <= smiteDmg &&
                        jungleMinions.Any(name => minion.Name.Substring(0, minion.Name.Length - 5).Equals(name)))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    if (minion.Health <= smiteDmg && jungleMinions.Any(name => minion.Name.StartsWith(name)) &&
                        !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    else if (jungle && usered && health && minion.Health >= smiteDmg &&
                             jungleMinions.Any(name => minion.Name.StartsWith("SRU_Red")) &&
                             !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                }
            }
        }

        private static void JungleClear()
        {
            string[] jungleMinions;
            if (Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline)
            {
                jungleMinions = new string[] {"TT_Spiderboss", "TT_NWraith", "TT_NGolem", "TT_NWolf"};
            }
            else
            {
                jungleMinions = new string[]
                {
                    "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red", "SRU_Krug", "SRU_Dragon",
                    "SRU_Baron"
                };
            }
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _bq.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var useItemsJ = _config.Item("UseItemsjungle").GetValue<bool>();
            var useQ = _config.Item("UseQJungle").GetValue<bool>();
            var useW = _config.Item("UseWJungle").GetValue<bool>();
            var useE = _config.Item("UseEJungle").GetValue<bool>();
            var reksaifury = Equals(_player.Mana, _player.MaxMana);

            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (_unburrowed)
                {
                    if (useQ && _q.IsReady() && Orbwalking.InAutoAttackRange(mob))
                    {
                        _q.Cast();
                    }
                    if (_e.IsReady() && useE && _player.Distance(mob) < _e.Range &&
                        !jungleMinions.Any(name => mob.Name.Contains("Mini")))
                    {
                        if (reksaifury)
                        {
                            _e.Cast(mob);
                        }
                        else if (mob.Health <= EDamage(mob))
                        {
                            _e.Cast(mob);
                        }
                    }
                    if (useW && !mob.HasBuff("reksaiknockupimmune", true) && _w.IsReady() && !_q.IsReady() &&
                        !_e.IsReady() &&
                        mob.IsValidTarget(_w.Range) && _unburrowed)
                    {
                        _w.Cast();
                    }
                }
                if (_burrowed && _bq.IsReady() && useQ && _player.Distance(mob) < _bq.Range)
                {
                    _bq.Cast(mob);
                }
                if (useItemsJ && _tiamat.IsReady() && mob.IsValidTarget(_tiamat.Range))
                {
                    _tiamat.Cast();
                }
                if (useItemsJ && _hydra.IsReady() && mob.IsValidTarget(_tiamat.Range))
                {
                    _hydra.Cast();
                }
            }
        }


        private static void KillSteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var igniteDmg = _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
                
                var qDmg = _player.GetSpellDamage(hero, SpellSlot.Q);
                var eDmg = _player.GetSpellDamage(hero, SpellSlot.E);
                if (hero.IsValidTarget(600) && _config.Item("UseIgnite").GetValue<bool>() &&
                    _igniteSlot != SpellSlot.Unknown &&
                    _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                {
                    if (igniteDmg > hero.Health)
                    {
                        _player.Spellbook.CastSpell(_igniteSlot, hero);
                    }
                }
                if (_config.Item("UseQKs").GetValue<bool>())
                {
                    if (_bq.IsReady() && hero.IsValidTarget(_bq.Range) && _burrowed)
                    {
                        if (hero.Health <= BqDamage(hero))
                        _bq.CastIfHitchanceEquals(hero, Qchangekill());
                    }
                    if (_q.IsReady() && hero.IsValidTarget(_q.Range) && hero.Health <= qDmg && _unburrowed)
                    {
                        if (hero.Health <= QDamage(hero))
                        _q.Cast();
                    }
                }
                if (_e.IsReady() && hero.IsValidTarget(_e.Range) && _config.Item("UseEKs").GetValue<bool>() &&
                    _unburrowed)
                {
                    if (_player.Mana <=100 &&hero.Health <= EDamage(hero))
                    {
                        _e.Cast(hero);
                    }
                    if (_player.Mana == 100 && hero.Health <= EDamagetrue(hero))
                    {
                        _e.Cast(hero);
                    }
                }
            }
        }

        private static void AutoW()
        {
            var reksaiHp = (_player.MaxHealth*(_config.Item("AutoWHP").GetValue<Slider>().Value)/100);
            var reksaiMp = (_player.MaxMana*(_config.Item("AutoWMP").GetValue<Slider>().Value)/100);
            if (_player.HasBuff("Recall") || _player.InFountain()) return;
            if (_w.IsReady() && _player.Health <= reksaiHp && _unburrowed && _player.Mana >= reksaiMp)
            {
                _w.Cast();
            }

        }

        /* private static bool Packets()
         {
             return _config.Item("usePackets").GetValue<bool>();
         }*/





        private static void CheckSpells()
        {
            if (_player.Spellbook.GetSpell(SpellSlot.W).Name == "reksaiwburrowed" ||
                _player.Spellbook.GetSpell(SpellSlot.Q).Name == "reksaiqburrowed" ||
                _player.Spellbook.GetSpell(SpellSlot.E).Name == "reksaieburrowed")
            {
                _burrowed = true;
                _unburrowed = false;

            }
            else if (_player.Spellbook.GetSpell(SpellSlot.W).Name == "RekSaiW" ||
                     _player.Spellbook.GetSpell(SpellSlot.Q).Name == "RekSaiQ" ||
                     _player.Spellbook.GetSpell(SpellSlot.E).Name == "reksaie")
            {
                _burrowed = false;
                _unburrowed = true;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {

            if (_config.Item("Drawsmite").GetValue<bool>())
            {
                if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
                {
                    Drawing.DrawText(Drawing.Width*0.02f, Drawing.Height*0.90f, System.Drawing.Color.DarkOrange,
                        "Smite Jungle On");
                }
                else
                    Drawing.DrawText(Drawing.Width*0.02f, Drawing.Height*0.90f, System.Drawing.Color.DarkRed,
                        "Smite Jungle Off");
                if (_config.Item("smitecombo").GetValue<bool>())
                {
                    Drawing.DrawText(Drawing.Width*0.02f, Drawing.Height*0.88f, System.Drawing.Color.DarkOrange,
                        "Smite Target On");
                }
                else
                    Drawing.DrawText(Drawing.Width*0.02f, Drawing.Height*0.88f, System.Drawing.Color.DarkRed,
                        "Smite Target Off");
            }

            if (_config.Item("CircleLag").GetValue<bool>())
            {
                if (_config.Item("DrawQ").GetValue<bool>())
                {
                    if (_burrowed)
                        Utility.DrawCircle(ObjectManager.Player.Position, _bq.Range, System.Drawing.Color.DarkOrange,
                            _config.Item("CircleThickness").GetValue<Slider>().Value,
                            _config.Item("CircleQuality").GetValue<Slider>().Value);
                }

                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, _burrowed ? _be.Range : _e.Range,
                        System.Drawing.Color.DarkOrange,
                        _config.Item("CircleThickness").GetValue<Slider>().Value,
                        _config.Item("CircleQuality").GetValue<Slider>().Value);
                }

            }
            else
            {
                if (_config.Item("DrawQ").GetValue<bool>() && _burrowed)
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _bq.Range, System.Drawing.Color.White);
                }
                if (_config.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, _burrowed ? _be.Range : _e.Range,
                        System.Drawing.Color.White);
                }

            }
        }
    }
}