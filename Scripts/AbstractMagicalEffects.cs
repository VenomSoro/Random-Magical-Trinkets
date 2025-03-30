using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using System.Collections.Generic;
using UnityEngine;

namespace RandomMagicalTrinketsMod
{
    public abstract class RandomMagicalEffect : BaseEntityEffect
    {
        public abstract string EffectKey { get; }
        public abstract int Variants { get; }

        public override void SetProperties()
        {
            properties.Key = EffectKey;
            properties.ShowSpellIcon = false;
            properties.AllowedTargets = TargetTypes.CasterOnly;
            properties.EnchantmentPayloadFlags = EnchantmentPayloadFlags.Held;
        }

        public override EnchantmentSettings[] GetEnchantmentSettings()
        {
            List<EnchantmentSettings> enchantments = new List<EnchantmentSettings>();

            for (int i = 0; i < Variants; i++)
            {
                EnchantmentSettings enchantment = new EnchantmentSettings()
                {
                    Version = 1,
                    EffectKey = EffectKey,
                    CustomParam = i.ToString(),
                    PrimaryDisplayName = EffectKey,
                    EnchantCost = 1000,
                };

                enchantments.Add(enchantment);
            }

            return enchantments.ToArray();
        }
    }

    public abstract class MagicProtection : RandomMagicalEffect
    {
        public override void SetProperties()
        {
            base.SetProperties();

            properties.EnchantmentPayloadFlags = EnchantmentPayloadFlags.Equipped | EnchantmentPayloadFlags.MagicRound | EnchantmentPayloadFlags.RerollEffect;
        }

        public override PayloadCallbackResults? EnchantmentPayloadCallback(EnchantmentPayloadFlags context, EnchantmentParam? param = null, DaggerfallEntityBehaviour sourceEntity = null, DaggerfallEntityBehaviour targetEntity = null, DaggerfallUnityItem sourceItem = null, int sourceDamage = 0)
        {
            base.EnchantmentPayloadCallback(context, param, sourceEntity, targetEntity, sourceItem, sourceDamage);

            if ((context != EnchantmentPayloadFlags.Equipped &&
                context != EnchantmentPayloadFlags.MagicRound &&
                context != EnchantmentPayloadFlags.RerollEffect) ||
                param == null || sourceEntity == null || sourceItem == null)
                return null;

            EntityEffectManager effectManager = sourceEntity.GetComponent<EntityEffectManager>();
            if (!effectManager)
                return null;

            if (context == EnchantmentPayloadFlags.Equipped)
            {
                CreateBundle(param.Value, sourceEntity, sourceItem, effectManager);
            }
            else if (context == EnchantmentPayloadFlags.RerollEffect)
            {
                CreateBundle(param.Value, sourceEntity, sourceItem, effectManager);
            }

            return null;
        }

        public void CreateBundle(EnchantmentParam param, DaggerfallEntityBehaviour sourceEntity, DaggerfallUnityItem sourceItem, EntityEffectManager effectManager)
        {
            string key = getEntryKey(param);
            int chance = getChance(param);
            string name = getBundleName(param);
            int iconIndex = getIconIndex(param);

            EffectEntry effectEntry = createEffectEntry(key, chance);

            EffectBundleSettings effectBundleSettings = createEffectBundleSettings(name, effectEntry);
            effectBundleSettings.Icon.index = iconIndex;

            EntityEffectBundle bundle = new EntityEffectBundle(effectBundleSettings, sourceEntity);
            bundle.FromEquippedItem = sourceItem;
            bundle.AddRuntimeFlags(BundleRuntimeFlags.ItemRecastEnabled);

            effectManager.AssignBundle(bundle, AssignBundleFlags.BypassSavingThrows);

            sourceItem.timeEffectsLastRerolled = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
        }

        public abstract string getEntryKey(EnchantmentParam param);
        public abstract int getChance(EnchantmentParam param);
        public abstract string getBundleName(EnchantmentParam param);
        public abstract int getIconIndex(EnchantmentParam param);

        public EffectEntry createEffectEntry(string key, int chance)
        {
            EffectEntry entry = new EffectEntry()
            {
                Key = key,
                Settings = new EffectSettings()
                {
                    DurationBase = 12,
                    DurationPlus = 12,
                    DurationPerLevel = 1,
                    ChanceBase = chance,
                    ChancePlus = 0,
                    ChancePerLevel = 1
                }
            };

            return entry;
        }

        public EffectBundleSettings createEffectBundleSettings(string name, EffectEntry effectEntry)
        {
            EffectBundleSettings bundleSettings = new EffectBundleSettings()
            {
                Version = EntityEffectBroker.CurrentSpellVersion,
                BundleType = BundleTypes.HeldMagicItem,
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = name,
                Effects = new EffectEntry[] { effectEntry },
                Icon = new SpellIcon()
            };

            return bundleSettings;
        }
    }

    public abstract class ElementalGuard : MagicProtection
    {
        static readonly string[] elementVariants = { "Fire", "Frost", "Poison", "Shock", "Magicka" };
        static readonly int[] elementIcon = { 58, 55, 57, 38, 56 };

        public override string getEntryKey(EnchantmentParam param)
        {
            return "ElementalResistance-" + elementVariants[int.Parse(param.CustomParam)];
        }

