using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;

namespace Umbraco.Cms.Core.Xml.XPath;

/// <summary>
///     Provides a cursor model for navigating {macro /} as if it were XML.
/// </summary>
public class MacroNavigator : XPathNavigator
{
    private readonly MacroRoot _macro;
    private readonly XmlNameTable _nameTable;

    /// <summary>
    ///     Gets a value indicating whether the current node is an empty element without an end element tag.
    /// </summary>
    public override bool IsEmptyElement
    {
        get
        {
            DebugEnter("IsEmptyElement");
            bool isEmpty;

            switch (InternalState.Position)
            {
                case StatePosition.Macro:
                    isEmpty = _macro.Parameters.Length == 0;
                    break;
                case StatePosition.Parameter:
                    MacroParameter parameter = _macro.Parameters[InternalState.ParameterIndex];
                    XPathNavigator? nav = parameter.NavigatorValue;
                    if (parameter.WrapNavigatorInNodes || nav != null)
                    {
                        isEmpty = false;
                    }
                    else
                    {
                        var s = _macro.Parameters[InternalState.ParameterIndex].StringValue;
                        isEmpty = s == null;
                    }

                    break;
                case StatePosition.ParameterNavigator:
                    isEmpty = InternalState.ParameterNavigator?.IsEmptyElement ?? true;
                    break;
                case StatePosition.ParameterNodes:
                    isEmpty = _macro.Parameters[InternalState.ParameterIndex].NavigatorValue == null;
                    break;
                case StatePosition.ParameterAttribute:
                case StatePosition.ParameterText:
                case StatePosition.Root:
                    throw new InvalidOperationException("Not an element.");
                default:
                    throw new InvalidOperationException("Invalid position.");
            }

            DebugReturn(isEmpty);
            return isEmpty;
        }
    }

    /// <summary>
    ///     Gets the qualified name of the current node.
    /// </summary>
    public override string Name
    {
        get
        {
            DebugEnter("Name");
            string name;

            switch (InternalState.Position)
            {
                case StatePosition.Macro:
                    name = "macro";
                    break;
                case StatePosition.Parameter:
                    name = _macro.Parameters[InternalState.ParameterIndex].Name;
                    break;
                case StatePosition.ParameterAttribute:
                    name = _macro.Parameters[InternalState.ParameterIndex]
                        .Attributes?[InternalState.ParameterAttributeIndex].Key ?? string.Empty;
                    break;
                case StatePosition.ParameterNavigator:
                    name = InternalState.ParameterNavigator?.Name ?? string.Empty;
                    break;
                case StatePosition.ParameterNodes:
                    name = "nodes";
                    break;
                case StatePosition.ParameterText:
                case StatePosition.Root:
                    name = string.Empty;
                    break;
                default:
                    throw new InvalidOperationException("Invalid position.");
            }

            DebugReturn("\"{0}\"", name);
            return name;
        }
    }

    /// <summary>
    ///     Gets the Name of the current node without any namespace prefix.
    /// </summary>
    public override string LocalName
    {
        get
        {
            DebugEnter("LocalName");
            var name = Name;
            DebugReturn("\"{0}\"", name);
            return name;
        }
    }

    /// <summary>
    ///     Gets the base URI for the current node.
    /// </summary>
    public override string BaseURI => string.Empty;

    /// <summary>
    ///     Gets the XmlNameTable of the XPathNavigator.
    /// </summary>
    public override XmlNameTable NameTable => _nameTable;

    /// <summary>
    ///     Gets the namespace URI of the current node.
    /// </summary>
    public override string NamespaceURI => string.Empty;

    /// <summary>
    ///     Gets the XPathNodeType of the current node.
    /// </summary>
    public override XPathNodeType NodeType
    {
        get
        {
            DebugEnter("NodeType");
            XPathNodeType type;

            switch (InternalState.Position)
            {
                case StatePosition.Macro:
                case StatePosition.Parameter:
                case StatePosition.ParameterNodes:
                    type = XPathNodeType.Element;
                    break;
                case StatePosition.ParameterNavigator:
                    type = InternalState.ParameterNavigator?.NodeType ?? XPathNodeType.Root;
                    break;
                case StatePosition.ParameterAttribute:
                    type = XPathNodeType.Attribute;
                    break;
                case StatePosition.ParameterText:
                    type = XPathNodeType.Text;
                    break;
                case StatePosition.Root:
                    type = XPathNodeType.Root;
                    break;
                default:
                    throw new InvalidOperationException("Invalid position.");
            }

            DebugReturn("\'{0}\'", type);
            return type;
        }
    }

