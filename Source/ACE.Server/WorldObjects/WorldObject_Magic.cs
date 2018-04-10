using System;
using ACE.Database;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.DatLoader.Entity;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.Entity.Actions;
using ACE.Server.Network.GameEvent.Events;
using ACE.Server.Network.GameMessages.Messages;
using ACE.Server.Network.Motion;
using ACE.Server.WorldObjects.Entity;

namespace ACE.Server.WorldObjects
{
    partial class WorldObject
    {
        private enum SpellLevel
        {
            One     = 1,
            Two     = 50,
            Three   = 100,
            Four    = 150,
            Five    = 200,
            Six     = 250,
            Seven   = 300,
            Eight   = 350
        }

        private SpellLevel CalculateSpellLevel(uint spellPower)
        {
            if (spellPower < 50)
                return SpellLevel.One;
            else if (spellPower < 100)
                return SpellLevel.Two;
            else if (spellPower < 150)
                return SpellLevel.Three;
            else if (spellPower < 200)
                return SpellLevel.Four;
            else if (spellPower < 250)
                return SpellLevel.Five;
            else if (spellPower < 300)
                return SpellLevel.Six;
            else if (spellPower < 350)
                return SpellLevel.Seven;
            else return SpellLevel.Eight;
        }

        /// <summary>
        /// Method used for the scaling, windup motion, and spell gestures for spell casts
        /// </summary>
        private float SpellAttributes(string account, uint spellId, out float castingDelay, out MotionCommand windUpMotion, out MotionCommand spellGesture)
        {
            float scale;

            SpellComponentsTable comps = DatManager.PortalDat.SpellComponentsTable;

            SpellTable spellTable = DatManager.PortalDat.SpellTable;
            if (!spellTable.Spells.ContainsKey(spellId))
            {
                windUpMotion = MotionCommand.Invalid;
                spellGesture = MotionCommand.Invalid;
                castingDelay = 0.0f;
                return -1.0f;
            }

            SpellBase spell = spellTable.Spells[spellId];

            ////Determine scale of the spell effects and windup animation
            SpellLevel spellLevel = CalculateSpellLevel(spell.Power);
            if (account == null)
            {
                switch (spellLevel)
                {
                    case SpellLevel.One:
                        scale = 0.1f;
                        break;
                    case SpellLevel.Two:
                        scale = 0.2f;
                        break;
                    case SpellLevel.Three:
                        scale = 0.4f;
                        break;
                    case SpellLevel.Four:
                        scale = 0.5f;
                        break;
                    case SpellLevel.Five:
                        scale = 0.6f;
                        break;
                    case SpellLevel.Six:
                        scale = 1.0f;
                        break;
                    default:
                        scale = 1.0f;
                        break;
                }

                spellGesture = MotionCommand.Magic;
                windUpMotion = 0;
                castingDelay = 0.0f;
                return scale;
            }

            switch (spellLevel)
            {
                case SpellLevel.One:
                    scale = 0.1f;
                    castingDelay = 1.3f;
                    windUpMotion = MotionCommand.MagicPowerUp01;
                    break;
                case SpellLevel.Two:
                    scale = 0.2f;
                    castingDelay = 1.4f;
                    windUpMotion = MotionCommand.MagicPowerUp02;
                    break;
                case SpellLevel.Three:
                    scale = 0.4f;
                    castingDelay = 1.5f;
                    windUpMotion = MotionCommand.MagicPowerUp03;
                    break;
                case SpellLevel.Four:
                    scale = 0.5f;
                    castingDelay = 1.7f;
                    windUpMotion = MotionCommand.MagicPowerUp04;
                    break;
                case SpellLevel.Five:
                    scale = 0.6f;
                    castingDelay = 1.9f;
                    windUpMotion = MotionCommand.MagicPowerUp05;
                    break;
                case SpellLevel.Six:
                    scale = 1.0f;
                    castingDelay = 2.0f;
                    windUpMotion = MotionCommand.MagicPowerUp06;
                    break;
                default:
                    scale = 1.0f;
                    castingDelay = 2.0f;
                    windUpMotion = MotionCommand.MagicPowerUp07Purple;
                    break;
            }

            var formula = SpellTable.GetSpellFormula(spellTable, spellId, account);
            spellGesture = (MotionCommand)comps.SpellComponents[formula[formula.Count - 1]].Gesture;

            return scale;
        }

