
using System;
using System.Collections.Generic;
using System.Linq;

namespace CampusLostFound.Models
{
    public class MatchService
    {
        public List<Item> FindMatches(Item target, List<Item> items)
        {
            if (target == null || items == null)
                return new List<Item>();

            return items
                .Where(x => x.ItemId != target.ItemId
                    && x.ItemType != target.ItemType
                    && x.Status != "Returned")
                .Select(x => new
                {
                    Item = x,
                    Score = CalculateScore(target, x)
                })
                .Where(x => x.Score >= 2)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Item.ItemDate)
                .Select(x => x.Item)
                .ToList();
        }

        private int CalculateScore(Item a, Item b)
        {
            int score = 0;

            if (Same(a.Category, b.Category))
                score += 3;

            if (Same(a.ItemName, b.ItemName))
                score += 3;

            if (ContainsSimilarWords(a.Description, b.Description))
                score += 2;

            if (Same(a.Location, b.Location))
                score += 2;

            return score;
        }

        private bool Same(string a, string b)
        {
            return !string.IsNullOrWhiteSpace(a)
                && !string.IsNullOrWhiteSpace(b)
                && string.Equals(
                    a.Trim(),
                    b.Trim(),
                    StringComparison.OrdinalIgnoreCase);
        }

        private bool ContainsSimilarWords(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a)
                || string.IsNullOrWhiteSpace(b))
                return false;

            string[] wordsA = a.ToLowerInvariant()
                .Split(new[] { ' ', ',', '.', '-', '/' },
                    StringSplitOptions.RemoveEmptyEntries);

            string[] wordsB = b.ToLowerInvariant()
                .Split(new[] { ' ', ',', '.', '-', '/' },
                    StringSplitOptions.RemoveEmptyEntries);

            return wordsA.Intersect(wordsB).Any();
        }
    }
}
