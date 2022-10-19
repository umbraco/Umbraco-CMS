// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models.Collections;

public class OrderItem : Item
{
    public readonly string Description;
    public readonly int PartNumber;
    public readonly double UnitPrice;

    private int _quantity;

    public OrderItem(int partNumber, string description, int quantity, double unitPrice)
    {
        PartNumber = partNumber;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public int Quantity
    {
        get => _quantity;

        set
        {
            if (value < 0)
            {
                throw new ArgumentException("Quantity cannot be negative.");
            }

            _quantity = value;
        }
    }

    public override string ToString() =>
        string.Format(
            "{0,9} {1,6} {2,-12} at {3,8:#,###.00} = {4,10:###,###.00}",
            PartNumber,
            _quantity,
            Description,
            UnitPrice,
            UnitPrice * _quantity);
}
