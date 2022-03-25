﻿using System;
using System.Collections.Generic;
using System.Text;
using InscryptionAPI.Card;
using SpecialAbility = InscryptionAPI.Card.SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility;

namespace ReadmeMaker.Sections
{
    public class NewSpecialAbilitiesSection : ASection
    {
        private List<SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility> allAbilities = null;
        
        public override void Initialize()
        {
            if (!ReadmeConfig.Instance.SpecialAbilitiesShow)
            {
                allAbilities = new List<SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility>();
                return;
            }


            allAbilities = ReadmeHelpers.GetAllNewSpecialAbilities();
	        
            // Remove special abilities that have no rulebook entry
            var icons = ReadmeHelpers.GetAllNewStatInfoIcons();
            for (int i = 0; i < allAbilities.Count; i++)
            {
                SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility specialAbility = allAbilities[i];
                StatIconManager.FullStatIcon fullStatIcon = icons.Find((b) => b.VariableStatBehavior == specialAbility.AbilityBehaviour);
                if (fullStatIcon == null || fullStatIcon.Info == null || string.IsNullOrEmpty(fullStatIcon.Info.rulebookName))
                {
                    allAbilities.RemoveAt(i--);
                }
            }
	        
            allAbilities.Sort(SortNewSpecialAbilities);
        }
        
        private static int SortNewSpecialAbilities(SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility a, SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility b)
        {
            var icons = ReadmeHelpers.GetAllNewStatInfoIcons();
            StatIconManager.FullStatIcon aStatIcon = icons.Find((icon) => icon.VariableStatBehavior == a.AbilityBehaviour);
            StatIconManager.FullStatIcon bStatIcon = icons.Find((icon) => icon.VariableStatBehavior == b.AbilityBehaviour);
            return String.Compare(aStatIcon.Info.rulebookName, bStatIcon.Info.rulebookName, StringComparison.Ordinal);
        }
        
        public override string GetSectionName()
        {
            return "New Special Abilities";
        }

        public override void DumpSummary(StringBuilder stringBuilder)
        {
            if (allAbilities.Count > 0)
            {
                stringBuilder.Append($"- {allAbilities.Count} {GetSectionName()}\n");
            }
        }

        public override void GetTableDump(out List<TableHeader> headers, out List<Dictionary<string, string>> splitCards)
        {
            splitCards = BreakdownForTable(allAbilities, out headers, new TableColumn<SpecialAbility>[]
            {
                new TableColumn<SpecialAbility>("Name", ReadmeHelpers.GetSpecialAbilityName),
                new TableColumn<SpecialAbility>("Description", ReadmeHelpers.GetSpecialAbilityDescription)
            });
        }
    }
}