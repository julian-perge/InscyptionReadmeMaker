﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using APIPlugin;
using BepInEx.Bootstrap;
using DiskCardGame;

namespace ReadmeMaker
{
    public static class ReadmeDump
    {
	    // List if different costs the mod supports
	    // Ordered by what will be shown. Vanilla first then Custom last
	    public static List<ACost> Costs = new List<ACost>()
	    {
		    new BloodCost(),
		    new BoneCost(),
		    new EnergyCost(),
		    new MoxBlueCost(),
		    new MoxGreenCost(),
		    new MoxOrangeCost(),
		    
		    // List Custom costs here
		    new LifeCost(),
	    };

	    // Custom Traits made by mods that we want to show the name of instead of a number
	    private static Dictionary<Trait, string> TraitToName = new Dictionary<Trait, string>()
	    {
		    { (Trait)5103, "Side Deck" }
	    };

	    // Custom Tribes made by mods that we want to show the name of instead of a number
	    private static Dictionary<Tribe, string> TribeToName = new Dictionary<Tribe, string>()
	    {
		    
	    };
	    
	    private static List<SpecialTriggeredAbility> PowerModifyingSpecials = new List<SpecialTriggeredAbility>()
	    {
		    SpecialTriggeredAbility.Ant, SpecialTriggeredAbility.Mirror, SpecialTriggeredAbility.Lammergeier
	    };
	    
	    private static List<SpecialTriggeredAbility> HealthModifyingSpecials = new List<SpecialTriggeredAbility>()
	    {
		    SpecialTriggeredAbility.Lammergeier
	    };

        public static void Dump()
        {
	        Plugin.Log.LogInfo("Generating Readme...");
	        string text = GetDumpString();

	        string fullPath = GetOutputFullPath();
	        Plugin.Log.LogInfo("Dumping Readme to '" + fullPath + "'");
	        
	        File.WriteAllText(fullPath, text);
	    }

        private static string GetOutputFullPath()
        {
	        string defaultPath = Path.Combine(Plugin.Directory, "GENERATED_README.md");
	        string path = Plugin.ReadmeConfig.SavePath;
	        if (string.IsNullOrEmpty(path))
	        {
		        path = defaultPath;
		        return path;
	        }
	        
	        string directory = Path.GetDirectoryName(path);
	        
	        // Create directory if it doesn't exist
	        if (!Directory.Exists(directory))
	        {
		        Directory.CreateDirectory(directory);
	        }
	        
	        // Append file name if there is none
	        if (path.IndexOf('.') < 0)
	        {
		        path = Path.Combine(path, "GENERATED_README.md");
	        }
	        
	        
	        return path;
        }

        private static string GetDumpString()
        {
			// TODO: Dynamically find Power&Health modifiers: eg: Mirror, Ants, Lammergeier
			// TODO: Fix vanilla Special abilities not using their rulebook name 
			// TODO: Section to show all configs 
			// TODO: Support for mods to add their own names and descriptions for costs/tribes/trait... etc renames 
			
	        //
	        // Initialize everything for the Summary
	        //
	        List<CardInfo> allCards = GetAllCards();
	        Plugin.Log.LogInfo(allCards.Count + " All New Cards");

	        List<CardInfo> modifiedCards = GetModifiedCards();
	        Plugin.Log.LogInfo(modifiedCards.Count + " Modified Cards");

	        List<CardInfo> newCards = GetNewCards(allCards);
	        Plugin.Log.LogInfo(newCards.Count + " New Cards");
	        
	        List<CardInfo> newRareCards = GetNewRareCards(allCards);
	        Plugin.Log.LogInfo(newRareCards.Count + " New Rare Cards");
	        
	        List<CardInfo> sideDeckCards = GetSideDeckCards();
	        Plugin.Log.LogInfo(sideDeckCards.Count + " Side Deck Cards");

	        
	        List<NewAbility> abilities = GetNewAbilities();
	        Plugin.Log.LogInfo(abilities.Count + " New Abilities");
	        
	        List<NewSpecialAbility> specialAbilities = GetNewSpecialAbilities();
	        Plugin.Log.LogInfo(specialAbilities.Count + " New Special Abilities");

	        // Does not work.
	        //GetPowerAndHealthModifiers();
	        
	        //
	        // Build string
	        //

	        switch (Plugin.ReadmeConfig.CardDisplayByType)
	        {
		        case ReadmeConfig.DisplayType.List:
			        return ReadmeListMaker.Dump(allCards, newCards, newRareCards, modifiedCards, sideDeckCards, abilities, specialAbilities);
		        case ReadmeConfig.DisplayType.Table:
			        return ReadmeTableMaker.Dump(allCards, newCards, newRareCards, modifiedCards, sideDeckCards, abilities, specialAbilities);
		        default:
			        throw new ArgumentOutOfRangeException();
	        }
        }

