using System;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Modelsl.Collections
{ 
    public class OrderItem : Item
    {
        public readonly int PartNumber;
        public readonly string Description;
        public readonly double UnitPrice;

        private int _quantity = 0;

        public OrderItem(int partNumber, string description,
            int quantity, double unitPrice)
        {
            PartNumber = partNumber;
            Description = description;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Quantity cannot be negative.");

                _quantity = value;
            }
        }

        public override string ToString()
        {
            return string.Format(
                "{0,9} {1,6} {2,-12} at {3,8:#,###.00} = {4,10:###,###.00}",
                PartNumber, _quantity, Description, UnitPrice,
                UnitPrice * _quantity);
        }
    }
}
