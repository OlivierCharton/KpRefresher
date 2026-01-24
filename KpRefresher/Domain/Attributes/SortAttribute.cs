using System;

namespace KpRefresher.Domain.Attributes
{
    public class SortAttribute : Attribute
    {
        public int CategoryOrder { get; set; }
        public int EncounterOrder { get; set; }

        public SortAttribute(int categoryOrder, int encounterOrder)
        {
            CategoryOrder = categoryOrder;
            EncounterOrder = encounterOrder;
        }
    }
}