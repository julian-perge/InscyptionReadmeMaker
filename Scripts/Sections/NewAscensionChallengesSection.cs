﻿using System;
using System.Collections.Generic;
using System.Text;
using DiskCardGame;
using InscryptionAPI.Ascension;
using InscryptionAPI.Card;
using FullAbility = InscryptionAPI.Card.AbilityManager.FullAbility;

namespace ReadmeMaker.Sections
{
    public class NewAscensionChallengesSection : ASection
    {
        private List<AscensionChallengeInfo> challenges = null;
        
        public override void Initialize()
        {
            if (!ReadmeConfig.Instance.AscensionChallengesShow)
            {
                challenges = new List<AscensionChallengeInfo>();
                return;
            }

            List<AscensionChallengeInfo> nodes = new List<AscensionChallengeInfo>(ChallengeManager.NewInfos);
            nodes.Sort(SortAscensionChallenges);
            challenges = nodes;
        }

        private static int SortAscensionChallenges(AscensionChallengeInfo a, AscensionChallengeInfo b)
        {
            return String.Compare(a.title, b.title, StringComparison.Ordinal);
        }
        
        public override string GetSectionName()
        {
            return "Ascension Challenges";
        }

        public override void DumpSummary(StringBuilder stringBuilder)
        {
            if (challenges.Count > 0)
            {
                stringBuilder.Append($"- {challenges.Count} {GetSectionName()}\n");
            }
        }

        public override void GetTableDump(out List<TableHeader> headers, out List<Dictionary<string, string>> splitCards)
        {
            splitCards = BreakdownForTable(challenges, out headers, new TableColumn<AscensionChallengeInfo>[]
            {
                new TableColumn<AscensionChallengeInfo>("Name", (a)=>a.title),
                new TableColumn<AscensionChallengeInfo>("Points", (a)=>a.pointValue.ToString()),
                new TableColumn<AscensionChallengeInfo>("Description", (a)=>a.description)
            });
        }
    }
}