        /// <summary>
        /// Method used for handling player targeted spell casts
        /// </summary>
        public void CreatePlayerSpell(ObjectGuid guidTarget, uint spellId)
        {
            Player player = CurrentLandblock.GetObject(Guid) as Player;
            WorldObject target = CurrentLandblock.GetObject(guidTarget);

            if (player.IsBusy == true)
            {
                player.Session.Network.EnqueueSend(new GameEventUseDone(player.Session, errorType: WeenieError.YoureTooBusy));
                return;
            }
            else
                player.IsBusy = true;

            SpellTable spellTable = DatManager.PortalDat.SpellTable;
            if (!spellTable.Spells.ContainsKey(spellId))
            {
                player.Session.Network.EnqueueSend(new GameEventUseDone(player.Session, errorType: WeenieError.MagicInvalidSpellType));
                player.IsBusy = false;
                return;
            }

            SpellBase spell = spellTable.Spells[spellId];

            Database.Models.World.Spell spellStatMod = DatabaseManager.World.GetCachedSpell(spellId);

            uint targetEffect = spell.TargetEffect;

            if (guidTarget == null)
            {
                player.Session.Network.EnqueueSend(new GameEventUseDone(player.Session, errorType: WeenieError.YourSpellTargetIsMissing));
                player.IsBusy = false;
                return;
            }

            if (spell.BaseRangeConstant == 0 && target.WeenieClassId != 1)
            {
                player.Session.Network.EnqueueSend(new GameEventUseDone(player.Session, errorType: WeenieError.IncorrectTargetType));
                player.IsBusy = false;
                return;
            }

            if (spell.School == MagicSchool.LifeMagic && spell.MetaSpellType != SpellType.Transfer)
            {
                if (spellStatMod.Boost > 0 && target.WeenieClassId != 1 && WeenieClassId == 1)
                {
                    player.Session.Network.EnqueueSend(new GameEventUseDone(player.Session, errorType: WeenieError.IncorrectTargetType));
                    player.IsBusy = false;
                    return;
                }
            }

            if (target == null)
                target = player.GetWieldedItem(guidTarget);
            else
            {
                if (guidTarget != Guid)
                {
                    float distanceTo = Location.Distance2D(target.Location);

                    if (distanceTo > (spell.BaseRangeConstant + (spell.BaseRangeConstant * spell.BaseRangeMod)))
                    {
                        player.Session.Network.EnqueueSend(new GameEventUseDone(player.Session, errorType: WeenieError.MagicTargetOutOfRange),
                            new GameMessageSystemChat($"{target.Name} is out of range!", ChatMessageType.Magic));
                        player.IsBusy = false;
                        return;
                    }
                }
            }

            if (spell.School == MagicSchool.WarMagic || spell.School == MagicSchool.VoidMagic)
            {
                player.Session.Network.EnqueueSend(new GameEventUseDone(player.Session, errorType: WeenieError.None),
                    new GameMessageSystemChat($"{spell.Name} spell not implemented, yet!", ChatMessageType.System));
                player.IsBusy = false;
                return;
            }

            float scale = SpellAttributes(player.Session.Account, spellId, out float castingDelay, out MotionCommand windUpMotion, out MotionCommand spellGesture);
            var formula = SpellTable.GetSpellFormula(spellTable, spellId, player.Session.Account);

            CreatureSkill mc = player.GetCreatureSkill(Skill.ManaConversion);
            double z = mc.Current;
            double baseManaPercent = 1;
            if (z > spell.Power)
            {
                baseManaPercent = spell.Power / z;
            }
            Random rnd = new Random();
            double preCost;
            uint manaUsed;
            if (baseManaPercent == 1)
            {
                preCost = spell.BaseMana;
                manaUsed = (uint)preCost;
            }
            else
            {
                preCost = spell.BaseMana * baseManaPercent;
                if (preCost < 1)
                    preCost = 1;
                manaUsed = (uint)rnd.Next(1, (int)preCost);
            }
            if (spell.MetaSpellType == SpellType.Transfer)
            {
                uint vitalChange, casterVitalChange;
                vitalChange = (uint)(player.GetCurrentCreatureVital((PropertyAttribute2nd)spellStatMod.Source) * spellStatMod.Proportion);
                if (spellStatMod.TransferCap != 0)
                {
                    if (vitalChange > spellStatMod.TransferCap)
                        vitalChange = (uint)spellStatMod.TransferCap;
                }
                casterVitalChange = (uint)(vitalChange * (1.0f - spellStatMod.LossPercent));
                vitalChange = (uint)(casterVitalChange / (1.0f - spellStatMod.LossPercent));

                if (spellStatMod.Source == (int)PropertyAttribute2nd.Mana && (vitalChange + 10 + manaUsed) > player.Mana.Current)
                {
                    ActionChain resourceCheckChain = new ActionChain();

                    resourceCheckChain.AddAction(this, () =>
                    {
                        CurrentLandblock.EnqueueBroadcast(Location, new GameMessageScript(Guid, ACE.Entity.Enum.PlayScript.Fizzle, 0.5f));
                    });

                    resourceCheckChain.AddDelaySeconds(2.0f);

                    resourceCheckChain.AddAction(this, () =>
                    {
                        player.Session.Network.EnqueueSend(new GameEventUseDone(player.Session, errorType: WeenieError.YouDontHaveEnoughManaToCast));
                        player.IsBusy = false;
                    });

                    resourceCheckChain.EnqueueChain();

                    return;
                }
                else if ((vitalChange + 10) > player.GetCurrentCreatureVital((PropertyAttribute2nd)spellStatMod.Source))
                {
                    ActionChain resourceCheckChain = new ActionChain();

                    resourceCheckChain.AddAction(this, () =>
                    {
                        CurrentLandblock.EnqueueBroadcast(Location, new GameMessageScript(Guid, ACE.Entity.Enum.PlayScript.Fizzle, 0.5f));
                    });

                    resourceCheckChain.AddDelaySeconds(2.0f);

                    resourceCheckChain.AddAction(this, () =>
                    {
                        player.IsBusy = false;
                    });

                    resourceCheckChain.EnqueueChain();

                    return;
                }
            }
            else if (manaUsed > player.Mana.Current)
            {
                ActionChain resourceCheckChain = new ActionChain();

                resourceCheckChain.AddAction(this, () =>
                {
                    CurrentLandblock.EnqueueBroadcast(Location, new GameMessageScript(Guid, ACE.Entity.Enum.PlayScript.Fizzle, 0.5f));
                });

                resourceCheckChain.AddDelaySeconds(2.0f);

                resourceCheckChain.AddAction(this, () =>
                {
                    player.Session.Network.EnqueueSend(new GameEventUseDone(player.Session, errorType: WeenieError.YouDontHaveEnoughManaToCast));
                    player.IsBusy = false;
                });

                resourceCheckChain.EnqueueChain();

                return;
            }
            else
            {
            rnd = null;
            player.Mana.Current = player.Mana.Current - manaUsed;
            }

#if DEBUG
            player.Session.Network.EnqueueSend(new GameMessageSystemChat("Starting spell casting action chain", ChatMessageType.System));
#endif

            ActionChain spellChain = new ActionChain();

            spellChain.AddAction(this, () =>
            {
                var motionWindUp = new UniversalMotion(MotionStance.Spellcasting);
                motionWindUp.MovementData.CurrentStyle = (ushort)((uint)MotionStance.Spellcasting & 0xFFFF);
                motionWindUp.MovementData.ForwardCommand = (uint)windUpMotion;
                motionWindUp.MovementData.ForwardSpeed = 2;
                DoMotion(motionWindUp);
            });

            spellChain.AddAction(this, () =>
            {
                CurrentLandblock.EnqueueBroadcast(Location, new GameMessageCreatureMessage(SpellComponentsTable.GetSpellWords(DatManager.PortalDat.SpellComponentsTable,
                    formula), Name, Guid.Full, ChatMessageType.Magic));
            });

            spellChain.AddAction(this, () =>
            {
                var motionCastSpell = new UniversalMotion(MotionStance.Spellcasting);
                motionCastSpell.MovementData.CurrentStyle = (ushort)((uint)MotionStance.Spellcasting & 0xFFFF);
                motionCastSpell.MovementData.ForwardCommand = (uint)spellGesture;
                motionCastSpell.MovementData.ForwardSpeed = 2;
                DoMotion(motionCastSpell);
            });

            spellChain.AddDelaySeconds(castingDelay);

            spellChain.AddAction(this, () =>
            {
                bool targetDeath;
                string message;

                switch(spell.School)
                {
                    case MagicSchool.WarMagic:
                        WarMagic(target, spell, spellStatMod);
                        break;
                    case MagicSchool.VoidMagic:
                        VoidMagic(target, spell, spellStatMod);
                        break;
                    case MagicSchool.CreatureEnchantment:
                        CurrentLandblock.EnqueueBroadcast(Location, new GameMessageScript(target.Guid, (PlayScript)spell.TargetEffect, scale));
                        CreatureMagic(target, spell, spellStatMod);
                        break;
                    case MagicSchool.LifeMagic:
                        CurrentLandblock.EnqueueBroadcast(Location, new GameMessageScript(target.Guid, (PlayScript)spell.TargetEffect, scale));
                        targetDeath = LifeMagic(target, spell, spellStatMod, out message);
                        player.Session.Network.EnqueueSend(new GameMessageSystemChat(message, ChatMessageType.Magic));
                        if (targetDeath == true)
                        {
                            Creature creatureTarget = (Creature)target;
                            creatureTarget.Die();
                        }
                        break;
                    case MagicSchool.ItemEnchantment:
                        if (guidTarget == Guid)
                            CurrentLandblock.EnqueueBroadcast(Location, new GameMessageScript(Guid, (PlayScript)spell.CasterEffect, scale));
                        else
                            CurrentLandblock.EnqueueBroadcast(Location, new GameMessageScript(target.Guid, (PlayScript)spell.TargetEffect, scale));
                        ItemMagic(target, spell, spellStatMod);
                        break;
                    default:
                        break;
                }
            });

            spellChain.AddAction(this, () =>
            {
                var motionReturnToCastStance = new UniversalMotion(MotionStance.Spellcasting);
                motionReturnToCastStance.MovementData.CurrentStyle = (ushort)((uint)MotionStance.Spellcasting & 0xFFFF);
                motionReturnToCastStance.MovementData.ForwardCommand = (uint)MotionCommand.Invalid;
                DoMotion(motionReturnToCastStance);
            });

            spellChain.AddDelaySeconds(1.0f);

            spellChain.AddAction(this, () =>
            {
                player.Session.Network.EnqueueSend(new GameEventUseDone(player.Session, errorType: WeenieError.None));
                player.IsBusy = false;
            });

            spellChain.EnqueueChain();

#if DEBUG
            player.Session.Network.EnqueueSend(new GameMessageSystemChat("Spell casting action chain enqueued", ChatMessageType.System));
#endif
            return;
        }

