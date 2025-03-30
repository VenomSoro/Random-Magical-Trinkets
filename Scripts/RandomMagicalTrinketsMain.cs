using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Utility;
using System.Collections.Generic;
using System.Linq;
using DaggerfallWorkshop;
using DaggerfallConnect.Save;
using Wenzil.Console;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Formulas;

namespace RandomMagicalTrinketsMod
{
    public class RandomMagicalTrinketsMain : MonoBehaviour
    {
        private static Mod mod;

        static PlayerEntity player = GameManager.Instance.PlayerEntity;

        // Mod Settings
        private static int trinketGenerationBaseChance;
        private static int trinketQuality1BaseChance;
        private static int trinketQuality2BaseChance;


        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            var go = new GameObject(mod.Title);
            go.AddComponent<RandomMagicalTrinketsMain>();

            mod.LoadSettingsCallback = LoadSettings;

            mod.IsReady = true;
        }

        private void Start()
        {
            mod.LoadSettings();

            RegisterRandomEffects();

            RegisterRMTCommands();

            EnemyDeath.OnEnemyDeath += RandomMagicalTrinkets_OnEnemyDeath;
            LootTables.OnLootSpawned += RandomMagicalTrinnkets_OnDungeonLootSpawned;
        }

        private static void LoadSettings(ModSettings settings, ModSettingsChange change)
        {
            trinketGenerationBaseChance = settings.GetValue<int>("TrinketGenerationChances", "TrinketGenerationBaseChance");
            trinketQuality1BaseChance = settings.GetValue<int>("TrinketGenerationChances", "MajorTrinketBaseChance");
            trinketQuality2BaseChance = settings.GetValue<int>("TrinketGenerationChances", "GrandTrinketBaseChance");
        }

        public static void RegisterRMTCommands()
        {
            Debug.Log("RMT: Trying to register commands.");
            try
            {
                ConsoleCommandsDatabase.RegisterCommand(GenerateTrinket.name, GenerateTrinket.description, GenerateTrinket.usage, GenerateTrinket.Execute);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error registering RMT commands: {0}", e.Message));
            }
        }

        private static class GenerateTrinket
        {
            public static readonly string name = "generate_trinket";
            public static readonly string description = "Generates a random magical trinket";
            public static readonly string usage = "generate_trinket n | quality=n, 0<=n<=2";

            public static string Execute(params string[] args)
            {
                if (args.Length == 0)
                {
                    return usage;
                }

                int quality;
                bool isArgGood = int.TryParse(args[0], out quality);

                if (!isArgGood || quality < 0 || quality > 2)
                {
                    return "Parameter passed is not valid. see usage";
                }

                DaggerfallUnityItem randomTrinket = GenerateRandomItem(quality);

                randomTrinket.customMagic = GenerateMagicEffects(quality, randomTrinket.ItemGroup == ItemGroups.Weapons);
                RenameTrinket(randomTrinket);

                if (randomTrinket.ItemGroup == ItemGroups.Weapons || randomTrinket.ItemGroup == ItemGroups.Armor)
                {
                    randomTrinket.value += 1000 * (quality + 1);
                }
                else
                {
                    randomTrinket.value = 1000 * (quality + 1);
                }

                randomTrinket.IdentifyItem();

                player.Items.AddItem(randomTrinket);

                return "Received a random magical trinket";
            }
        }

        public static void RandomMagicalTrinkets_OnEnemyDeath(object sender, EventArgs e)
        {
            EnemyDeath enemyDeath = sender as EnemyDeath;

            if (enemyDeath != null)
            {
                DaggerfallEntityBehaviour entityBehaviour = enemyDeath.GetComponent<DaggerfallEntityBehaviour>();

                if (entityBehaviour != null)
                {
                    EnemyEntity enemyEntity = entityBehaviour.Entity as EnemyEntity;

                    if (enemyEntity != null && !IsNotValidEnemy(enemyEntity))
                    {
                        bool isHighValueTarget = IsHighValueTarget(enemyEntity.MobileEnemy.ID);
                        DaggerfallUnityItem randomTrinket = RollTrinketGeneration(isHighValueTarget);


                        if (randomTrinket != null)
                        {
                            entityBehaviour.CorpseLootContainer.Items.AddItem(randomTrinket);
                        }
                    }
                }
            }
        }

