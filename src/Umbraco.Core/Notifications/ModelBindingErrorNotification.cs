using System.Text;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Contains event data for the <see cref="ModelBindingException" /> event.
/// </summary>
public class ModelBindingErrorNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ModelBindingErrorNotification" /> class.
    /// </summary>
    public ModelBindingErrorNotification(Type sourceType, Type modelType, StringBuilder message)
    {
        SourceType = sourceType;
        ModelType = modelType;
        Message = message;
    }

    /// <summary>
    ///     Gets the type of the source object.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    ///     Gets the type of the view model.
    /// </summary>
    public Type ModelType { get; }

    /// <summary>
    ///     Gets the message string builder.
    /// </summary>
    /// <remarks>Handlers of the event can append text to the message.</remarks>
    public StringBuilder Message { get; }
}