        /// <summary>
        /// Method used for handling player untargeted spell casts
        /// </summary>
        public void CreatePlayerSpell(uint spellId)
        {
            Player player = CurrentLandblock.GetObject(Guid) as Player;

            if (player.IsBusy == true)
            {
                player.Session.Network.EnqueueSend(new GameEventUseDone(player.Session, errorType: WeenieError.YoureTooBusy));
                return;
            }
            else
                player.IsBusy = true;

            SpellTable spellTable = DatManager.PortalDat.SpellTable;
            if (!spellTable.Spells.ContainsKey(spellId))
            {
                player.Session.Network.EnqueueSend(new GameEventUseDone(player.Session, errorType: WeenieError.YourSpellTargetIsMissing));
                player.IsBusy = false;
                return;
            }

            SpellBase spell = spellTable.Spells[spellId];

            float scale = SpellAttributes(player.Session.Account, spellId, out float castingDelay, out MotionCommand windUpMotion, out MotionCommand spellGesture);

            player.Session.Network.EnqueueSend(new GameMessageSystemChat("Targeted SpellID " + spellId + " not yet implemented!", ChatMessageType.System));
            player.Session.Network.EnqueueSend(new GameEventUseDone(player.Session, errorType: WeenieError.None));
            player.IsBusy = false;

            return;
        }

