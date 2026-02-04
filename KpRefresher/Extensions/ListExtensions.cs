using KpRefresher.Domain;
using KpRefresher.Domain.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace KpRefresher.Extensions
{
    public static class ListExtensions
    {
        public static List<(Token, int)> SortByEncounter(this List<(Token, int)> data)
        {
            return [.. data
                    .OrderBy(t => t.Item1.GetAttribute<SortAttribute>().CategoryOrder)
                    .ThenBy(t => t.Item1.GetAttribute<SortAttribute>().EncounterOrder)];
        }
    }
}
