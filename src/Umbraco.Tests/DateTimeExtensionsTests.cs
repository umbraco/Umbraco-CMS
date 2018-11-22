using System;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests
{
    [TestFixture]
    public class DateTimeExtensionsTests
    {
        [Test]
        public void PeriodicMinutesFrom_PostTime_CalculatesMinutesBetween()
        {
            var nowDateTime = new DateTime(2017, 1, 1, 10, 30, 0);
            var scheduledTime = "1145";
            var minutesBetween = nowDateTime.PeriodicMinutesFrom(scheduledTime);
            Assert.AreEqual(75, minutesBetween);
        }

        [Test]
        public void PeriodicMinutesFrom_PriorTime_CalculatesMinutesBetween()
        {
            var nowDateTime = new DateTime(2017, 1, 1, 10, 30, 0);
            var scheduledTime = "900";
            var minutesBetween = nowDateTime.PeriodicMinutesFrom(scheduledTime);
            Assert.AreEqual(1350, minutesBetween);
        }

        [Test]
        public void PeriodicMinutesFrom_PriorTime_WithLeadingZero_CalculatesMinutesBetween()
        {
            var nowDateTime = new DateTime(2017, 1, 1, 10, 30, 0);
            var scheduledTime = "0900";
            var minutesBetween = nowDateTime.PeriodicMinutesFrom(scheduledTime);
            Assert.AreEqual(1350, minutesBetween);
        }

        [Test]
        public void PeriodicMinutesFrom_SameTime_CalculatesMinutesBetween()
        {
            var nowDateTime = new DateTime(2017, 1, 1, 10, 30, 0);
            var scheduledTime = "1030";
            var minutesBetween = nowDateTime.PeriodicMinutesFrom(scheduledTime);
            Assert.AreEqual(0, minutesBetween);
        }
    }
}