        /// <summary>
        /// Method used for handling creature untargeted spell casts
        /// </summary>
        public void CreateCreatureSpell(ObjectGuid guidTarget, uint spellId)
        {
            Creature creature = CurrentLandblock.GetObject(Guid) as Creature;

            if (creature.IsBusy == true)
                return;
            else
                creature.IsBusy = true;

            SpellTable spellTable = DatManager.PortalDat.SpellTable;
            if (!spellTable.Spells.ContainsKey(spellId))
            {
                creature.IsBusy = false;
                return;
            }

            SpellBase spell = spellTable.Spells[spellId];

            float scale = SpellAttributes(null, spellId, out float castingDelay, out MotionCommand windUpMotion, out MotionCommand spellGesture);

            creature.IsBusy = false;
            return;
        }

        /// <summary>
        /// Method used for handling creature untargeted spell casts
        /// </summary>
        public void CreateCreatureSpell(uint spellId)
        {
            Creature creature = CurrentLandblock.GetObject(Guid) as Creature;

            if (creature.IsBusy == true)
                return;
            else
                creature.IsBusy = true;

            SpellTable spellTable = DatManager.PortalDat.SpellTable;
            if (!spellTable.Spells.ContainsKey(spellId))
            {
                creature.IsBusy = false;
                return;
            }

            SpellBase spell = spellTable.Spells[spellId];

            float scale = SpellAttributes(null, spellId, out float castingDelay, out MotionCommand windUpMotion, out MotionCommand spellGesture);

            creature.IsBusy = false;
            return;
        }