        /// <summary>
        /// Uses reflection or something to find all subclasses of VariableStatBehaviour and record them for the readme maker to show properly.
        /// TODO: Investigate how to get classes from different mods more
        /// </summary>
        private static void GetPowerAndHealthModifiers()
        {
	        foreach (Type type in typeof(VariableStatBehaviour).Assembly.GetTypes()
		        .Where(type => type.IsSubclassOf(typeof(VariableStatBehaviour))))
	        {
		        Plugin.Log.LogInfo("Sub type: '" + type + "'");
		        
		        FieldInfo info = type.GetField("specialStatIcon", BindingFlags.NonPublic | BindingFlags.Static);
		        Plugin.Log.LogInfo("Field '" + info + "'");
		        if (info == null)
		        {
			        continue;
		        }

		        object value = info.GetValue(null);
		        Plugin.Log.LogInfo("Got '" + value + "'");
	        }
        }

        private static List<NewSpecialAbility> GetNewSpecialAbilities()
        {
	        if (!Plugin.ReadmeConfig.SpecialAbilitiesShow)
		        return new List<NewSpecialAbility>();
	        
		    List<NewSpecialAbility> specialAbilities = NewSpecialAbility.specialAbilities;
	        specialAbilities.RemoveAll((a) => a.statIconInfo == null || string.IsNullOrEmpty(a.statIconInfo.rulebookName));
	        specialAbilities.Sort((a, b) => String.Compare(a.statIconInfo.rulebookName, b.statIconInfo.rulebookName, StringComparison.Ordinal));
	        return specialAbilities;
        }

        private static List<CardInfo> GetAllCards()
        {
	        List<CardInfo> allCards = NewCard.cards;
	        if (!Plugin.ReadmeConfig.CardShowUnobtainable)
	        {
		        allCards = allCards.FindAll((a) => a.metaCategories.Count > 0);
	        }

	        return allCards;
        }

        private static List<CardInfo> GetNewCards(List<CardInfo> allCards)
        {
	        List<CardInfo> newCards = allCards.FindAll((a) =>
		        !a.appearanceBehaviour.Contains(CardAppearanceBehaviour.Appearance.RareCardBackground));
	        newCards.Sort(SortCards);
	        return newCards;
        }

        private static List<CardInfo> GetNewRareCards(List<CardInfo> allCards)
        {
	        List<CardInfo> newRareCards = allCards.FindAll((a) =>
		        a.appearanceBehaviour.Contains(CardAppearanceBehaviour.Appearance.RareCardBackground));
	        newRareCards.Sort(SortCards);
	        return newRareCards;
        }

        private static List<CardInfo> GetSideDeckCards()
        {
	        if (!Plugin.ReadmeConfig.SideDeckShow)
	        {
		        return new List<CardInfo>();
	        }
	        
	        List<CardInfo> allCards = ScriptableObjectLoader<CardInfo>.AllData;
	        List<CardInfo> sideDeckCards = allCards.FindAll((a) => a.HasTrait((Trait)5103));
	        sideDeckCards.Sort(SortCards);
	        return sideDeckCards;
        }

        private static List<NewAbility> GetNewAbilities()
        {
	        List<NewAbility> abilities = Plugin.ReadmeConfig.SigilsShow ? NewAbility.abilities : new List<NewAbility>();
	        abilities.RemoveAll((a) => a.info == null || string.IsNullOrEmpty(a.info.rulebookName));
	        abilities.Sort((a, b) => String.Compare(a.info.rulebookName, b.info.rulebookName, StringComparison.Ordinal));
	        return abilities;
        }