        public static void RandomMagicalTrinnkets_OnDungeonLootSpawned(object sender, TabledLootSpawnedEventArgs e)
        {
            if (!IsDungeon(e.LocationIndex))
                return;

            DaggerfallUnityItem randomTrinket = RollTrinketGeneration(false);

            if (randomTrinket != null)
            {
                e.Items.AddItem(randomTrinket);
            }
        }

        private static DaggerfallUnityItem RollTrinketGeneration(bool isHighValue)
        {
            int rollGeneration = Dice100.Roll();
            int quality = -1;
            int[] chances = calculateTrinketGenerationChances(isHighValue);
            DaggerfallUnityItem randomTrinket = null;

            //Debug.Log("Roll for Generation: " + rollGeneration + " vs. " + chances[0]);
            if (rollGeneration <= chances[0])
            {
                int rollQuality = Dice100.Roll();

                //Debug.Log("Roll for Grand Tier: " + rollQuality + " vs. " + chances[2]);
                //Debug.Log("Roll for Major Tier: " + rollQuality + " vs. " + chances[1]);
                if (rollQuality <= chances[2])
                {
                    quality = 2;
                }
                else if (rollQuality <= chances[1])
                {
                    quality = 1;
                }
                else
                {
                    quality = 0;
                }
            }

            if (quality >= 0)
            {
                randomTrinket = GenerateRandomItem(quality);

                randomTrinket.customMagic = GenerateMagicEffects(quality, randomTrinket.ItemGroup == ItemGroups.Weapons);
                RenameTrinket(randomTrinket);

                if (randomTrinket.ItemGroup == ItemGroups.Weapons || randomTrinket.ItemGroup == ItemGroups.Armor)
                {
                    randomTrinket.value += 1000 * (quality + 1);
                }
                else
                {
                    randomTrinket.value = 1000 * (quality + 1);
                }
            }

            return randomTrinket;
        }

        private static DaggerfallUnityItem GenerateRandomItem(int quality)
        {
            DaggerfallUnityItem randomItem;
            int rollItemType = Dice100.Roll();

            if (rollItemType <= 40)
            {
                rollItemType = UnityEngine.Random.Range(0, 3);

                if (rollItemType == 0)
                {
                    randomItem = GenerateRandomArmor(quality);
                }
                else if (rollItemType == 1)
                {
                    randomItem = GenerateRandomWeapon(quality);
                }
                else
                    randomItem = ItemBuilder.CreateRandomClothing(player.Gender, player.Race);
            }
            else
            {
                rollItemType = Dice100.Roll();

                if (rollItemType <= 30)
                {
                    randomItem = ItemBuilder.CreateRandomGem();
                }
                else
                {
                    randomItem = GenerateRandomJewellery();
                }
            }

            return randomItem;
        }

        private static DaggerfallUnityItem GenerateRandomArmor(int quality)
        {
            DaggerfallUnityItem armor = ItemBuilder.CreateRandomArmor(player.Level, player.Gender, player.Race);

            // Quality = 0 -> 2% | Quality = 1 -> 8% | Quality = 2 -> 32%
            int DaedricChance = (int)(2 * Mathf.Pow(4, quality));
            if (Dice100.SuccessRoll(DaedricChance) && armor.NativeMaterialValue != (int)ArmorMaterialTypes.Daedric)
            {
                ItemBuilder.ApplyArmorSettings(armor, player.Gender, player.Race, ArmorMaterialTypes.Daedric, -1);
            }

            return armor;
        }