        private bool LifeMagic(WorldObject target, SpellBase spell, Database.Models.World.Spell spellStatMod, out string message)
        {
            string srcVital, destVital, action;

            Random rng = new Random();

            Player player = null;
            Creature creature = null;
            if (WeenieClassId == 1)
                player = (Player)this;
            else if (WeenieType == WeenieType.Creature)
                creature = (Creature)this;

            Creature spellTarget;
            if (spell.BaseRangeConstant == 0)
                spellTarget = (Creature)this;
            else
                spellTarget = (Creature)target;

            int newSpellTargetVital;
            switch (spell.MetaSpellType)
            {
                case SpellType.Boost:
                    int minBoostValue, maxBoostValue;
                    if ((spellStatMod.BoostVariance + spellStatMod.Boost) < spellStatMod.Boost)
                    {
                        minBoostValue = (int)(spellStatMod.BoostVariance + spellStatMod.Boost);
                        maxBoostValue = (int)spellStatMod.Boost;
                    }
                    else
                    {
                        minBoostValue = (int)spellStatMod.Boost;
                        maxBoostValue = (int)(spellStatMod.BoostVariance + spellStatMod.Boost);
                    }
                    int boost = rng.Next(minBoostValue, maxBoostValue);
                    if (boost < 0)
                        action = "drain";
                    else
                        action = "restore";
                    switch (spellStatMod.DamageType)
                    {
                        case 512:   // Mana
                            newSpellTargetVital = (int)(spellTarget.Mana.Current + boost);
                            srcVital = "mana";
                            if (newSpellTargetVital < spellTarget.Mana.MaxValue)
                            {
                                if (newSpellTargetVital <= 0)
                                    spellTarget.Mana.Current = 0;
                                else
                                    spellTarget.Mana.Current = (uint)newSpellTargetVital;
                            }
                            else
                                spellTarget.Mana.Current = spellTarget.Mana.MaxValue;
                            break;
                        case 256:   // Stamina
                            newSpellTargetVital = (int)(spellTarget.Stamina.Current + boost);
                            srcVital = "stamina";
                            if (newSpellTargetVital < spellTarget.Stamina.MaxValue)
                            {
                                if (newSpellTargetVital <= 0)
                                    spellTarget.Stamina.Current = 0;
                                else
                                    spellTarget.Stamina.Current = (uint)newSpellTargetVital;
                            }
                            else
                                spellTarget.Stamina.Current = spellTarget.Stamina.MaxValue;
                            break;
                        default:   // Health
                            newSpellTargetVital = (int)(spellTarget.Health.Current + boost);
                            srcVital = "health";
                            if (newSpellTargetVital < spellTarget.Health.MaxValue)
                            {
                                if (newSpellTargetVital <= 0)
                                    spellTarget.Health.Current = 0;
                                else
                                    spellTarget.Health.Current = (uint)newSpellTargetVital;
                            }
                            else
                                spellTarget.Health.Current = spellTarget.Health.MaxValue;
                            break;
                    }
                    if (WeenieClassId == 1)
                    {
                        if (spell.BaseRangeConstant == 0)
                            message = $"You {action} {Math.Abs(boost).ToString()} {srcVital}";
                        else
                            message = $"You {action} {Math.Abs(boost).ToString()} of {srcVital} from {spellTarget.Name}";
                    }
                    else
                        message = null;
                    break;
                case SpellType.Transfer:
                    // Calculate the change in vitals of the target
                    Creature caster;
                    if (spell.BaseRangeConstant == 0 && spell.BaseRangeMod == 1)
                        caster = spellTarget;
                    else
                        caster = (Creature)this;
                    uint vitalChange, casterVitalChange;
                    vitalChange = (uint)(spellTarget.GetCurrentCreatureVital((PropertyAttribute2nd)spellStatMod.Source) * spellStatMod.Proportion);
                    if (spellStatMod.TransferCap != 0)
                    {
                        if (vitalChange > spellStatMod.TransferCap)
                            vitalChange = (uint)spellStatMod.TransferCap;
                    }
                    casterVitalChange = (uint)(vitalChange * (1.0f - spellStatMod.LossPercent));
                    vitalChange = (uint)(casterVitalChange / (1.0f - spellStatMod.LossPercent));
                    newSpellTargetVital = (int)(spellTarget.GetCurrentCreatureVital((PropertyAttribute2nd)spellStatMod.Source) - vitalChange);

                    // Apply the change in vitals to the target
                    switch (spellStatMod.Source)
                    {
                        case (int)PropertyAttribute2nd.Mana:
                            srcVital = "mana";
                            if (newSpellTargetVital <= 0)
                                spellTarget.Mana.Current = 0;
                            else
                                spellTarget.Mana.Current = (uint)newSpellTargetVital;
                            break;
                        case (int)PropertyAttribute2nd.Stamina:
                            srcVital = "stamina";
                            if (newSpellTargetVital <= 0)
                                spellTarget.Stamina.Current = 0;
                            else
                                spellTarget.Stamina.Current = (uint)newSpellTargetVital;
                            break;
                        default:   // Health
                            srcVital = "health";
                            if (newSpellTargetVital <= 0)
                                spellTarget.Health.Current = 0;
                            else
                                spellTarget.Health.Current = (uint)newSpellTargetVital;
                            break;
                    }

                    // Apply the scaled change in vitals to the caster
                    uint newCasterVital;
                    switch (spellStatMod.Destination)
                    {
                        case (int)PropertyAttribute2nd.Mana:
                            destVital = "mana";
                            newCasterVital = caster.Mana.Current + casterVitalChange;
                            caster.Mana.Current = newCasterVital;
                            if (caster.Mana.Current >= caster.Mana.MaxValue)
                                caster.Mana.Current = caster.Mana.MaxValue;
                            break;
                        case (int)PropertyAttribute2nd.Stamina:
                            destVital = "stamina";
                            newCasterVital = caster.Stamina.Current + casterVitalChange;
                            caster.Stamina.Current = newCasterVital;
                            if (caster.Stamina.Current >= caster.Stamina.MaxValue)
                                caster.Stamina.Current = caster.Stamina.MaxValue;
                            break;
                        default:   // Health
                            destVital = "health";
                            newCasterVital = caster.Mana.Current + casterVitalChange;
                            caster.Health.Current = newCasterVital;
                            if (caster.Health.Current >= caster.Health.MaxValue)
                                caster.Health.Current = caster.Health.MaxValue;
                            break;
                    }

                    if (WeenieClassId == 1)
                    {
                        if (target.Guid == Guid)
                        {
                            message = $"You drain {vitalChange.ToString()} of {srcVital} and apply {casterVitalChange.ToString()} of {destVital} to yourself";
                        }
                        else
                            message = $"You drain {vitalChange.ToString()} of {srcVital} from {spellTarget.Name} and apply {casterVitalChange.ToString()} to yourself";
                    }
                    else
                        message = null;
                    break;
                case SpellType.LifeProjectile:
                    message = "Spell not implemented, yet!";
                    break;
                case SpellType.Dispel:
                    message = "Spell not implemented, yet!";
                    break;
                case SpellType.Enchantment:
                    message = "Spell not implemented, yet!";
                    break;
                default:
                    message = "Spell not implemented, yet!";
                    break;
            }

            if (spellTarget.Health.Current == 0)
                return true;
            else
                return false;
        }