        public override string getBundleName(EnchantmentParam param)
        {
            return elementVariants[int.Parse(param.CustomParam)] + " Guard";
        }

        public override int getIconIndex(EnchantmentParam param)
        {
            return elementIcon[int.Parse(param.CustomParam)];
        }
    }

    public abstract class WeaponStrike : RandomMagicalEffect
    {
        public override void SetProperties()
        {
            properties.Key = EffectKey;
            properties.ShowSpellIcon = false;
            properties.EnchantmentPayloadFlags = EnchantmentPayloadFlags.Strikes;
        }

        public override EnchantmentSettings[] GetEnchantmentSettings()
        {
            List<EnchantmentSettings> enchantments = new List<EnchantmentSettings>();

            EnchantmentSettings enchantment = new EnchantmentSettings()
            {
                Version = 1,
                EffectKey = EffectKey,
                EnchantCost = 1000,
                PrimaryDisplayName = EffectKey
            };

            enchantments.Add(enchantment);

            return enchantments.ToArray();
        }

        public override PayloadCallbackResults? EnchantmentPayloadCallback(EnchantmentPayloadFlags context, EnchantmentParam? param = null, DaggerfallEntityBehaviour sourceEntity = null, DaggerfallEntityBehaviour targetEntity = null, DaggerfallUnityItem sourceItem = null, int sourceDamage = 0)
        {
            base.EnchantmentPayloadCallback(context, param, sourceEntity, targetEntity, sourceItem, sourceDamage);

            if (context != EnchantmentPayloadFlags.Strikes || targetEntity == null || param == null || sourceDamage == 0)
                return null;

            EntityEffectManager effectManager = targetEntity.GetComponent<EntityEffectManager>();
            if (!effectManager)
                return null;

            CreateDamageBundle(param.Value, sourceEntity, effectManager);

            return new PayloadCallbackResults()
            {
                // Durability loss less than standard 10 since effects do not scale with level like vanilla enchants
                durabilityLoss = 5
            };
        }

        public void CreateDamageBundle(EnchantmentParam param, DaggerfallEntityBehaviour sourceEntity, EntityEffectManager effectManager)
        {
            EffectEntry effectEntry = CreateEffectEntry(param);

            EffectBundleSettings effectBundleSettings = CreateEffectBundleSettings(param, effectEntry);

            EntityEffectBundle bundle = new EntityEffectBundle(effectBundleSettings, sourceEntity);

            effectManager.AssignBundle(bundle, AssignBundleFlags.ShowNonPlayerFailures);
        }

        public virtual EffectEntry CreateEffectEntry(EnchantmentParam param)
        {
            EffectEntry effectEntry = new EffectEntry()
            {
                Settings = new EffectSettings()
            };

            return effectEntry;
        }

        public virtual EffectBundleSettings CreateEffectBundleSettings(EnchantmentParam param, EffectEntry effectEntry)
        {
            EffectBundleSettings effectBundleSettings = new EffectBundleSettings()
            {
                Version = EntityEffectBroker.CurrentSpellVersion,
                BundleType = BundleTypes.Spell,
                TargetType = TargetTypes.ByTouch,
                ElementType = ElementTypes.Magic,
                Effects = new EffectEntry[] { effectEntry },
                Icon = new SpellIcon()
            };

            return effectBundleSettings;
        }
    }

    public abstract class ElementalStrike : WeaponStrike
    {
        public abstract int minMagnitude { get; }
        public abstract int maxMagnitude { get; }

        static readonly int[] elementIcon = { 58, 55, 57, 38, 56 };

        public override int Variants => 5;

        public override EffectEntry CreateEffectEntry(EnchantmentParam param)
        {
            EffectEntry effectEntry = new EffectEntry()
            {
                Key = DamageHealth.EffectKey,
                Settings = new EffectSettings()
                {
                    MagnitudeBaseMin = minMagnitude,
                    MagnitudeBaseMax = maxMagnitude,
                    MagnitudePlusMin = 0,
                    MagnitudePlusMax = 0,
                    MagnitudePerLevel = 1
                }
            };

            return effectEntry;
        }

        public override EffectBundleSettings CreateEffectBundleSettings(EnchantmentParam param, EffectEntry effectEntry)
        {
            int[] elementTypesKey = { 1, 2, 4, 8, 16 };
            string[] elementNames = { "Flames", "Blizzards", "Acid", "Storms", "Force" };
            int variant = int.Parse(param.CustomParam);

            EffectBundleSettings effectBundleSettings = new EffectBundleSettings()
            {
                Version = EntityEffectBroker.CurrentSpellVersion,
                BundleType = BundleTypes.Spell,
                TargetType = TargetTypes.ByTouch,
                ElementType = (ElementTypes)elementTypesKey[variant],
                Name = elementNames[variant] + " Strike",
                Effects = new EffectEntry[] { effectEntry },
                Icon = new SpellIcon()
            };
            effectBundleSettings.Icon.index = elementIcon[variant];

            //Debug.Log($"{effectBundleSettings.Name} damage: {effectEntry.Settings.MagnitudeBaseMin} - {effectEntry.Settings.MagnitudeBaseMax}");
            //Debug.Log($"{effectBundleSettings.Name} element: {effectBundleSettings.ElementType}");

            return effectBundleSettings;
        }
    }
}