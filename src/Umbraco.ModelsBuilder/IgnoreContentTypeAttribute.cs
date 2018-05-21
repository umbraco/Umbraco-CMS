using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Umbraco.ModelsBuilder;

namespace Umbraco.ModelsBuilder
{
    // for the time being it's all-or-nothing
    // when the content type is ignored then
    // - no class is generated for that content type
    // - no class is generated for any child of that class
    // - no interface is generated for that content type as a mixin
    // - and it is ignored as a mixin ie its properties are not generated
    // in the future we may way to do
    // [assembly:IgnoreContentType("foo", ContentTypeIgnorable.ContentType|ContentTypeIgnorable.Mixin|ContentTypeIgnorable.MixinProperties)]
    // so that we can
    // - generate a class for that content type, or not
    // -  if not generated, generate children or not
    // -   if generate children, include properties or not
    // - generate an interface for that content type as a mixin
    // -  if not generated, still generate properties in content types implementing the mixin or not
    // but... I'm not even sure it makes sense
    // if we don't want it... we don't want it.

    // about ignoring
    // - content (don't generate the content, use as mixin)
    // - mixin (don't generate the interface, use the properties)
    // - mixin properties (generate the interface, not the properties)
    // - mixin: local only or children too...

    /// <summary>
    /// Indicates that no model should be generated for a specified content type alias.
    /// </summary>
    /// <remarks>When a content type is ignored, its descendants are also ignored.</remarks>
    /// <remarks>Supports trailing wildcard eg "foo*".</remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class IgnoreContentTypeAttribute : Attribute
    {
        public IgnoreContentTypeAttribute(string alias /*, bool ignoreContent = true, bool ignoreMixin = true, bool ignoreMixinProperties = true*/)
        {}
    }
}