    /// <summary>
    ///     Gets the namespace prefix associated with the current node.
    /// </summary>
    public override string Prefix => string.Empty;

    /// <summary>
    ///     Gets the string value of the item.
    /// </summary>
    /// <remarks>
    ///     Does not fully behave as per the specs, as we report empty value on root and macro elements, and we start
    ///     reporting values only on parameter elements. This is because, otherwise, we would might dump the whole database
    ///     and it probably does not make sense at Umbraco level.
    /// </remarks>
    public override string Value
    {
        get
        {
            DebugEnter("Value");
            string value;

            XPathNavigator? nav;
            switch (InternalState.Position)
            {
                case StatePosition.Parameter:
                    nav = _macro.Parameters[InternalState.ParameterIndex].NavigatorValue;
                    if (nav != null)
                    {
                        nav = nav.Clone(); // never use the raw parameter's navigator
                        nav.MoveToFirstChild();
                        value = nav.Value;
                    }
                    else
                    {
                        var s = _macro.Parameters[InternalState.ParameterIndex].StringValue;
                        value = s ?? string.Empty;
                    }

                    break;
                case StatePosition.ParameterAttribute:
                    value = _macro.Parameters[InternalState.ParameterIndex]
                        .Attributes?[InternalState.ParameterAttributeIndex].Value ?? string.Empty;
                    break;
                case StatePosition.ParameterNavigator:
                    value = InternalState.ParameterNavigator?.Value ?? string.Empty;
                    break;
                case StatePosition.ParameterNodes:
                    nav = _macro.Parameters[InternalState.ParameterIndex].NavigatorValue;
                    if (nav == null)
                    {
                        value = string.Empty;
                    }
                    else
                    {
                        nav = nav.Clone(); // never use the raw parameter's navigator
                        nav.MoveToFirstChild();
                        value = nav.Value;
                    }

                    break;
                case StatePosition.ParameterText:
                    value = _macro.Parameters[InternalState.ParameterIndex].StringValue ?? string.Empty;
                    break;
                case StatePosition.Macro:
                case StatePosition.Root:
                    value = string.Empty;
                    break;
                default:
                    throw new InvalidOperationException("Invalid position.");
            }

            DebugReturn("\"{0}\"", value);
            return value;
        }
    }

    /// <summary>
    ///     Creates a new XPathNavigator positioned at the same node as this XPathNavigator.
    /// </summary>
    /// <returns>A new XPathNavigator positioned at the same node as this XPathNavigator.</returns>
    public override XPathNavigator Clone()
    {
        DebugEnter("Clone");
        var nav = new MacroNavigator(_macro, _nameTable, InternalState.Clone());
        DebugCreate(nav);
        DebugReturn("[XPathNavigator]");
        return nav;
    }