        private static List<CardInfo> GetModifiedCards()
        {
	        List<CardInfo> modifiedCards = new List<CardInfo>();
	        if (!Plugin.ReadmeConfig.ModifiedCardsShow)
	        {
		        return modifiedCards;
	        }
	     
	        List<CardInfo> allData = ScriptableObjectLoader<CardInfo>.AllData;   
	        foreach (CustomCard card in CustomCard.cards)
	        {
		        int index = allData.FindIndex((Predicate<CardInfo>)(x => x.name == card.name));
		        if (index >= 0)
		        {
			        modifiedCards.Add(allData[index]);
		        }
	        }

	        modifiedCards.Sort(SortCards);
	        return modifiedCards;
        }

        public static void AppendSummary(StringBuilder stringBuilder, List<CardInfo> newCards, List<CardInfo> modifiedCards, List<CardInfo> sideDeckCards, List<NewAbility> abilities, List<NewSpecialAbility> specialAbilities)
        {
	        stringBuilder.Append("### Includes:\n");
	        if (newCards.Count > 0)
	        {
		        stringBuilder.Append($"- {newCards.Count} New Cards:\n");
	        }
	        
	        if (modifiedCards.Count > 0)
	        {
		        stringBuilder.Append($"- {modifiedCards.Count} Modified Cards:\n");
	        }
	        
	        if (sideDeckCards.Count > 0)
	        {
		        stringBuilder.Append($"- {sideDeckCards.Count} Side Deck Cards:\n");
	        }

	        if (abilities.Count > 0)
	        {
		        stringBuilder.Append($"- {abilities.Count} New Sigils:\n");
	        }

	        if (specialAbilities.Count > 0)
	        {
		        stringBuilder.Append($"- {specialAbilities.Count} New Special Abilities:\n");
	        }
        }

        private static int SortCards(CardInfo a, CardInfo b)
        {
	        int sorted = 0;
	        switch (Plugin.ReadmeConfig.CardSortBy)
	        {
		        case ReadmeConfig.SortByType.Cost:
			        sorted = CompareByCost(a, b); 
			        break;
		        case ReadmeConfig.SortByType.Name:
			        sorted = CompareByDisplayName(a, b); 
			        break;
	        }

	        if (!Plugin.ReadmeConfig.CardSortAscending)
	        {
		        return sorted * -1;
	        }
	        
	        return sorted;
        }

        private static int CompareByDisplayName(CardInfo a, CardInfo b)
        {
	        return String.Compare(a.displayedName.ToLower(), b.displayedName.ToLower(), StringComparison.Ordinal);
        }

        private static int CompareByCost(CardInfo a, CardInfo b)
        {
	        List<Tuple<int, int>> aCosts = GetCostType(a);
	        List<Tuple<int, int>> bCosts = GetCostType(b);

	        // Show least amount of costs at the top (Blood, Bone, Blood&Bone)
	        if (aCosts.Count != bCosts.Count)
	        {
		        return aCosts.Count - bCosts.Count;
	        }
	        
	        // Show lowest cost first (Blood, Bone, Energy)
	        for (var i = 0; i < aCosts.Count; i++)
	        {
		        Tuple<int, int> aCost = aCosts[i];
		        Tuple<int, int> bCost = bCosts[i];
		        if (aCost.Item1 != bCost.Item1)
		        {
			        return aCost.Item1 - bCost.Item1;
		        }
	        }

	        // Show lowest amounts first (1 Blood, 2 Blood)
	        for (var i = 0; i < aCosts.Count; i++)
	        {
		        Tuple<int, int> aCost = aCosts[i];
		        Tuple<int, int> bCost = bCosts[i];
		        if (aCost.Item2 != bCost.Item2)
		        {
			        return aCost.Item2 - bCost.Item2;
		        }
	        }

	        ListPool.Push(aCosts);
	        ListPool.Push(bCosts);

	        // Same Costs
	        // Default to Name
	        return CompareByDisplayName(a, b);
        }
        