        private static DaggerfallUnityItem GenerateRandomWeapon(int quality)
        {
            WeaponMaterialTypes minMaterial = GetMinimumWeaponMaterial(quality);
            WeaponMaterialTypes currentMaterial = FormulaHelper.RandomMaterial(player.Level);
            int[] customWeapons = GameManager.Instance.ItemHelper.GetCustomItemsForGroup(ItemGroups.Weapons);
            DaggerfallUnityItem weapon;

            // 0 - 17 for vanilla weapons (except for the 19th item arrows with index 18)
            // Weapons enum lengtth is 19, so 18 without arrows
            int randWeaponIndex = UnityEngine.Random.Range(0, 18 + customWeapons.Length);

            if (randWeaponIndex < 18)
            {
                weapon = new DaggerfallUnityItem(ItemGroups.Weapons, randWeaponIndex);
            }
            else
            {
                weapon = ItemBuilder.CreateItem(ItemGroups.Weapons, customWeapons[randWeaponIndex - 18]);
            }

            int DaedricChance = (int)(2 * Mathf.Pow(4, quality));
            if (Dice100.SuccessRoll(DaedricChance) && currentMaterial != WeaponMaterialTypes.Daedric)
            {
                currentMaterial = WeaponMaterialTypes.Daedric;
            }
            else if (currentMaterial < minMaterial)
            {
                currentMaterial = minMaterial;
            }

            ItemBuilder.ApplyWeaponMaterial(weapon, currentMaterial);

            return weapon;
        }

        private static WeaponMaterialTypes GetMinimumWeaponMaterial(int quality)
        {
            WeaponMaterialTypes minMaterial = WeaponMaterialTypes.Steel;

            if (quality == 2)
                minMaterial = WeaponMaterialTypes.Mithril;
            else if (quality == 1)
                minMaterial = WeaponMaterialTypes.Elven;

            return minMaterial;
        }

        // Exists primarily to generate magical items excluding the wand item
        private static DaggerfallUnityItem GenerateRandomJewellery()
        {
            int randJewl = UnityEngine.Random.Range(0, 7); // 0 to 6, Wand is the 8th item in the Jewellery Enum (index = 7)

            DaggerfallUnityItem jewellery = new DaggerfallUnityItem(ItemGroups.Jewellery, randJewl);

            return jewellery;
        }

        private static CustomEnchantment[] GenerateMagicEffects(int quality, bool isWeapon)
        {
            List<CustomEnchantment> magicEffects = new List<CustomEnchantment>();
            List<string> effects = new List<string> { "Mind", "Stamina", "Vitality", "Fortify Stat", "Fortify Skill", "Elemental Guard", "Spell Guard", "Reflect", "Wizardry", "Capacity", "Protection", "Feather" };
            List<int> skills = Enumerable.Range(0, 35).ToList();
            List<int> stats = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };

            if (isWeapon)
            {
                // Add weapon-specific enchantmnets to the list to potentially be picked
                effects.AddRange(new List<string> { "Elemental Strike", "Miracles", "Silencing", "Ruin" });
            }

            for (int i = 0; i <= quality; i++)
            {
                int randEffect = UnityEngine.Random.Range(0, effects.Count);
                string selectedEffect = effects.ElementAt(randEffect);
                CustomEnchantment enchantment;

                if (selectedEffect.Equals("Fortify Stat"))
                {
                    int randStat = UnityEngine.Random.Range(0, stats.Count);
                    enchantment = GenerateCustomEnchantment("Fortify Stat " + quality, stats.ElementAt(randStat).ToString());
                    stats.RemoveAt(randStat);
                }
                else if (selectedEffect.Equals("Fortify Skill"))
                {
                    int randSkill = UnityEngine.Random.Range(0, skills.Count);
                    enchantment = GenerateCustomEnchantment("Fortify Skill " + quality, skills.ElementAt(randSkill).ToString());
                    skills.RemoveAt(randSkill);
                }
                else if (selectedEffect.Equals("Elemental Guard"))
                {
                    int randElemental = UnityEngine.Random.Range(0, DaggerfallResistances.Count);
                    enchantment = GenerateCustomEnchantment("Elemental Guard " + quality, randElemental.ToString());
                    effects.RemoveAt(randEffect);
                }
                else if (selectedEffect.Equals("Elemental Strike")) {
                    int randElemental = UnityEngine.Random.Range(0, 5);
                    enchantment = GenerateCustomEnchantment("Elemental Strike " + quality, randElemental.ToString());
                    effects.RemoveAt(randEffect);
                }
                else
                {
                    enchantment = GenerateCustomEnchantment(selectedEffect, quality.ToString());
                    effects.RemoveAt(randEffect);
                }

                magicEffects.Add(enchantment);
            }