    /// <summary>
    ///     Determines whether the current XPathNavigator is at the same position as the specified XPathNavigator.
    /// </summary>
    /// <param name="nav">The XPathNavigator to compare to this XPathNavigator.</param>
    /// <returns>true if the two XPathNavigator objects have the same position; otherwise, false.</returns>
    public override bool IsSamePosition(XPathNavigator nav)
    {
        DebugEnter("IsSamePosition");
        bool isSame;

        switch (InternalState.Position)
        {
            case StatePosition.ParameterNavigator:
            case StatePosition.Macro:
            case StatePosition.Parameter:
            case StatePosition.ParameterAttribute:
            case StatePosition.ParameterNodes:
            case StatePosition.ParameterText:
            case StatePosition.Root:
                var other = nav as MacroNavigator;
                isSame = other != null && other._macro == _macro && InternalState.IsSamePosition(other.InternalState);
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        DebugReturn(isSame);
        return isSame;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the same position as the specified XPathNavigator.
    /// </summary>
    /// <param name="nav">The XPathNavigator positioned on the node that you want to move to. </param>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the same position as the specified XPathNavigator;
    ///     otherwise, false. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveTo(XPathNavigator nav)
    {
        DebugEnter("MoveTo");

        var other = nav as MacroNavigator;
        var succ = false;

        if (other != null && other._macro == _macro)
        {
            InternalState = other.InternalState.Clone();
            DebugState();
            succ = true;
        }

        DebugReturn(succ);
        return succ;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the first attribute of the current node.
    /// </summary>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the first attribute of the current node;
    ///     otherwise, false. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToFirstAttribute()
    {
        DebugEnter("MoveToFirstAttribute");
        bool succ;

        switch (InternalState.Position)
        {
            case StatePosition.ParameterNavigator:
                succ = InternalState.ParameterNavigator?.MoveToFirstAttribute() ?? false;
                break;
            case StatePosition.Parameter:
                if (_macro.Parameters[InternalState.ParameterIndex].Attributes != null)
                {
                    InternalState.Position = StatePosition.ParameterAttribute;
                    InternalState.ParameterAttributeIndex = 0;
                    succ = true;
                    DebugState();
                }
                else
                {
                    succ = false;
                }

                break;
            case StatePosition.ParameterAttribute:
            case StatePosition.ParameterNodes:
            case StatePosition.Macro:
            case StatePosition.ParameterText:
            case StatePosition.Root:
                succ = false;
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        DebugReturn(succ);
        return succ;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the first child node of the current node.
    /// </summary>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the first child node of the current node;
    ///     otherwise, false. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToFirstChild()
    {
        DebugEnter("MoveToFirstChild");
        bool succ;

        switch (InternalState.Position)
        {
            case StatePosition.Macro:
                if (_macro.Parameters.Length == 0)
                {
                    succ = false;
                }
                else
                {
                    InternalState.ParameterIndex = 0;
                    InternalState.Position = StatePosition.Parameter;
                    succ = true;
                }

                break;
            case StatePosition.Parameter:
                MacroParameter parameter = _macro.Parameters[InternalState.ParameterIndex];
                XPathNavigator? nav = parameter.NavigatorValue;
                if (parameter.WrapNavigatorInNodes)
                {
                    InternalState.Position = StatePosition.ParameterNodes;
                    DebugState();
                    succ = true;
                }
                else if (nav != null)
                {
                    nav = nav.Clone(); // never use the raw parameter's navigator
                    nav.MoveToFirstChild();
                    InternalState.ParameterNavigator = nav;
                    InternalState.ParameterNavigatorDepth = 0;
                    InternalState.Position = StatePosition.ParameterNavigator;
                    DebugState();
                    succ = true;
                }
                else
                {
                    var s = _macro.Parameters[InternalState.ParameterIndex].StringValue;
                    if (s != null)
                    {
                        InternalState.Position = StatePosition.ParameterText;
                        DebugState();
                        succ = true;
                    }
                    else
                    {
                        succ = false;
                    }
                }

                break;
            case StatePosition.ParameterNavigator:
                if (InternalState.ParameterNavigatorDepth ==
                    _macro.Parameters[InternalState.ParameterIndex].MaxNavigatorDepth)
                {
                    succ = false;
                }
                else
                {
                    // move to first doc child => increment depth, else (property child) do nothing
                    succ = InternalState.ParameterNavigator?.MoveToFirstChild() ?? false;
                    if (succ && IsDoc(InternalState.ParameterNavigator))
                    {
                        ++InternalState.ParameterNavigatorDepth;
                        DebugState();
                    }
                }

                break;
            case StatePosition.ParameterNodes:
                if (_macro.Parameters[InternalState.ParameterIndex].NavigatorValue != null)
                {
                    // never use the raw parameter's navigator
                    InternalState.ParameterNavigator =
                        _macro.Parameters[InternalState.ParameterIndex].NavigatorValue?.Clone();
                    InternalState.Position = StatePosition.ParameterNavigator;
                    succ = true;
                    DebugState();
                }
                else
                {
                    succ = false;
                }

                break;
            case StatePosition.ParameterAttribute:
            case StatePosition.ParameterText:
                succ = false;
                break;
            case StatePosition.Root:
                InternalState.Position = StatePosition.Macro;
                DebugState();
                succ = true;
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        DebugReturn(succ);
        return succ;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the first namespace node that matches the XPathNamespaceScope specified.
    /// </summary>
    /// <param name="namespaceScope">An XPathNamespaceScope value describing the namespace scope. </param>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the first namespace node;
    ///     otherwise, false. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
    {
        DebugEnter("MoveToFirstNamespace");
        DebugReturn(false);
        return false;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the next namespace node matching the XPathNamespaceScope specified.
    /// </summary>
    /// <param name="namespaceScope">An XPathNamespaceScope value describing the namespace scope. </param>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the next namespace node;
    ///     otherwise, false. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
    {
        DebugEnter("MoveToNextNamespace");
        DebugReturn(false);
        return false;
    }

    /// <summary>
    ///     Moves to the node that has an attribute of type ID whose value matches the specified String.
    /// </summary>
    /// <param name="id">A String representing the ID value of the node to which you want to move.</param>
    /// <returns>
    ///     true if the XPathNavigator is successful moving; otherwise, false.
    ///     If false, the position of the navigator is unchanged.
    /// </returns>
    public override bool MoveToId(string id)
    {
        DebugEnter("MoveToId");

        // impossible to implement since parameters can contain duplicate fragments of the
        // main xml and therefore there can be duplicate unique node identifiers.
        DebugReturn("NotImplementedException");
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Moves the XPathNavigator to the next sibling node of the current node.
    /// </summary>
    /// <returns>
    ///     true if the XPathNavigator is successful moving to the next sibling node;
    ///     otherwise, false if there are no more siblings or if the XPathNavigator is currently
    ///     positioned on an attribute node. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToNext()
    {
        DebugEnter("MoveToNext");
        bool succ;

        switch (InternalState.Position)
        {
            case StatePosition.Parameter:
                if (InternalState.ParameterIndex == _macro.Parameters.Length - 1)
                {
                    succ = false;
                }
                else
                {
                    ++InternalState.ParameterIndex;
                    DebugState();
                    succ = true;
                }

                break;
            case StatePosition.ParameterNavigator:
                var wasDoc = IsDoc(InternalState.ParameterNavigator);
                succ = InternalState.ParameterNavigator?.MoveToNext() ?? false;
                if (succ && !wasDoc && IsDoc(InternalState.ParameterNavigator))
                {
                    // move to first doc child => increment depth, else (another property child) do nothing
                    if (InternalState.ParameterNavigatorDepth ==
                        _macro.Parameters[InternalState.ParameterIndex].MaxNavigatorDepth)
                    {
                        InternalState.ParameterNavigator?.MoveToPrevious();
                        succ = false;
                    }
                    else
                    {
                        ++InternalState.ParameterNavigatorDepth;
                        DebugState();
                    }
                }

                break;
            case StatePosition.ParameterNodes:
            case StatePosition.ParameterAttribute:
            case StatePosition.ParameterText:
            case StatePosition.Macro:
            case StatePosition.Root:
                succ = false;
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        DebugReturn(succ);
        return succ;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the previous sibling node of the current node.
    /// </summary>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the previous sibling node;
    ///     otherwise, false if there is no previous sibling node or if the XPathNavigator is currently
    ///     positioned on an attribute node. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToPrevious()
    {
        DebugEnter("MoveToPrevious");
        bool succ;

        switch (InternalState.Position)
        {
            case StatePosition.Parameter:
                if (InternalState.ParameterIndex == -1)
                {
                    succ = false;
                }
                else
                {
                    --InternalState.ParameterIndex;
                    DebugState();
                    succ = true;
                }

                break;
            case StatePosition.ParameterNavigator:
                var wasDoc = IsDoc(InternalState.ParameterNavigator);
                succ = InternalState.ParameterNavigator?.MoveToPrevious() ?? false;
                if (succ && wasDoc && !IsDoc(InternalState.ParameterNavigator))
                {
                    // move from doc child back to property child => decrement depth
                    --InternalState.ParameterNavigatorDepth;
                    DebugState();
                }

                break;
            case StatePosition.ParameterAttribute:
            case StatePosition.ParameterNodes:
            case StatePosition.ParameterText:
            case StatePosition.Macro:
            case StatePosition.Root:
                succ = false;
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        DebugReturn(succ);
        return succ;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the next attribute.
    /// </summary>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the next attribute;
    ///     false if there are no more attributes. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToNextAttribute()
    {
        DebugEnter("MoveToNextAttribute");
        bool succ;

        switch (InternalState.Position)
        {
            case StatePosition.ParameterNavigator:
                succ = InternalState.ParameterNavigator?.MoveToNextAttribute() ?? false;
                break;
            case StatePosition.ParameterAttribute:
                if (InternalState.ParameterAttributeIndex ==
                    _macro.Parameters[InternalState.ParameterIndex].Attributes?.Length - 1)
                {
                    succ = false;
                }
                else
                {
                    ++InternalState.ParameterAttributeIndex;
                    DebugState();
                    succ = true;
                }

                break;
            case StatePosition.Parameter:
            case StatePosition.ParameterNodes:
            case StatePosition.ParameterText:
            case StatePosition.Macro:
            case StatePosition.Root:
                succ = false;
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        DebugReturn(succ);
        return succ;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the parent node of the current node.
    /// </summary>
    /// <returns>
    ///     Returns true if the XPathNavigator is successful moving to the parent node of the current node;
    ///     otherwise, false. If false, the position of the XPathNavigator is unchanged.
    /// </returns>
    public override bool MoveToParent()
    {
        DebugEnter("MoveToParent");
        bool succ;

        switch (InternalState.Position)
        {
            case StatePosition.Macro:
                InternalState.Position = StatePosition.Root;
                DebugState();
                succ = true;
                break;
            case StatePosition.Parameter:
                InternalState.Position = StatePosition.Macro;
                DebugState();
                succ = true;
                break;
            case StatePosition.ParameterAttribute:
            case StatePosition.ParameterNodes:
                InternalState.Position = StatePosition.Parameter;
                DebugState();
                succ = true;
                break;
            case StatePosition.ParameterNavigator:
                var wasDoc = IsDoc(InternalState.ParameterNavigator);
                succ = InternalState.ParameterNavigator?.MoveToParent() ?? false;
                if (succ)
                {
                    // move from doc child => decrement depth
                    if (wasDoc && --InternalState.ParameterNavigatorDepth == 0)
                    {
                        InternalState.Position = StatePosition.Parameter;
                        InternalState.ParameterNavigator = null;
                        DebugState();
                    }
                }

                break;
            case StatePosition.ParameterText:
                InternalState.Position = StatePosition.Parameter;
                DebugState();
                succ = true;
                break;
            case StatePosition.Root:
                succ = false;
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        DebugReturn(succ);
        return succ;
    }

    /// <summary>
    ///     Moves the XPathNavigator to the root node that the current node belongs to.
    /// </summary>
    public override void MoveToRoot()
    {
        DebugEnter("MoveToRoot");

        switch (InternalState.Position)
        {
            case StatePosition.ParameterNavigator:
                InternalState.ParameterNavigator = null;
                InternalState.ParameterNavigatorDepth = -1;
                break;
            case StatePosition.Parameter:
            case StatePosition.ParameterText:
                InternalState.ParameterIndex = -1;
                break;
            case StatePosition.ParameterAttribute:
            case StatePosition.ParameterNodes:
            case StatePosition.Macro:
            case StatePosition.Root:
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        InternalState.Position = StatePosition.Root;
        DebugState();

        DebugReturn();
    }

    private static bool IsDoc(XPathNavigator? nav)
    {
        if (nav is null)
        {
            return false;
        }

        if (nav.NodeType != XPathNodeType.Element)
        {
            return false;
        }

        XPathNavigator clone = nav.Clone();
        if (!clone.MoveToFirstAttribute())
        {
            return false;
        }

        do
        {
            if (clone.Name == "isDoc")
            {
                return true;
            }
        }
        while (clone.MoveToNextAttribute());

        return false;
    }

    #region Constructor

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroNavigator" /> class with macro parameters.
    /// </summary>
    /// <param name="parameters">The macro parameters.</param>
    public MacroNavigator(IEnumerable<MacroParameter> parameters)
        : this(new MacroRoot(parameters), new NameTable(), new State())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroNavigator" /> class with a macro node,
    ///     a name table and a state.
    /// </summary>
    /// <param name="macro">The macro node.</param>
    /// <param name="nameTable">The name table.</param>
    /// <param name="state">The state.</param>
    /// <remarks>Privately used for cloning a navigator.</remarks>
    private MacroNavigator(MacroRoot macro, XmlNameTable nameTable, State state)
    {
        _macro = macro;
        _nameTable = nameTable;
        InternalState = state;
    }

    #endregion

    #region Diagnostics

    // diagnostics code will not be compiled nor called into Release configuration.
    // in Debug configuration, uncomment lines in Debug() to write to console or to log.
    //
    // much of this code is duplicated in each navigator due to conditional compilation
#if DEBUG
    private const string Tabs = "                    ";
    private int _tabs;
    private readonly int _uid = GetUid();
    private static int _uidg;
    private static readonly object Uidl = new();

    private static int GetUid()
    {
        lock (Uidl)
        {
            return _uidg++;
        }
    }
#endif

    [Conditional("DEBUG")]
    private void DebugEnter(string name)
    {
#if DEBUG
        Debug(string.Empty);
        DebugState(":");
        Debug(name);
        _tabs = Math.Min(Tabs.Length, _tabs + 2);
#endif
    }

    [Conditional("DEBUG")]
    private void DebugCreate(MacroNavigator nav)
    {
#if DEBUG
        Debug("Create: [MacroNavigator::{0}]", nav._uid);
#endif
    }

    [Conditional("DEBUG")]
    private void DebugReturn()
    {
#if DEBUG
// ReSharper disable IntroduceOptionalParameters.Local
        DebugReturn("(void)");

// ReSharper restore IntroduceOptionalParameters.Local
#endif
    }

    [Conditional("DEBUG")]
    private void DebugReturn(bool value)
    {
#if DEBUG
        DebugReturn(value ? "true" : "false");
#endif
    }

    [Conditional("DEBUG")]
    private void DebugReturn(string format, params object[] args)
    {
#if DEBUG
        Debug("=> " + format, args);
        if (_tabs > 0)
        {
            _tabs -= 2;
        }
#endif
    }

    [Conditional("DEBUG")]
    private void DebugState(string s = " =>")
    {
#if DEBUG
        string position;

        switch (InternalState.Position)
        {
            case StatePosition.Macro:
                position = "At macro.";
                break;
            case StatePosition.Parameter:
                position = string.Format(
                    "At parameter '{0}'.",
                    _macro.Parameters[InternalState.ParameterIndex].Name);
                break;
            case StatePosition.ParameterAttribute:
                position = string.Format(
                    "At parameter attribute '{0}/{1}'.",
                    _macro.Parameters[InternalState.ParameterIndex].Name,
                    _macro.Parameters[InternalState.ParameterIndex].Attributes?[InternalState.ParameterAttributeIndex].Key);
                break;
            case StatePosition.ParameterNavigator:
                position = string.Format(
                    "In parameter '{0}{1}' navigator.",
                    _macro.Parameters[InternalState.ParameterIndex].Name,
                    _macro.Parameters[InternalState.ParameterIndex].WrapNavigatorInNodes ? "/nodes" : string.Empty);
                break;
            case StatePosition.ParameterNodes:

                position = string.Format("At parameter '{0}/nodes'.", _macro.Parameters[InternalState.ParameterIndex].Name);
                break;
            case StatePosition.ParameterText:
                position = string.Format("In parameter '{0}' text.", _macro.Parameters[InternalState.ParameterIndex].Name);
                break;
            case StatePosition.Root:
                position = "At root.";
                break;
            default:
                throw new InvalidOperationException("Invalid position.");
        }

        Debug("State{0} {1}", s, position);
#endif
    }

#if DEBUG
    private void Debug(string format, params object[] args)
    {
        // remove comments to write
        format = "[" + _uid.ToString("00000") + "] " + Tabs.Substring(0, _tabs) + format;
#pragma warning disable 168
        var msg = string.Format(format, args); // unused if not writing, hence #pragma
#pragma warning restore 168
    }
#endif

    #endregion

    #region Macro

    private class MacroRoot
    {
        public MacroRoot(IEnumerable<MacroParameter> parameters) =>
            Parameters = parameters == null ? new MacroParameter[] { } : parameters.ToArray();

        public MacroParameter[] Parameters { get; }
    }

    public class MacroParameter
    {
        // note: assuming we're not thinking about supporting
        // XPathIterator in parameters - enough nonsense!
        public MacroParameter(string name, string value)
        {
            Name = name;
            StringValue = value;
        }

        public MacroParameter(
            string name,
            XPathNavigator navigator,
            int maxNavigatorDepth = int.MaxValue,
            bool wrapNavigatorInNodes = false,
            IEnumerable<KeyValuePair<string, string>>? attributes = null)
        {
            Name = name;
            MaxNavigatorDepth = maxNavigatorDepth;
            WrapNavigatorInNodes = wrapNavigatorInNodes;
            if (attributes != null)
            {
                KeyValuePair<string, string>[] a = attributes.ToArray();
                if (a.Length > 0)
                {
                    Attributes = a;
                }
            }

            NavigatorValue = navigator; // should not be empty
        }

        public string Name { get; }

        public string? StringValue { get; }

        public XPathNavigator? NavigatorValue { get; }

        public int MaxNavigatorDepth { get; }

        public bool WrapNavigatorInNodes { get; }

        public KeyValuePair<string, string>[]? Attributes { get; }
    }

    #endregion

    #region State management

    // the possible state positions
    internal enum StatePosition
    {
        Root,
        Macro,
        Parameter,
        ParameterAttribute,
        ParameterText,
        ParameterNodes,
        ParameterNavigator,
    }

    // gets the state
    // for unit tests only
    internal State InternalState { get; private set; }

    // represents the XPathNavigator state
    internal class State
    {
        // initialize a new state
        private State(StatePosition position)
        {
            Position = position;
            ParameterIndex = 0;
            ParameterNavigatorDepth = 0;
            ParameterAttributeIndex = 0;
        }

        // initialize a new state
        // used for creating the very first state
        public State()
            : this(StatePosition.Root)
        {
        }

        // initialize a clone state
        private State(State other)
        {
            Position = other.Position;

            ParameterIndex = other.ParameterIndex;

            if (Position == StatePosition.ParameterNavigator)
            {
                ParameterNavigator = other.ParameterNavigator?.Clone();
                ParameterNavigatorDepth = other.ParameterNavigatorDepth;
                ParameterAttributeIndex = other.ParameterAttributeIndex;
            }
        }

        public StatePosition Position { get; set; }

        // the index of the current element
        public int ParameterIndex { get; set; }

        // the current depth within the element navigator
        public int ParameterNavigatorDepth { get; set; }

        // the index of the current element's attribute
        public int ParameterAttributeIndex { get; set; }

        // gets or sets the element navigator
        public XPathNavigator? ParameterNavigator { get; set; }

        public State Clone() => new State(this);

        // gets a value indicating whether this state is at the same position as another one.
        public bool IsSamePosition(State other)
        {
            if (other.ParameterNavigator is null || ParameterNavigator is null)
            {
                return false;
            }

            return other.Position == Position
                   && (Position != StatePosition.ParameterNavigator ||
                       other.ParameterNavigator.IsSamePosition(ParameterNavigator))
                   && other.ParameterIndex == ParameterIndex
                   && other.ParameterAttributeIndex == ParameterAttributeIndex;
        }
    }

    #endregion
}
