using System;

namespace KpRefresher.Domain.Attributes
{
    public class WingAttribute : Attribute
    {
        public int WingNumber { get; set; }

        public WingAttribute(int wingNumber)
        {
            WingNumber = wingNumber;
        }
    }
}