            return magicEffects.ToArray();
        }

        private static CustomEnchantment GenerateCustomEnchantment(String effectKey, String customParam)
        {
            CustomEnchantment customEnchantment = new CustomEnchantment()
            {
                EffectKey = effectKey,
                CustomParam = customParam
            };

            return customEnchantment;
        }

        private static void RenameTrinket(DaggerfallUnityItem trinket)
        {
            int numOfEffects = trinket.customMagic.Length;
            string name;

            if (numOfEffects == 1)
            {
                name = trinket.ItemName + " of Minor " + getEffectName(trinket.customMagic[0]);
            }
            else if (numOfEffects == 2)
            {
                name = trinket.ItemName + " of Major " + getEffectName(trinket.customMagic[0]) + " & " + getEffectName(trinket.customMagic[1]);
            }
            else
            {
                name = trinket.ItemName + " of Grand " + getEffectName(trinket.customMagic[0]) + ", " + getEffectName(trinket.customMagic[1]) + ", & " + getEffectName(trinket.customMagic[2]);
            }

            trinket.RenameItem(name);
        }

        private static string getEffectName(CustomEnchantment customEnchantment)
        {
            string effectName = customEnchantment.EffectKey;
            string[] elementKey = { "Fire", "Frost", "Poison", "Shock", "Magicka" };
            string[] elementStrikeKey = { "Flames", "Blizzards", "Acid", "Storms", "Force" };
            int customParm = int.Parse(customEnchantment.CustomParam);

            switch (effectName)
            {
                case "Fortify Stat 0":
                case "Fortify Stat 1":
                case "Fortify Stat 2":
                    effectName = ((DFCareer.Stats)customParm).ToString();
                    break;
                case "Fortify Skill 0":
                case "Fortify Skill 1":
                case "Fortify Skill 2":
                    effectName = ((DFCareer.Skills)customParm) + " Expertise";
                    break;
                case "Elemental Guard 0":
                case "Elemental Guard 1":
                case "Elemental Guard 2":
                    effectName = elementKey[customParm] + " Guard";
                    break;
                case "Elemental Strike 0":
                case "Elemental Strike 1":
                case "Elemental Strike 2":
                    effectName = elementStrikeKey[customParm];
                    break;
            }

            return effectName;
        }

        private static int[] calculateTrinketGenerationChances(bool isHighValue)
        {
            int[] chanceValues = new int[3];
            int highValueBonusMulti = isHighValue ? 1 : 0;
            int playerLuck = player.Stats.LiveLuck;

            // chance for generation
            chanceValues[0] = (int)(trinketGenerationBaseChance * (1 + (playerLuck / 100f) + (1 * highValueBonusMulti)));
            // chance for Major Quality (after generation succeeds)
            chanceValues[1] = (int)(trinketQuality1BaseChance * (1 + (playerLuck / 100f) + (1 * highValueBonusMulti)));
            // chance for Grand Quality (after generation succeeds)
            chanceValues[2] = (int)(trinketQuality2BaseChance * (1 + (playerLuck / 100f) + (1 * highValueBonusMulti)));

            return chanceValues;
        }

        // High Value targets (Daedra, Liches, Vampires) increase generation and tier chance by 100%
        private static bool IsHighValueTarget(int enemyID)
        {
            switch (enemyID)
            {
                case (int)MobileTypes.AncientLich:
                case (int)MobileTypes.DaedraLord:
                case (int)MobileTypes.DaedraSeducer:
                case (int)MobileTypes.FireDaedra:
                case (int)MobileTypes.FrostDaedra:
                case (int)MobileTypes.Daedroth:
                case (int)MobileTypes.Lich:
                case (int)MobileTypes.Vampire:
                case (int)MobileTypes.VampireAncient:
                    return true;
            }

            return false;
        }

        // Animals, atronachs, and certain types of enemies are not allowed to drop trinkets
        private static bool IsNotValidEnemy(EnemyEntity enemyEntity)
        {
            MobileAffinity enemyAffinity = enemyEntity.MobileEnemy.Affinity;
            if (enemyAffinity == MobileAffinity.Animal || enemyAffinity == MobileAffinity.Golem)
                return true;


            switch(enemyEntity.MobileEnemy.ID)
            {
                case (int)MobileTypes.Slaughterfish:
                case (int)MobileTypes.Gargoyle:
                case (int)MobileTypes.Spriggan:
                case (int)MobileTypes.Imp:
                case (int)MobileTypes.Dragonling:
                case (int)MobileTypes.Dragonling_Alternate:
                case (int)MobileTypes.Harpy:
                    return true;
            }

            return false;
        }

        private static bool IsDungeon(int locationIndex)
        {
            switch(locationIndex)
            {
                case (int)DFRegion.DungeonTypes.BarbarianStronghold:
                case (int)DFRegion.DungeonTypes.Cemetery:
                case (int)DFRegion.DungeonTypes.Coven:
                case (int)DFRegion.DungeonTypes.Crypt:
                case (int)DFRegion.DungeonTypes.DesecratedTemple:
                case (int)DFRegion.DungeonTypes.DragonsDen:
                case (int)DFRegion.DungeonTypes.GiantStronghold:
                case (int)DFRegion.DungeonTypes.HarpyNest:
                case (int)DFRegion.DungeonTypes.HumanStronghold:
                case (int)DFRegion.DungeonTypes.Laboratory:
                case (int)DFRegion.DungeonTypes.Mine:
                case (int)DFRegion.DungeonTypes.NaturalCave:
                case (int)DFRegion.DungeonTypes.OrcStronghold:
                case (int)DFRegion.DungeonTypes.Prison:
                case (int)DFRegion.DungeonTypes.RuinedCastle:
                case (int)DFRegion.DungeonTypes.ScorpionNest:
                case (int)DFRegion.DungeonTypes.SpiderNest:
                case (int)DFRegion.DungeonTypes.VampireHaunt:
                case (int)DFRegion.DungeonTypes.VolcanicCaves:
                    return true;
            }

            return false;
        }

        private void RegisterRandomEffects()
        {
            Mind mind = new Mind();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(mind);

            Stamina stamina = new Stamina();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(stamina);

            Vitality vitality = new Vitality();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(vitality);

            FortifyStat0 fortifyStat0 = new FortifyStat0();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(fortifyStat0);

            FortifyStat1 fortifyStat1 = new FortifyStat1();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(fortifyStat1);

            FortifyStat2 fortifyStat2 = new FortifyStat2();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(fortifyStat2);

            FortifySkill0 fortifySkill0 = new FortifySkill0();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(fortifySkill0);

            FortifySkill1 fortifySkill1 = new FortifySkill1();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(fortifySkill1);

            FortifySkill2 fortifySkill2 = new FortifySkill2();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(fortifySkill2);

            ElementalGuard0 elementalResist0 = new ElementalGuard0();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(elementalResist0);

            ElementalGuard1 elementalResist1 = new ElementalGuard1();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(elementalResist1);

            ElementalGuard2 elementalResist2 = new ElementalGuard2();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(elementalResist2);

            SpellGuard spellGuard = new SpellGuard();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(spellGuard);

            Reflect reflect = new Reflect();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(reflect);

            Wizardry wizardry = new Wizardry();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(wizardry);

            Capacity capacity = new Capacity();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(capacity);

            Protection protection = new Protection();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(protection);

            Feather feather = new Feather();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(feather);

            ElementalStrike0 elementalStrike0 = new ElementalStrike0();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(elementalStrike0);

            ElementalStrike1 elementalStrike1 = new ElementalStrike1();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(elementalStrike1);

            ElementalStrike2 elementalStrike2 = new ElementalStrike2();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(elementalStrike2);

            MiracleStrike miracleStrike = new MiracleStrike();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(miracleStrike);

            SilencingStrike silencingStrike = new SilencingStrike();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(silencingStrike);

            RuinStrike ruinStrike = new RuinStrike();
            GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(ruinStrike);
        }
    }
}
