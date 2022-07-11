// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Extensions;

public static class ContentTypeChangeExtensions
{
    public static bool HasType(this ContentTypeChangeTypes change, ContentTypeChangeTypes type) =>
        (change & type) != ContentTypeChangeTypes.None;

    public static bool HasTypesAll(this ContentTypeChangeTypes change, ContentTypeChangeTypes types) =>
        (change & types) == types;

    public static bool HasTypesAny(this ContentTypeChangeTypes change, ContentTypeChangeTypes types) =>
        (change & types) != ContentTypeChangeTypes.None;

    public static bool HasTypesNone(this ContentTypeChangeTypes change, ContentTypeChangeTypes types) =>
        (change & types) == ContentTypeChangeTypes.None;
}