        private void CreatureMagic(WorldObject target, SpellBase spell, Database.Models.World.Spell spellStatMod)
        {
            // TODO
        }

        private void ItemMagic(WorldObject target, SpellBase spell, Database.Models.World.Spell spellStatMod)
        {
            Player player = CurrentLandblock.GetObject(Guid) as Player;

            if (spell.Name.Contains("Portal") || spell.Name.Contains("Lifestone"))
            {
                switch (spell.MetaSpellId)
                {
                    case 2645: // Portal Recall
                        if (!player.TeleToPosition(PositionType.LastPortal))
                        {
                            // You must link to a portal to recall it!
                            player.Session.Network.EnqueueSend(new GameEventWeenieError(player.Session, WeenieError.YouMustLinkToPortalToRecall));
                        }
                        break;
                    case 1635: // Lifestone Recall
                        if (!player.TeleToPosition(PositionType.LinkedLifestone))
                        {
                            // You must link to a lifestone to recall it!
                            player.Session.Network.EnqueueSend(new GameEventWeenieError(player.Session, WeenieError.YouMustLinkToLifestoneToRecall));
                        }
                        break;
                    case 48: // Primary Portal Recall
                        if (!player.TeleToPosition(PositionType.LinkedPortalOne))
                        {
                            // You must link to a portal to recall it!
                            player.Session.Network.EnqueueSend(new GameEventWeenieError(player.Session, WeenieError.YouMustLinkToPortalToRecall));
                        }
                        break;
                    case 2647: // Secondary Portal Recall
                        if (!player.TeleToPosition(PositionType.LinkedPortalTwo))
                        {
                            // You must link to a portal to recall it!
                            player.Session.Network.EnqueueSend(new GameEventWeenieError(player.Session, WeenieError.YouMustLinkToPortalToRecall));
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void WarMagic(WorldObject target, SpellBase spell, Database.Models.World.Spell spellStatMod)
        {
            // TODO
        }

        private void VoidMagic(WorldObject target, SpellBase spell, Database.Models.World.Spell spellStatMod)
        {
            // TODO
        }

    }
}
