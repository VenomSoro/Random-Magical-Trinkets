using DaggerfallConnect;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;

namespace RandomMagicalTrinketsMod
{
    
    public class Mind : RandomMagicalEffect
    {
        public static readonly string key = "Mind";

        public override string EffectKey => key;

        public override int Variants => 3;

        public override void MagicRound()
        {
            base.MagicRound();

            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            if (EnchantmentParam == null)
                return;

            entityBehaviour.Entity.IncreaseMagicka(1 + (1 * int.Parse(EnchantmentParam.Value.CustomParam)));
        }
    }

    public class Stamina : RandomMagicalEffect
    {
        public static readonly string key = "Stamina";

        public override string EffectKey => key;

        public override int Variants => 3;

        public override void MagicRound()
        {
            base.MagicRound();

            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            if (EnchantmentParam == null)
                return;

            entityBehaviour.Entity.IncreaseFatigue(1 + (1 * int.Parse(EnchantmentParam.Value.CustomParam)), true);
        }
    }

    public class Vitality : RandomMagicalEffect
    {
        public static readonly string key = "Vitality";

        public override string EffectKey => key;

        public override int Variants => 3;

        public override void MagicRound()
        {
            base.MagicRound();

            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            if (EnchantmentParam == null)
                return;

            entityBehaviour.Entity.IncreaseHealth(4 + (2 * int.Parse(EnchantmentParam.Value.CustomParam)));
        }
    }

    public class FortifyStat0 : RandomMagicalEffect
    {
        public static readonly string key = "Fortify Stat 0";

        public override string EffectKey => key;

        public override int Variants => DaggerfallStats.Count;

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);

            if (EnchantmentParam == null)
                return;

