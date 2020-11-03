using System;

namespace Umbraco.Core.Configuration
{
    public interface ICronTabParser
    {
        bool IsValidCronTab(string cronTab);
        DateTime GetNextOccurrence(string cronTab, DateTime time);
    }
}
