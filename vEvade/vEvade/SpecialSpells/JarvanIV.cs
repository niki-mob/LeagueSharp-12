﻿namespace vEvade.SpecialSpells
{
    #region

    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using vEvade.Core;
    using vEvade.Managers;
    using vEvade.Spells;

    using SpellData = vEvade.Spells.SpellData;

    #endregion

    public class JarvanIV : IChampionManager
    {
        #region Static Fields

        private static bool init;

        #endregion

        #region Public Methods and Operators

        public void LoadSpecialSpell(SpellData spellData)
        {
            if (init)
            {
                return;
            }

            init = true;
            SpellDetector.OnProcessSpell += JarvanQ;
        }

        #endregion

        #region Methods

        private static void JarvanQ(
            Obj_AI_Base sender,
            GameObjectProcessSpellCastEventArgs args,
            SpellData data,
            SpellArgs spellArgs)
        {
            if (data.MenuName != "JarvanIVQ")
            {
                return;
            }

            SpellData qeData;

            if (!Evade.OnProcessSpells.TryGetValue("JarvanIVQE", out qeData))
            {
                return;
            }

            var startPosQ = sender.ServerPosition.To2D();
            var endPosQ = startPosQ.Extend(args.End.To2D(), qeData.Range);
            var endPos = Vector2.Zero;

            foreach (var spell in
                Evade.SpellsDetected.Values.Where(
                    i =>
                    i.Data.MenuName == "JarvanIVE" && i.Unit.NetworkId == sender.NetworkId
                    && i.End.Distance(startPosQ, endPosQ, true) <= qeData.RadiusEx))
            {
                endPos = spell.End;
                break;
            }

            if (!endPos.IsValid())
            {
                foreach (var flag in
                    ObjectManager.Get<Obj_AI_Minion>()
                        .Where(
                            i =>
                            i.IsValid() && !i.IsDead && i.IsVisible && i.CharData.BaseSkinName == "jarvanivstandard"
                            && i.Team == sender.Team
                            && i.ServerPosition.To2D().Distance(startPosQ, endPosQ, true) <= qeData.RadiusEx))
                {
                    endPos = flag.ServerPosition.To2D();
                    break;
                }
            }

            if (endPos.IsValid())
            {
                SpellDetector.AddSpell(sender, startPosQ, endPos.Extend(startPosQ, -110), qeData);
            }
        }

        #endregion
    }
}