            SetStatMod((DFCareer.Stats)int.Parse(EnchantmentParam.Value.CustomParam), 10);
        }
    }

    public class FortifyStat1 : RandomMagicalEffect
    {
        public static readonly string key = "Fortify Stat 1";

        public override string EffectKey => key;

        public override int Variants => DaggerfallStats.Count;

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);

            if (EnchantmentParam == null)
                return;

            SetStatMod((DFCareer.Stats)int.Parse(EnchantmentParam.Value.CustomParam), 15);
        }
    }

    public class FortifyStat2 : RandomMagicalEffect
    {
        public static readonly string key = "Fortify Stat 2";

        public override string EffectKey => key;

        public override int Variants => DaggerfallStats.Count;

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);

            if (EnchantmentParam == null)
                return;

            SetStatMod((DFCareer.Stats)int.Parse(EnchantmentParam.Value.CustomParam), 20);
        }
    }

    public class FortifySkill0 : RandomMagicalEffect
    {
        public static readonly string key = "Fortify Skill 0";

        public override string EffectKey => key;

        public override int Variants => DaggerfallSkills.Count;

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);

            if (EnchantmentParam == null)
                return;

            SetSkillMod((DFCareer.Skills)int.Parse(EnchantmentParam.Value.CustomParam), 10);
        }
    }

    public class FortifySkill1 : RandomMagicalEffect
    {
        public static readonly string key = "Fortify Skill 1";

        public override string EffectKey => key;

        public override int Variants => DaggerfallSkills.Count;

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);

            if (EnchantmentParam == null)
                return;

            SetSkillMod((DFCareer.Skills)int.Parse(EnchantmentParam.Value.CustomParam), 15);
        }
    }

    public class FortifySkill2 : RandomMagicalEffect
    {
        public static readonly string key = "Fortify Skill 2";

        public override string EffectKey => key;

        public override int Variants => DaggerfallSkills.Count;

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);

            if (EnchantmentParam == null)
                return;

            SetSkillMod((DFCareer.Skills)int.Parse(EnchantmentParam.Value.CustomParam), 20);
        }
    }
    
    public class ElementalGuard0 : ElementalGuard
    {
        public static readonly string key = "Elemental Guard 0";

        public override string EffectKey => key;

        public override int Variants => DaggerfallResistances.Count;

        public override int getChance(EnchantmentParam param)
        {
            return 50;
        }
    }

    public class ElementalGuard1 : ElementalGuard
    {
        public static readonly string key = "Elemental Guard 1";

        public override string EffectKey => key;

        public override int Variants => DaggerfallResistances.Count;

        public override int getChance(EnchantmentParam param)
        {
            return 65;
        }
    }

    public class ElementalGuard2 : ElementalGuard
    {
        public static readonly string key = "Elemental Guard 2";

        public override string EffectKey => key;

        public override int Variants => DaggerfallResistances.Count;

        public override int getChance(EnchantmentParam param)
        {
            return 80;
        }
    }

    public class SpellGuard : MagicProtection
    {
        public static readonly string key = "Spell Guard";

        public override string EffectKey => key;

        public override int Variants => 3;

        public override int getChance(EnchantmentParam param)
        {
            return 20 + (15 * int.Parse(param.CustomParam));
        }

        public override int getIconIndex(EnchantmentParam param)
        {
            return 11;
        }

        public override string getEntryKey(EnchantmentParam param)
        {
            return SpellResistance.EffectKey;
        }

        public override string getBundleName(EnchantmentParam param)
        {
            return "Spell Guard";
        }
    }

    public class Reflect : MagicProtection
    {
        public static readonly string key = "Reflect";

        public override string EffectKey => key;

        public override int Variants => 3;

        public override int getChance(EnchantmentParam param)
        {
            return 15 + (10 * int.Parse(param.CustomParam));
        }

        public override int getIconIndex(EnchantmentParam param)
        {
            return 10;
        }

        public override string getEntryKey(EnchantmentParam param)
        {
            return SpellReflection.EffectKey;
        }

        public override string getBundleName(EnchantmentParam param)
        {
            return "Reflect";
        }
    }

    public class Wizardry : RandomMagicalEffect
    {
        public static readonly string key = "Wizardry";

        public override string EffectKey => key;

        public override int Variants => 3;

        public override void ConstantEffect()
        {
            base.ConstantEffect();

            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            if (EnchantmentParam == null)
                return;

            entityBehaviour.Entity.ChangeMaxMagickaModifier(50 + (25 * int.Parse(EnchantmentParam.Value.CustomParam)));
        }
    }

    public class Capacity : RandomMagicalEffect
    {
        public static readonly string key = "Capacity";

        public override string EffectKey => key;

        public override int Variants => 3;

        public override void ConstantEffect()
        {
            base.ConstantEffect();

            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            if (EnchantmentParam == null)
                return;

            entityBehaviour.Entity.SetIncreasedWeightAllowanceMultiplier((1f/3) + ((1f/3) * int.Parse(EnchantmentParam.Value.CustomParam)));
        }
    }

    public class Protection : RandomMagicalEffect
    {
        public static readonly string key = "Protection";

        public override string EffectKey => key;

        public override int Variants => 3;

        public override void ConstantEffect()
        {
            base.ConstantEffect();

            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            if (EnchantmentParam == null)
                return;

            entityBehaviour.Entity.SetIncreasedArmorValueModifier(-5 + (-5 * int.Parse(EnchantmentParam.Value.CustomParam)));
        }
    }

    public class Feather : RandomMagicalEffect
    {
        public static readonly string key = "Feather";

        public override string EffectKey => key;

        public override int Variants => 3;

        public override void SetProperties()
        {
            base.SetProperties();

            properties.EnchantmentPayloadFlags = EnchantmentPayloadFlags.Enchanted | EnchantmentPayloadFlags.Equipped | EnchantmentPayloadFlags.Unequipped; 
        }

        public override PayloadCallbackResults? EnchantmentPayloadCallback(EnchantmentPayloadFlags context, EnchantmentParam? param = null, DaggerfallEntityBehaviour sourceEntity = null, DaggerfallEntityBehaviour targetEntity = null, DaggerfallUnityItem sourceItem = null, int sourceDamage = 0)
        {
            base.EnchantmentPayloadCallback(context, param, sourceEntity, targetEntity, sourceItem, sourceDamage);

            if ((context != EnchantmentPayloadFlags.Enchanted &&
                context != EnchantmentPayloadFlags.Equipped &&
                context != EnchantmentPayloadFlags.Unequipped) ||
                param == null || sourceEntity == null || sourceItem == null)
                return null;

            if (context == EnchantmentPayloadFlags.Enchanted)
            {
                sourceItem.weightInKg = 0.25f;
            }
            else if (context == EnchantmentPayloadFlags.Equipped)
            {
                sourceItem.weightInKg = -20f - (15f * (int.Parse(param.Value.CustomParam)));
            }
            else if (context == EnchantmentPayloadFlags.Unequipped)
            {
                sourceItem.weightInKg = 0.25f;
            }

            return null;
        }
    }
}