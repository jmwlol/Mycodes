﻿using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

namespace D_Udyr
{
    internal class Program
    {

        public const string ChampionName = "Udyr";

        private static Orbwalking.Orbwalker _orbwalker;

        private static readonly List<Spell> SpellList = new List<Spell>();

        public static bool Tiger;

        public static bool Turtle;

        public static bool Bear;

        public static bool Phoenix;

        private static Spell _q;

        private static Spell _w;

        private static Spell _e;

        private static Spell _r;

        private static Menu _config;

        private static Obj_AI_Hero _player;

        private static SpellSlot _smiteSlot;

        private static Spell _smite;
       
        private static Items.Item _tiamat, _hydra, _blade, _bilge, _rand, _lotis;

        //Tiger Style                              1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18
        private static readonly int[] TigerQewr = { 1, 2, 3, 1, 1, 3, 1, 3, 1, 3, 3, 2, 2, 2, 2, 4, 4, 4 };
        private static readonly int[] TigerQwer = { 1, 2, 3, 1, 1, 2, 1, 2, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4 };

        //Phoenix Style                              1  2  3  4  5  6  7  8  9  10 11 12 13 14 15 16 17 18
        private static readonly int[] PhoenixRewq = { 4, 1, 4, 2, 3, 4, 4, 4, 3, 3, 3, 3, 2, 2, 2, 2, 1, 1 };
        private static readonly int[] PhoenixRweq = { 4, 1, 4, 2, 3, 4, 4, 4, 2, 2, 2, 2, 3, 3, 3, 3, 1, 1 };

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (_player.ChampionName != "Udyr")
                return;

            _q = new Spell(SpellSlot.Q, 200);
            _w = new Spell(SpellSlot.W, 200);
            _e = new Spell(SpellSlot.E, 200);
            _r = new Spell(SpellSlot.R, 200);

            SpellList.Add(_q);
            SpellList.Add(_w);
            SpellList.Add(_e);
            SpellList.Add(_r);

            _bilge = new Items.Item(3144, 450f);
            _blade = new Items.Item(3153, 450f);
            _hydra = new Items.Item(3074, 250f);
            _tiamat = new Items.Item(3077, 250f);
            _rand = new Items.Item(3143, 490f);
            _lotis = new Items.Item(3190, 590f);

            if (_player.GetSpell(SpellSlot.Summoner1).Name.ToLower().Contains("smite"))
            {
                _smite = new Spell(SpellSlot.Summoner1, 570f);
                _smiteSlot = SpellSlot.Summoner1;
            }
            else if (_player.GetSpell(SpellSlot.Summoner2).Name.ToLower().Contains("smite"))
            {
                _smite = new Spell(SpellSlot.Summoner2, 570f);
                _smiteSlot = SpellSlot.Summoner2;
            }