        private static List<Tuple<int, int>> GetCostType(CardInfo a)
        {
	        List<Tuple<int, int>> list = ListPool.Pull<Tuple<int, int>>();
	        if (a.BloodCost > 0)
	        {
		        list.Add(new Tuple<int, int>(0, a.BloodCost));
	        }
	        if (a.bonesCost > 0)
	        {
		        list.Add(new Tuple<int, int>(1, a.bonesCost));
	        }
	        if (a.energyCost > 0)
	        {
		        list.Add(new Tuple<int, int>(2, a.energyCost));
	        }
	        if (a.gemsCost.Count > 0)
	        {
		        for (int i = 0; i < a.gemsCost.Count; i++)
		        {
			        switch (a.gemsCost[i])
			        {
				        case GemType.Green:
					        list.Add(new Tuple<int, int>(3, 1));
					        break;
				        case GemType.Orange:
					        list.Add(new Tuple<int, int>(4, 1));
					        break;
				        case GemType.Blue:
					        list.Add(new Tuple<int, int>(5, 1));
					        break;
				        default:
					        throw new ArgumentOutOfRangeException();
			        }
		        }
	        }

	        return list;
        }

		public static void AppendAllCosts(CardInfo info, StringBuilder builder)
		{
			bool hasCost = false;
			for (int i = 0; i < Costs.Count; i++)
			{
				hasCost |= Costs[i].AppendCost(info, builder);
			}
			
			// Add Free if we don't get a cost
			if (!hasCost)
			{
				if (Plugin.ReadmeConfig.CardDisplayByType == ReadmeConfig.DisplayType.Table)
				{
					builder.Append($"Free");
				}
				else
				{
					builder.Append($" Free.");
				}
			}
		}

		public static string GetSpecialAbilityName(SpecialTriggeredAbility ability)
		{
			if (ability <= SpecialTriggeredAbility.NUM_ABILITIES)
			{
				return ability.ToString();
			}

			for (int i = 0; i < NewSpecialAbility.specialAbilities.Count; i++)
			{
				if (NewSpecialAbility.specialAbilities[i].specialTriggeredAbility == ability)
				{
					return NewSpecialAbility.specialAbilities[i].statIconInfo.rulebookName;
				}
			}

			return null;
		}
		
		public static string GetAbilityName(NewAbility newAbility)
		{
			return newAbility.info.rulebookName;
		}
        
		// In-game, when the rulebook description for a sigil is being displyed all instances of "[creature]" are replaced with "A card bearing this sigil".
		// We do this when generating the readme as well for the sake of consistency.
		public static string GetAbilityDescription(NewAbility newAbility)
		{
			// Seeing "[creature]" appear in the readme looks jarring, sigil descriptions should appear exactly as they do in the rulebook for consistency
			string description = newAbility.info.rulebookDescription;
			return description.Replace("[creature]", "A card bearing this sigil");
		}

		public static string GetTraitName(Trait trait)
		{
			if (TraitToName.TryGetValue(trait, out string name))
			{
				return name;
			}
			
			return trait.ToString();
		}

		public static string GetTribeName(Tribe tribe)
		{
			if (TribeToName.TryGetValue(tribe, out string name))
			{
				return name;
			}
			
			return tribe.ToString();
		}

		public static string GetPower(CardInfo info)
		{
			string power = "";
			for (int i = 0; i < PowerModifyingSpecials.Count; i++)
			{
				if(info.SpecialAbilities.Contains(PowerModifyingSpecials[i]))
				{
					if (!string.IsNullOrEmpty(power))
					{
						power += ", ";
					}
					power += GetSpecialAbilityName(PowerModifyingSpecials[i]);
				}
			}

			if (string.IsNullOrEmpty(power))
			{
				return info.Attack.ToString();
			}
			else if (info.Attack > 0)
			{
				return power + " + " + info.Attack;
			}
			else
			{
				return power;
			}
		}

		public static string GetHealth(CardInfo info)
		{
			string health = "";
			for (int i = 0; i < HealthModifyingSpecials.Count; i++)
			{
				if(info.SpecialAbilities.Contains(HealthModifyingSpecials[i]))
				{
					if (!string.IsNullOrEmpty(health))
					{
						health += ", ";
					}
					health += GetSpecialAbilityName(HealthModifyingSpecials[i]);
				}
			}

			if (string.IsNullOrEmpty(health))
			{
				return info.Health.ToString();
			}
			else if (info.Health > 0)
			{
				return health + " + " + info.Health;
			}
			else
			{
				return health;
			}
		}
    }
}
