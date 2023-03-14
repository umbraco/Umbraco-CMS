namespace Umbraco.Cms.Core.Events;

[Serializable]
public delegate void TypedEventHandler<in TSender, in TEventArgs>(TSender sender, TEventArgs e);