            //Udyr
            _config = new Menu("D-Udyr", "D-Udyr", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            _config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Auto Level
            _config.AddSubMenu(new Menu("Style", "Style"));
            _config.SubMenu("Style").AddItem(new MenuItem("udAutoLevel", "Auto Level")).SetValue(false);
            _config.SubMenu("Style").AddItem(new MenuItem("udyrStyle", "Level Sequence").SetValue(
                new StringList(new[] { "Tiger Q-E-W-R", "Tiger Q-W-E-R", "Pheonix R-E-W-Q", "Pheonix R-W-E-Q" })));

            //Combo
            _config.AddSubMenu(new Menu("Main", "Main"));
            _config.SubMenu("Main").AddItem(new MenuItem("delaycombo", "Delay between Skills").SetValue(new Slider(200, 0, 1500)));
            _config.SubMenu("Main").AddItem(new MenuItem("AutoShield", "Auto Shield")).SetValue(true);
            _config.SubMenu("Main")
                .AddItem(new MenuItem("AutoShield%", "AutoShield HP %").SetValue(new Slider(50, 100, 0)));
            _config.SubMenu("Main")
                .AddItem(new MenuItem("TargetRange", "Range to Use E").SetValue(new Slider(1000, 600, 1500)));
            _config.SubMenu("Main")
                .AddItem(new MenuItem("ActiveCombo", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            _config.SubMenu("Main").AddItem(new MenuItem("smitecombo", "Use Smite in target")).SetValue(true);
            _config.SubMenu("Main")
                .AddItem(
                    new MenuItem("StunCycle", "Stun Cycle").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            //Forest gump
            _config.AddSubMenu(new Menu("Forest Gump", "Forest Gump"));
            _config.SubMenu("Forest Gump").AddItem(new MenuItem("ForestE", "Use E")).SetValue(true);
            _config.SubMenu("Forest Gump").AddItem(new MenuItem("ForestW", "Use W")).SetValue(true);
            _config.SubMenu("Forest Gump")
              .AddItem(new MenuItem("Forest", "Forest gump"))
              .SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Press));
           _config.SubMenu("Forest Gump")
                .AddItem(new MenuItem("Forest-Mana", "Forest gump Mana").SetValue(new Slider(50, 100, 0)));

            _config.AddSubMenu(new Menu("items", "items"));
            _config.SubMenu("items").AddSubMenu(new Menu("Offensive", "Offensive"));
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Tiamat", "Use Tiamat")).SetValue(true);
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Hydra", "Use Hydra")).SetValue(true);
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Bilge", "Use Bilge")).SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BilgeEnemyhp", "If Enemy Hp < ").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Bilgemyhp", "Or your Hp < ").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Blade", "Use Blade")).SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BladeEnemyhp", "If Enemy Hp < ").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Blademyhp", "Or Your  Hp < ").SetValue(new Slider(85, 1, 100)));
            _config.SubMenu("items").AddSubMenu(new Menu("Deffensive", "Deffensive"));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omen", "Use Randuin Omen"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omenenemys", "Randuin if enemys>").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("lotis", "Use Iron Solari")).SetValue(true);
            _config.SubMenu("items").SubMenu("Deffensive").AddItem(new MenuItem("lotisminhp", "Solari if Ally Hp<").SetValue(new Slider(35, 1, 100)));

            _config.SubMenu("items").SubMenu("Deffensive").AddItem(new MenuItem("Righteous", "Use Righteous Glory")).SetValue(true);
            _config.SubMenu("items").SubMenu("Deffensive").AddItem(new MenuItem("Righteousenemys", "Righteous Glory if  Enemy >=").SetValue(new Slider(2, 1, 5)));
            _config.SubMenu("items").SubMenu("Deffensive").AddItem(new MenuItem("Righteousenemysrange", "Righteous Glory Range Check").SetValue(new Slider(800, 400, 1400)));

            //potions
            _config.SubMenu("items").AddSubMenu(new Menu("Potions", "Potions"));
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usehppotions", "Use Healt potion/Refillable/Hunters/Corrupting/Biscuit"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionhp", "If Health % <").SetValue(new Slider(35, 1, 100)));
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usemppotions", "Use Hunters/Corrupting/Biscuit"))
                .SetValue(true);
            _config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionmp", "If Mana % <").SetValue(new Slider(35, 1, 100)));


            //Farm
            _config.AddSubMenu(new Menu("Farm", "Farm"));
            _config.SubMenu("Farm").AddItem(new MenuItem("delayfarm", "Delay between Skills").SetValue(new Slider(2000, 1000, 3000)));

            _config.SubMenu("Farm").AddSubMenu(new Menu("Lane", "Lane"));
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("laneitems", "Use Items")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("Use-Q-Farm", "Use Q")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("Use-W-Farm", "Use W")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("Use-E-Farm", "Use E")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Lane").AddItem(new MenuItem("Use-R-Farm", "Use R")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("Lane")
                .AddItem(new MenuItem("Farm-Mana", "Mana Limit").SetValue(new Slider(50, 100, 0)));
            _config.SubMenu("Farm")
                .SubMenu("Lane")
                .AddItem(
                    new MenuItem("ActiveLane", "Lane Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            _config.SubMenu("Farm").AddSubMenu(new Menu("Jungle", "Jungle"));
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("jungleitems", "Use Items")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("Use-Q-Jungle", "Use Q")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("Use-W-Jungle", "Use W")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("Use-E-Jungle", "Use E")).SetValue(true);
            _config.SubMenu("Farm").SubMenu("Jungle").AddItem(new MenuItem("Use-R-Jungle", "Use R")).SetValue(true);
            _config.SubMenu("Farm")
                .SubMenu("Jungle")
                .AddItem(new MenuItem("Jungle-Mana", "Mana Limit").SetValue(new Slider(50, 100, 0)));
            _config.SubMenu("Farm")
                .SubMenu("Jungle")
                .AddItem(
                    new MenuItem("ActiveJungle", "Jungle Key").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            //Smite 
            _config.AddSubMenu(new Menu("Smite", "Smite"));
            _config.SubMenu("Smite")
                .AddItem(
                    new MenuItem("Usesmite", "Use Smite(toggle)").SetValue(new KeyBind("H".ToCharArray()[0],
                        KeyBindType.Toggle)));
            _config.SubMenu("Smite").AddItem(new MenuItem("Useblue", "Smite Blue Early ")).SetValue(true);
            _config.SubMenu("Smite")
                .AddItem(new MenuItem("manaJ", "Smite Blue Early if MP% <").SetValue(new Slider(35, 1, 100)));
            _config.SubMenu("Smite").AddItem(new MenuItem("Usered", "Smite Red Early ")).SetValue(true);
            _config.SubMenu("Smite")
                .AddItem(new MenuItem("healthJ", "Smite Red Early if HP% <").SetValue(new Slider(35, 1, 100)));

            _config.AddToMainMenu();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += OnGameUpdate;
            _config.Item("udAutoLevel").ValueChanged += EnabledValueChanged;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            if (_config.Item("udAutoLevel").GetValue<bool>())
            {
                new AutoLevel(Style());
            }


            Game.PrintChat("<font color='#881df2'>Udyr By Diabaths </font>Loaded!");
            Game.PrintChat("<font color='#881df2'>StunCycle by xcxooxl");
            Game.PrintChat(
                 "<font color='#f2f21d'>If You like my work and want to support me,  plz donate via paypal in </font> <font color='#00e6ff'>ssssssssssmith@hotmail.com</font> (10) S");
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        { /* (_player.HasBuff("UdyrTigerStance")
           (_player.HasBuff("UdyrTurtleStance")
           (_player.HasBuff("UdyrBearStance")
           (_player.HasBuff("UdyrPhoenixStance") */
            Tiger = args.SData.Name == "UdyrTigerStance";

            Turtle = args.SData.Name == "UdyrTurtleStance";

            Bear = args.SData.Name == "UdyrBearStance";

            Phoenix = args.SData.Name == "UdyrPhoenixStance";

           /* var spell = args.SData;
            if (sender.IsMe)
            {
                Game.PrintChat("Spell name: " + args.SData.Name.ToString());
            }*/
        }

        private static void OnGameUpdate(EventArgs args)
        {
            _player = ObjectManager.Player;
            if (_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (_config.Item("StunCycle").GetValue<KeyBind>().Active)
            {
                StunCycle();
            }

            if (_config.Item("ActiveLane").GetValue<KeyBind>().Active &&
                (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Farm-Mana").GetValue<Slider>().Value)
            {
                Farm();
            }

            if (_config.Item("ActiveJungle").GetValue<KeyBind>().Active &&
                (100 * (_player.Mana / _player.MaxMana)) > _config.Item("Jungle-Mana").GetValue<Slider>().Value)
            {
                JungleClear();
            }

            if (_config.Item("AutoShield").GetValue<bool>() && !_config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                AutoW();
            }

            if (_config.Item("Forest").GetValue<KeyBind>().Active &&
                100 * (_player.Mana / _player.MaxMana) > _config.Item("Forest-Mana").GetValue<Slider>().Value)
            {
                Forest();
            }

            Usepotion();
            if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
            {
                Smiteuse();
            }

            _orbwalker.SetAttack(true);

            _orbwalker.SetMovement(true);
        }

        private static int[] Style()
        {
            switch (_config.Item("udyrStyle").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return TigerQewr;
                case 1:
                    return TigerQwer;
                case 2:
                    return PhoenixRewq;
                case 3:
                    return PhoenixRweq;

                default:
                    return null;
            }
        }

        /* (_player.HasBuff("UdyrTigerStance")
           (_player.HasBuff("UdyrTurtleStance")
           (_player.HasBuff("UdyrBearStance")
           (_player.HasBuff("UdyrPhoenixStance") */
       
        private static void EnabledValueChanged(object sender, OnValueChangeEventArgs e)
        {
            AutoLevel.Enabled(e.GetNewValue<bool>());
        }

        private static void Farm()
        {
            var useItemsl = _config.Item("laneitems").GetValue<bool>();
            if (!Orbwalking.CanMove(40)) return;
            var minions = MinionManager.GetMinions(_player.ServerPosition, 500.0F);
            var delay = _config.Item("delayfarm").GetValue<Slider>().Value;
            if (minions.Count < 2) return;

            foreach (var minion in minions)
            {
                if (_config.Item("Use-R-Farm").GetValue<bool>() && _r.IsReady())
                {
                    _r.Cast();
                }

                if (_config.Item("Use-Q-Farm").GetValue<bool>() && _q.IsReady())
                {
                   _q.Cast();
                }

                if (_config.Item("Use-W-Farm").GetValue<bool>() && _w.IsReady())
                {
                  _w.Cast();
                }

                if (_config.Item("Use-E-Farm").GetValue<bool>() && _e.IsReady())
                {
              _e.Cast();
                }

                if (useItemsl && _hydra.IsReady() && minion.IsValidTarget(_hydra.Range))
                {
                    _hydra.Cast();
                }

                if (useItemsl && minion.IsValidTarget(_hydra.Range))
                {
                    _tiamat.Cast();
                }
            }
        }

        private static void Forest()
        {
            if (_player.HasBuff("Recall") || _player.InFountain()) return;
            _player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (_e.IsReady() && _config.Item("ForestE").GetValue<bool>())
            {
                _e.Cast();
            }

            if (_w.IsReady() && _config.Item("ForestW").GetValue<bool>())
            {
                _w.Cast();
            }
        }

        private static void AutoW()
        {
            if (_w.IsReady())
            {
                if (_player.HasBuff("Recall") || _player.InFountain()) return;
                if (Utility.CountEnemiesInRange(1000) >= 1 &&
                    _player.Health <= (_player.MaxHealth * (_config.Item("AutoShield%").GetValue<Slider>().Value) / 100))
                {
                    _w.Cast();
                }
            }
        }

        private static void JungleClear()
        {
            if (!Orbwalking.CanMove(40)) return;
            var useitems = _config.Item("jungleitems").GetValue<bool>();
            var minions = MinionManager.GetMinions(_player.ServerPosition, 400, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var delay = _config.Item("delayfarm").GetValue<Slider>().Value;
            foreach (var minion in minions)
            {
                if (useitems && _hydra.IsReady() && minion.IsValidTarget(_hydra.Range))
                {
                    _hydra.Cast();
                }

                if (useitems && _tiamat.IsReady() && minion.IsValidTarget(_hydra.Range))
                {
                    _tiamat.Cast();
                }
            }

            foreach (var minion in minions)
            {
                if (_config.Item("Use-Q-Jungle").GetValue<bool>() && _q.IsReady() && minion.IsValidTarget())
                {
                    Utility.DelayAction.Add(delay, () => _q.Cast());
                    return;
                }

                if (_config.Item("Use-R-Jungle").GetValue<bool>() && _r.IsReady() && minion.IsValidTarget())
                {
                    Utility.DelayAction.Add(delay, () => _r.Cast());
                    return;
                }

                if (_config.Item("Use-W-Jungle").GetValue<bool>() && _w.IsReady() && minion.IsValidTarget())
                {
                    Utility.DelayAction.Add(delay, () => _w.Cast());
                    return;
                }

                if (_config.Item("Use-E-Jungle").GetValue<bool>() && _e.IsReady() && minion.IsValidTarget())
                {
                    Utility.DelayAction.Add(delay, () => _e.Cast());
                    return;
                }
            }
        }



        private static void UseItemes()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var iBilge = _config.Item("Bilge").GetValue<bool>();
                var iBilgeEnemyhp = hero.Health <=
                                    (hero.MaxHealth * (_config.Item("BilgeEnemyhp").GetValue<Slider>().Value) / 100);
                var iBilgemyhp = _player.Health <=
                                 (_player.MaxHealth * (_config.Item("Bilgemyhp").GetValue<Slider>().Value) / 100);
                var iBlade = _config.Item("Blade").GetValue<bool>();
                var iBladeEnemyhp = hero.Health <=
                                    (hero.MaxHealth * (_config.Item("BladeEnemyhp").GetValue<Slider>().Value) / 100);
                var iBlademyhp = _player.Health <=
                                 (_player.MaxHealth * (_config.Item("Blademyhp").GetValue<Slider>().Value) / 100);
                var iOmen = _config.Item("Omen").GetValue<bool>();
                var iOmenenemys = hero.CountEnemiesInRange(450) >= _config.Item("Omenenemys").GetValue<Slider>().Value;
                var iTiamat = _config.Item("Tiamat").GetValue<bool>();
                var iHydra = _config.Item("Hydra").GetValue<bool>();
                var iRighteous = _config.Item("Righteous").GetValue<bool>();
                var iRighteousenemys = hero.CountEnemiesInRange(_config.Item("Righteousenemysrange").GetValue<Slider>().Value) >= _config.Item("Righteousenemys").GetValue<Slider>().Value;

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
                    Utility.DelayAction.Add(100, () => _rand.Cast());
                }

                if (iRighteousenemys && iRighteous && Items.HasItem(3800) && Items.CanUseItem(3800) &&
                    hero.IsValidTarget(_config.Item("Righteousenemysrange").GetValue<Slider>().Value))
                {
                    Items.UseItem(3800);
                }
            }

            var ilotis = _config.Item("lotis").GetValue<bool>();
            if (ilotis)
            {
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly || hero.IsMe))
                {
                    if (hero.Health <= (hero.MaxHealth * (_config.Item("lotisminhp").GetValue<Slider>().Value) / 100) &&
                        hero.Distance(_player.ServerPosition) <= _lotis.Range && _lotis.IsReady())
                        _lotis.Cast();
                }
            }

        }

        private static void Usepotion()
        {
            var mobs = MinionManager.GetMinions(_player.ServerPosition, _q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var iusehppotion = _config.Item("usehppotions").GetValue<bool>();
            var iusepotionhp = _player.Health <=
                               (_player.MaxHealth * (_config.Item("usepotionhp").GetValue<Slider>().Value) / 100);
            var iusemppotion = _config.Item("usemppotions").GetValue<bool>();
            var iusepotionmp = _player.Mana <=
                               (_player.MaxMana * (_config.Item("usepotionmp").GetValue<Slider>().Value) / 100);
            if (_player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (Utility.CountEnemiesInRange(800) > 0 ||
                (mobs.Count > 0 && _config.Item("ActiveJungle").GetValue<KeyBind>().Active && _smite !=null))
            {
                if (iusepotionhp && iusehppotion &&
                    !(ObjectManager.Player.HasBuff("RegenerationPotion") ||
                      ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                      || ObjectManager.Player.HasBuff("ItemCrystalFlask") ||
                      ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                      || ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")))
                {
                    if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }

                    if (Items.HasItem(2003) && Items.CanUseItem(2003))
                    {
                        Items.UseItem(2003);
                    }

                    if (Items.HasItem(2031) && Items.CanUseItem(2031))
                    {
                        Items.UseItem(2031);
                    }

                    if (Items.HasItem(2032) && Items.CanUseItem(2032))
                    {
                        Items.UseItem(2032);
                    }

                    if (Items.HasItem(2033) && Items.CanUseItem(2033))
                    {
                        Items.UseItem(2033);
                    }
                }

                if (iusepotionmp && iusemppotion &&
                    !(ObjectManager.Player.HasBuff("ItemDarkCrystalFlask") ||
                      ObjectManager.Player.HasBuff("ItemMiniRegenPotion") ||
                      ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle") ||
                      ObjectManager.Player.HasBuff("ItemCrystalFlask")))
                {
                    if (Items.HasItem(2041) && Items.CanUseItem(2041))
                    {
                        Items.UseItem(2041);
                    }

                    if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }

                    if (Items.HasItem(2032) && Items.CanUseItem(2032))
                    {
                        Items.UseItem(2032);
                    }

                    if (Items.HasItem(2033) && Items.CanUseItem(2033))
                    {
                        Items.UseItem(2033);
                    }
                }
            }
        }

        private static void Smiteontarget()
        {
            if (_smite == null) return;
            var hero = HeroManager.Enemies.FirstOrDefault(x => x.IsValidTarget(570));
            var smiteDmg = _player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Smite);
            var usesmite = _config.Item("smitecombo").GetValue<bool>();
            if (_player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteplayerganker" && usesmite &&
                ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready)
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

            if (_player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteduel" && usesmite &&
                ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready &&
                hero.IsValidTarget(570))
            {
                ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
            }
        }

        private static void Combo()
        {
            //Create target

            var target = TargetSelector.GetTarget(_config.Item("TargetRange").GetValue<Slider>().Value,
                TargetSelector.DamageType.Magical);
            var delay = _config.Item("delaycombo").GetValue<Slider>().Value;
            if (target != null && _player.Distance(target) <= _config.Item("TargetRange").GetValue<Slider>().Value)
            {
                Smiteontarget();
                if (_e.IsReady() && !target.HasBuff("udyrbearstuncheck"))
                {
                    _e.Cast();
                    return;
                }

                if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level >=
                    ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Level)
                    if (_q.Cast()) return;

                if (_r.IsReady() && target.HasBuff("udyrbearstuncheck"))
                {
                    Utility.DelayAction.Add(delay, () => _r.Cast());
                    return;
                }

                if (_q.IsReady() && target.HasBuff("udyrbearstuncheck"))
                {
                    Utility.DelayAction.Add(delay, () => _q.Cast());
                    return;
                }

                if (_w.IsReady() && target.HasBuff("udyrbearstuncheck"))
                {
                    Utility.DelayAction.Add(delay, () => _w.Cast());
                    return;
                }

                UseItemes();
            }
        }

        private static void StunCycle()
        {
            Obj_AI_Hero closestEnemy = null;

            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsValidTarget(800) && !enemy.HasBuff("udyrbearstuncheck"))
                {
                    if (_e.IsReady())
                    {
                        _e.Cast();
                    }

                    if (closestEnemy == null)
                    {
                        closestEnemy = enemy;
                    }
                    else if (_player.Distance(closestEnemy) < _player.Distance(enemy))
                    {
                        closestEnemy = enemy;
                    }
                    else if (enemy.HasBuff("udyrbearstuncheck"))
                    {
                        Game.PrintChat(closestEnemy.ChampionName + " has buff already !!!");
                        closestEnemy = enemy;
                        Game.PrintChat(enemy.ChampionName + "is the new target");
                    }

                    if (!enemy.HasBuff("udyrbearstuncheck"))
                    {
                        _player.IssueOrder(GameObjectOrder.AttackUnit, closestEnemy);
                    }

                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_smite != null)
            {
                if (_config.Item("Usesmite").GetValue<KeyBind>().Active)
                {
                    Drawing.DrawText(
                        Drawing.Width * 0.02f,
                        Drawing.Height * 0.88f,
                        System.Drawing.Color.GreenYellow,
                        "Smite Jungle On");
                }
                else
                    Drawing.DrawText(
                        Drawing.Width * 0.02f,
                        Drawing.Height * 0.88f,
                        System.Drawing.Color.OrangeRed,
                        "Smite Jungle Off");
                if (_player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteplayerganker"
                    || _player.GetSpell(_smiteSlot).Name.ToLower() == "s5_summonersmiteduel")
                {
                    if (_config.Item("smitecombo").GetValue<bool>())
                    {
                        Drawing.DrawText(
                            Drawing.Width * 0.02f,
                            Drawing.Height * 0.90f,
                            System.Drawing.Color.GreenYellow,
                            "Smite Target On");
                    }
                    else
                        Drawing.DrawText(
                            Drawing.Width * 0.02f,
                            Drawing.Height * 0.90f,
                            System.Drawing.Color.OrangeRed,
                            "Smite Target Off");
                }
            }

            if (_config.Item("Forest").GetValue<KeyBind>().Active)
            {
                Drawing.DrawText(
                    Drawing.Width * 0.02f,
                    Drawing.Height * 0.92f,
                    System.Drawing.Color.GreenYellow,
                    "Forest Is On");
            }
            else
                Drawing.DrawText(
                    Drawing.Width * 0.02f,
                    Drawing.Height * 0.92f,
                    System.Drawing.Color.OrangeRed,
                    "Forest Is Off");
        }

        public static readonly string[] Smitetype =
        {
            "s5_summonersmiteplayerganker", "s5_summonersmiteduel", "s5_summonersmitequick", "itemsmiteaoe",
            "summonersmite"
        };

        private static int GetSmiteDmg()
        {
            int level = _player.Level;
            int index = _player.Level / 5;
            float[] dmgs = { 370 + 20 * level, 330 + 30 * level, 240 + 40 * level, 100 + 50 * level };
            return (int)dmgs[index];
        }

        //New map Monsters Name By SKO
        private static void Smiteuse()
        {
            var jungle = _config.Item("ActiveJungle").GetValue<KeyBind>().Active;
            if (ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) != SpellState.Ready) return;
            var useblue = _config.Item("Useblue").GetValue<bool>();
            var usered = _config.Item("Usered").GetValue<bool>();
            var health = (100 * (_player.Health / _player.MaxHealth)) < _config.Item("healthJ").GetValue<Slider>().Value;
            var mana = (100 * (_player.Mana / _player.MaxMana)) < _config.Item("manaJ").GetValue<Slider>().Value;
            string[] jungleMinions;
            if (Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline)
            {
                jungleMinions = new string[] { "TT_Spiderboss", "TT_NWraith", "TT_NGolem", "TT_NWolf" };
            }
            else
            {
                jungleMinions = new string[]
                                    {
                                        "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_RiftHerald",
                                        "SRU_Red", "SRU_Krug", "SRU_Dragon_Air", "SRU_Dragon_Water", "SRU_Dragon_Fire",
                                        "SRU_Dragon_Elder", "SRU_Baron"
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
                    else if (jungle && useblue && mana && minion.Health >= smiteDmg &&
                             jungleMinions.Any(name => minion.Name.StartsWith("SRU_Blue")) &&
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
    }
}



