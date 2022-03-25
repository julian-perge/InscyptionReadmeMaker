﻿using System.Collections.Generic;
using DiskCardGame;

namespace ReadmeMaker.Sections
{
    public class NewRareCardsSection : AllNewCards
    {
        public override string SectionName => "New Rare Cards";

        protected override List<CardInfo> GetCards()
        {
            List<CardInfo> allCards = base.GetCards();
            allCards.RemoveAll((a) => !a.metaCategories.Contains(CardMetaCategory.Rare));
            return allCards;
        }
    }
}