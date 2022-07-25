// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Services;

[TestFixture]
public class AmbiguousEventTests
{
    [Explicit]
    [TestCase(typeof(ContentService))]
    [TestCase(typeof(MediaService))]
    public void ListAmbiguousEvents(Type serviceType)
    {
        var typedEventHandler = typeof(TypedEventHandler<,>);

        // get all events
        var events = serviceType.GetEvents(BindingFlags.Static | BindingFlags.Public);

        string TypeName(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }

            var sb = new StringBuilder();
            TypeNameSb(type, sb);
            return sb.ToString();
        }

        void TypeNameSb(Type type, StringBuilder sb)
        {
            var name = type.Name;
            var pos = name.IndexOf('`');
            name = pos > 0 ? name.Substring(0, pos) : name;
            sb.Append(name);
            if (!type.IsGenericType)
            {
                return;
            }

            sb.Append("<");
            var first = true;
            foreach (var arg in type.GetGenericArguments())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }

                TypeNameSb(arg, sb);
            }

            sb.Append(">");
        }

        foreach (var e in events)
        {
            // only continue if this is a TypedEventHandler
            if (!e.EventHandlerType.IsGenericType)
            {
                continue;
            }

            var typeDef = e.EventHandlerType.GetGenericTypeDefinition();
            if (typedEventHandler != typeDef)
            {
                continue;
            }

            // get the event args type
            var eventArgsType = e.EventHandlerType.GenericTypeArguments[1];

            // try to find the event back, based upon sender type + args type
            // exclude -ing (eg Saving) events, we don't deal with them in EventDefinitionBase (they always trigger)
            var found = EventNameExtractor.FindEvents(serviceType, eventArgsType, EventNameExtractor.MatchIngNames);

            if (found.Length == 1)
            {
                continue;
            }

            if (found.Length == 0)
            {
                Console.WriteLine($"{typeof(ContentService).Name}  {e.Name}  {TypeName(eventArgsType)}  NotFound");
                continue;
            }

            Console.WriteLine($"{typeof(ContentService).Name}  {e.Name}  {TypeName(eventArgsType)}  Ambiguous");
            Console.WriteLine("\t" + string.Join(", ", found));
        }
    }
}
