using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;

namespace Umbraco.Cms.Core.Xml.XPath
{
    /// <summary>
    /// Provides a cursor model for navigating {macro /} as if it were XML.
    /// </summary>
    public class MacroNavigator : XPathNavigator
    {
        private readonly XmlNameTable _nameTable;
        private readonly MacroRoot _macro;
        private State _state;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MacroNavigator"/> class with macro parameters.
        /// </summary>
        /// <param name="parameters">The macro parameters.</param>
        public MacroNavigator(IEnumerable<MacroParameter> parameters)
            : this(new MacroRoot(parameters), new NameTable(), new State())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MacroNavigator"/> class with a macro node,
        /// a name table and a state.
        /// </summary>
        /// <param name="macro">The macro node.</param>
        /// <param name="nameTable">The name table.</param>
        /// <param name="state">The state.</param>
        /// <remarks>Privately used for cloning a navigator.</remarks>
        private MacroNavigator(MacroRoot macro, XmlNameTable nameTable, State state)
        {
            _macro = macro;
            _nameTable = nameTable;
            _state = state;
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
        private static readonly object Uidl = new object();
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

            switch (_state.Position)
            {
                case StatePosition.Macro:
                    position = "At macro.";
                    break;
                case StatePosition.Parameter:
                    position = string.Format("At parameter '{0}'.", _macro.Parameters[_state.ParameterIndex].Name);
                    break;
                case StatePosition.ParameterAttribute:
                    position = string.Format(
                        "At parameter attribute '{0}/{1}'.",
                        _macro.Parameters[_state.ParameterIndex].Name,
                        _macro.Parameters[_state.ParameterIndex].Attributes?[_state.ParameterAttributeIndex].Key);
                    break;
                case StatePosition.ParameterNavigator:
                    position = string.Format(
                        "In parameter '{0}{1}' navigator.",
                        _macro.Parameters[_state.ParameterIndex].Name,
                        _macro.Parameters[_state.ParameterIndex].WrapNavigatorInNodes ? "/nodes" : string.Empty);
                    break;
                case StatePosition.ParameterNodes:
                    position = string.Format(
                        "At parameter '{0}/nodes'.",
                        _macro.Parameters[_state.ParameterIndex].Name);
                    break;
                case StatePosition.ParameterText:
                    position = string.Format(
                        "In parameter '{0}' text.",
                        _macro.Parameters[_state.ParameterIndex].Name);
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
            public MacroRoot(IEnumerable<MacroParameter> parameters)
            {
                Parameters = parameters == null ? new MacroParameter[] {} : parameters.ToArray();
            }

            public MacroParameter[] Parameters { get; private set; }
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

            public string Name { get; private set; }
            public string? StringValue { get; private set; }
            public XPathNavigator? NavigatorValue { get; private set; }
            public int MaxNavigatorDepth { get; private set; }
            public bool WrapNavigatorInNodes { get; private set; }
            public KeyValuePair<string, string>[]? Attributes { get; private set; }
        }

        #endregion

        /// <summary>
        /// Creates a new XPathNavigator positioned at the same node as this XPathNavigator.
        /// </summary>
        /// <returns>A new XPathNavigator positioned at the same node as this XPathNavigator.</returns>
        public override XPathNavigator Clone()
        {
            DebugEnter("Clone");
            var nav = new MacroNavigator(_macro, _nameTable, _state.Clone());
            DebugCreate(nav);
            DebugReturn("[XPathNavigator]");
            return nav;
        }

        /// <summary>
        /// Gets a value indicating whether the current node is an empty element without an end element tag.
        /// </summary>
        public override bool IsEmptyElement
        {
            get
            {
                DebugEnter("IsEmptyElement");
                bool isEmpty;

                switch (_state.Position)
                {
                    case StatePosition.Macro:
                        isEmpty = _macro.Parameters.Length == 0;
                        break;
                    case StatePosition.Parameter:
                        MacroParameter parameter = _macro.Parameters[_state.ParameterIndex];
                        XPathNavigator? nav = parameter.NavigatorValue;
                        if (parameter.WrapNavigatorInNodes || nav != null)
                        {
                            isEmpty = false;
                        }
                        else
                        {
                            var s = _macro.Parameters[_state.ParameterIndex].StringValue;
                            isEmpty = s == null;
                        }

                        break;
                    case StatePosition.ParameterNavigator:
                        isEmpty = _state.ParameterNavigator?.IsEmptyElement ?? true;
                        break;
                    case StatePosition.ParameterNodes:
                        isEmpty = _macro.Parameters[_state.ParameterIndex].NavigatorValue == null;
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
        /// Determines whether the current XPathNavigator is at the same position as the specified XPathNavigator.
        /// </summary>
        /// <param name="nav">The XPathNavigator to compare to this XPathNavigator.</param>
        /// <returns>true if the two XPathNavigator objects have the same position; otherwise, false.</returns>
        public override bool IsSamePosition(XPathNavigator nav)
        {
            DebugEnter("IsSamePosition");
            bool isSame;

            switch (_state.Position)
            {
                case StatePosition.ParameterNavigator:
                case StatePosition.Macro:
                case StatePosition.Parameter:
                case StatePosition.ParameterAttribute:
                case StatePosition.ParameterNodes:
                case StatePosition.ParameterText:
                case StatePosition.Root:
                    var other = nav as MacroNavigator;
                    isSame = other != null && other._macro == _macro && _state.IsSamePosition(other._state);
                    break;
                default:
                    throw new InvalidOperationException("Invalid position.");
            }

            DebugReturn(isSame);
            return isSame;
        }

        /// <summary>
        /// Gets the qualified name of the current node.
        /// </summary>
        public override string Name
        {
            get
            {
                DebugEnter("Name");
                string name;

                switch (_state.Position)
                {
                    case StatePosition.Macro:
                        name = "macro";
                        break;
                    case StatePosition.Parameter:
                        name = _macro.Parameters[_state.ParameterIndex].Name;
                        break;
                    case StatePosition.ParameterAttribute:
                        name = _macro.Parameters[_state.ParameterIndex].Attributes?[_state.ParameterAttributeIndex].Key ?? string.Empty;
                        break;
                    case StatePosition.ParameterNavigator:
                        name = _state.ParameterNavigator?.Name ?? string.Empty;
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
        /// Gets the Name of the current node without any namespace prefix.
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
        /// Moves the XPathNavigator to the same position as the specified XPathNavigator.
        /// </summary>
        /// <param name="nav">The XPathNavigator positioned on the node that you want to move to. </param>
        /// <returns>Returns true if the XPathNavigator is successful moving to the same position as the specified XPathNavigator;
        /// otherwise, false. If false, the position of the XPathNavigator is unchanged.</returns>
        public override bool MoveTo(XPathNavigator nav)
        {
            DebugEnter("MoveTo");

            var other = nav as MacroNavigator;
            var succ = false;

            if (other != null && other._macro == _macro)
            {
                _state = other._state.Clone();
                DebugState();
                succ = true;
            }

            DebugReturn(succ);
            return succ;
        }

        /// <summary>
        /// Moves the XPathNavigator to the first attribute of the current node.
        /// </summary>
        /// <returns>Returns true if the XPathNavigator is successful moving to the first attribute of the current node;
        /// otherwise, false. If false, the position of the XPathNavigator is unchanged.</returns>
        public override bool MoveToFirstAttribute()
        {
            DebugEnter("MoveToFirstAttribute");
            bool succ;

            switch (_state.Position)
            {
                case StatePosition.ParameterNavigator:
                    succ = _state.ParameterNavigator?.MoveToFirstAttribute() ?? false;
                    break;
                case StatePosition.Parameter:
                    if (_macro.Parameters[_state.ParameterIndex].Attributes != null)
                    {
                        _state.Position = StatePosition.ParameterAttribute;
                        _state.ParameterAttributeIndex = 0;
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
        /// Moves the XPathNavigator to the first child node of the current node.
        /// </summary>
        /// <returns>Returns true if the XPathNavigator is successful moving to the first child node of the current node;
        /// otherwise, false. If false, the position of the XPathNavigator is unchanged.</returns>
        public override bool MoveToFirstChild()
        {
            DebugEnter("MoveToFirstChild");
            bool succ;

            switch (_state.Position)
            {
                case StatePosition.Macro:
                    if (_macro.Parameters.Length == 0)
                    {
                        succ = false;
                    }
                    else
                    {
                        _state.ParameterIndex = 0;
                        _state.Position = StatePosition.Parameter;
                        succ = true;
                    }
                    break;
                case StatePosition.Parameter:
                    MacroParameter parameter = _macro.Parameters[_state.ParameterIndex];
                    XPathNavigator? nav = parameter.NavigatorValue;
                    if (parameter.WrapNavigatorInNodes)
                    {
                        _state.Position = StatePosition.ParameterNodes;
                        DebugState();
                        succ = true;
                    }
                    else if (nav != null)
                    {
                        nav = nav.Clone(); // never use the raw parameter's navigator
                        nav.MoveToFirstChild();
                        _state.ParameterNavigator = nav;
                        _state.ParameterNavigatorDepth = 0;
                        _state.Position = StatePosition.ParameterNavigator;
                        DebugState();
                        succ = true;
                    }
                    else
                    {
                        var s = _macro.Parameters[_state.ParameterIndex].StringValue;
                        if (s != null)
                        {
                            _state.Position = StatePosition.ParameterText;
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
                    if (_state.ParameterNavigatorDepth == _macro.Parameters[_state.ParameterIndex].MaxNavigatorDepth)
                    {
                        succ = false;
                    }
                    else
                    {
                        // move to first doc child => increment depth, else (property child) do nothing
                        succ = _state.ParameterNavigator?.MoveToFirstChild() ?? false;
                        if (succ && IsDoc(_state.ParameterNavigator))
                        {
                            ++_state.ParameterNavigatorDepth;
                            DebugState();
                        }
                    }
                    break;
                case StatePosition.ParameterNodes:
                    if (_macro.Parameters[_state.ParameterIndex].NavigatorValue != null)
                    {
                        // never use the raw parameter's navigator
                        _state.ParameterNavigator = _macro.Parameters[_state.ParameterIndex].NavigatorValue?.Clone();
                        _state.Position = StatePosition.ParameterNavigator;
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
                    _state.Position = StatePosition.Macro;
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
        /// Moves the XPathNavigator to the first namespace node that matches the XPathNamespaceScope specified.
        /// </summary>
        /// <param name="namespaceScope">An XPathNamespaceScope value describing the namespace scope. </param>
        /// <returns>Returns true if the XPathNavigator is successful moving to the first namespace node;
        /// otherwise, false. If false, the position of the XPathNavigator is unchanged.</returns>
        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            DebugEnter("MoveToFirstNamespace");
            DebugReturn(false);
            return false;
        }

        /// <summary>
        /// Moves the XPathNavigator to the next namespace node matching the XPathNamespaceScope specified.
        /// </summary>
        /// <param name="namespaceScope">An XPathNamespaceScope value describing the namespace scope. </param>
        /// <returns>Returns true if the XPathNavigator is successful moving to the next namespace node;
        /// otherwise, false. If false, the position of the XPathNavigator is unchanged.</returns>
        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            DebugEnter("MoveToNextNamespace");
            DebugReturn(false);
            return false;
        }

        /// <summary>
        /// Moves to the node that has an attribute of type ID whose value matches the specified String.
        /// </summary>
        /// <param name="id">A String representing the ID value of the node to which you want to move.</param>
        /// <returns>true if the XPathNavigator is successful moving; otherwise, false.
        /// If false, the position of the navigator is unchanged.</returns>
        public override bool MoveToId(string id)
        {
            DebugEnter("MoveToId");
            // impossible to implement since parameters can contain duplicate fragments of the
            // main xml and therefore there can be duplicate unique node identifiers.
            DebugReturn("NotImplementedException");
            throw new NotImplementedException();
        }

        /// <summary>
        /// Moves the XPathNavigator to the next sibling node of the current node.
        /// </summary>
        /// <returns>true if the XPathNavigator is successful moving to the next sibling node;
        /// otherwise, false if there are no more siblings or if the XPathNavigator is currently
        /// positioned on an attribute node. If false, the position of the XPathNavigator is unchanged.</returns>
        public override bool MoveToNext()
        {
            DebugEnter("MoveToNext");
            bool succ;

            switch (_state.Position)
            {
                case StatePosition.Parameter:
                    if (_state.ParameterIndex == _macro.Parameters.Length - 1)
                    {
                        succ = false;
                    }
                    else
                    {
                        ++_state.ParameterIndex;
                        DebugState();
                        succ = true;
                    }
                    break;
                case StatePosition.ParameterNavigator:
                    var wasDoc = IsDoc(_state.ParameterNavigator);
                    succ = _state.ParameterNavigator?.MoveToNext() ?? false;
                    if (succ && !wasDoc && IsDoc(_state.ParameterNavigator))
                    {
                        // move to first doc child => increment depth, else (another property child) do nothing
                        if (_state.ParameterNavigatorDepth == _macro.Parameters[_state.ParameterIndex].MaxNavigatorDepth)
                        {
                            _state.ParameterNavigator?.MoveToPrevious();
                            succ = false;
                        }
                        else
                        {
                            ++_state.ParameterNavigatorDepth;
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
        /// Moves the XPathNavigator to the previous sibling node of the current node.
        /// </summary>
        /// <returns>Returns true if the XPathNavigator is successful moving to the previous sibling node;
        /// otherwise, false if there is no previous sibling node or if the XPathNavigator is currently
        /// positioned on an attribute node. If false, the position of the XPathNavigator is unchanged.</returns>
        public override bool MoveToPrevious()
        {
            DebugEnter("MoveToPrevious");
            bool succ;

            switch (_state.Position)
            {
                case StatePosition.Parameter:
                    if (_state.ParameterIndex == -1)
                    {
                        succ = false;
                    }
                    else
                    {
                        --_state.ParameterIndex;
                        DebugState();
                        succ = true;
                    }
                    break;
                case StatePosition.ParameterNavigator:
                    var wasDoc = IsDoc(_state.ParameterNavigator);
                    succ = _state.ParameterNavigator?.MoveToPrevious() ?? false;
                    if (succ && wasDoc && !IsDoc(_state.ParameterNavigator))
                    {
                        // move from doc child back to property child => decrement depth
                        --_state.ParameterNavigatorDepth;
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
        /// Moves the XPathNavigator to the next attribute.
        /// </summary>
        /// <returns>Returns true if the XPathNavigator is successful moving to the next attribute;
        /// false if there are no more attributes. If false, the position of the XPathNavigator is unchanged.</returns>
        public override bool MoveToNextAttribute()
        {
            DebugEnter("MoveToNextAttribute");
            bool succ;

            switch (_state.Position)
            {
                case StatePosition.ParameterNavigator:
                    succ = _state.ParameterNavigator?.MoveToNextAttribute() ?? false;
                    break;
                case StatePosition.ParameterAttribute:
                    if (_state.ParameterAttributeIndex == _macro.Parameters[_state.ParameterIndex].Attributes?.Length - 1)
                    {
                        succ = false;
                    }
                    else
                    {
                        ++_state.ParameterAttributeIndex;
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
        /// Moves the XPathNavigator to the parent node of the current node.
        /// </summary>
        /// <returns>Returns true if the XPathNavigator is successful moving to the parent node of the current node;
        /// otherwise, false. If false, the position of the XPathNavigator is unchanged.</returns>
        public override bool MoveToParent()
        {
            DebugEnter("MoveToParent");
            bool succ;

            switch (_state.Position)
            {
                case StatePosition.Macro:
                    _state.Position = StatePosition.Root;
                    DebugState();
                    succ = true;
                    break;
                case StatePosition.Parameter:
                    _state.Position = StatePosition.Macro;
                    DebugState();
                    succ = true;
                    break;
                case StatePosition.ParameterAttribute:
                case StatePosition.ParameterNodes:
                    _state.Position = StatePosition.Parameter;
                    DebugState();
                    succ = true;
                    break;
                case StatePosition.ParameterNavigator:
                    var wasDoc = IsDoc(_state.ParameterNavigator);
                    succ = _state.ParameterNavigator?.MoveToParent() ?? false;
                    if (succ)
                    {
                        // move from doc child => decrement depth
                        if (wasDoc && --_state.ParameterNavigatorDepth == 0)
                        {
                            _state.Position = StatePosition.Parameter;
                            _state.ParameterNavigator = null;
                            DebugState();
                        }
                    }
                    break;
                case StatePosition.ParameterText:
                    _state.Position = StatePosition.Parameter;
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
        /// Moves the XPathNavigator to the root node that the current node belongs to.
        /// </summary>
        public override void MoveToRoot()
        {
            DebugEnter("MoveToRoot");

            switch (_state.Position)
            {
                case StatePosition.ParameterNavigator:
                    _state.ParameterNavigator = null;
                    _state.ParameterNavigatorDepth = -1;
                    break;
                case StatePosition.Parameter:
                case StatePosition.ParameterText:
                    _state.ParameterIndex = -1;
                    break;
                case StatePosition.ParameterAttribute:
                case StatePosition.ParameterNodes:
                case StatePosition.Macro:
                case StatePosition.Root:
                    break;
                default:
                    throw new InvalidOperationException("Invalid position.");
            }

            _state.Position = StatePosition.Root;
            DebugState();

            DebugReturn();
        }

        /// <summary>
        /// Gets the base URI for the current node.
        /// </summary>
        public override string BaseURI
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the XmlNameTable of the XPathNavigator.
        /// </summary>
        public override XmlNameTable NameTable
        {
            get { return _nameTable; }
        }

        /// <summary>
        /// Gets the namespace URI of the current node.
        /// </summary>
        public override string NamespaceURI
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the XPathNodeType of the current node.
        /// </summary>
        public override XPathNodeType NodeType
        {
            get
            {
                DebugEnter("NodeType");
                XPathNodeType type;

                switch (_state.Position)
                {
                    case StatePosition.Macro:
                    case StatePosition.Parameter:
                    case StatePosition.ParameterNodes:
                        type = XPathNodeType.Element;
                        break;
                    case StatePosition.ParameterNavigator:
                        type = _state.ParameterNavigator?.NodeType ?? XPathNodeType.Root;
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
        /// Gets the namespace prefix associated with the current node.
        /// </summary>
        public override string Prefix
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the string value of the item.
        /// </summary>
        /// <remarks>Does not fully behave as per the specs, as we report empty value on root and macro elements, and we start
        /// reporting values only on parameter elements. This is because, otherwise, we would might dump the whole database
        /// and it probably does not make sense at Umbraco level.</remarks>
        public override string Value
        {
            get
            {
                DebugEnter("Value");
                string value;

                XPathNavigator? nav;
                switch (_state.Position)
                {
                    case StatePosition.Parameter:
                        nav = _macro.Parameters[_state.ParameterIndex].NavigatorValue;
                        if (nav != null)
                        {
                            nav = nav.Clone(); // never use the raw parameter's navigator
                            nav.MoveToFirstChild();
                            value = nav.Value;
                        }
                        else
                        {
                            var s = _macro.Parameters[_state.ParameterIndex].StringValue;
                            value = s ?? string.Empty;
                        }
                        break;
                    case StatePosition.ParameterAttribute:
                        value = _macro.Parameters[_state.ParameterIndex].Attributes?[_state.ParameterAttributeIndex].Value ?? string.Empty;
                        break;
                    case StatePosition.ParameterNavigator:
                        value = _state.ParameterNavigator?.Value ?? string.Empty;
                        break;
                    case StatePosition.ParameterNodes:
                        nav = _macro.Parameters[_state.ParameterIndex].NavigatorValue;
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
                        value = _macro.Parameters[_state.ParameterIndex].StringValue ?? string.Empty;
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
            ParameterNavigator
        }

        // gets the state
        // for unit tests only
        internal State InternalState { get { return _state; } }

        // represents the XPathNavigator state
        internal class State
        {
            public StatePosition Position { get; set; }

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
            { }

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

            public State Clone()
            {
                return new State(this);
            }

            // the index of the current element
            public int ParameterIndex { get; set; }

            // the current depth within the element navigator
            public int ParameterNavigatorDepth { get; set; }

            // the index of the current element's attribute
            public int ParameterAttributeIndex { get; set; }

            // gets or sets the element navigator
            public XPathNavigator? ParameterNavigator { get; set; }

            // gets a value indicating whether this state is at the same position as another one.
            public bool IsSamePosition(State other)
            {
                if (other.ParameterNavigator is null || ParameterNavigator is null)
                {
                    return false;
                }
                return other.Position == Position
                    && (Position != StatePosition.ParameterNavigator || other.ParameterNavigator.IsSamePosition(ParameterNavigator))
                    && other.ParameterIndex == ParameterIndex
                    && other.ParameterAttributeIndex == ParameterAttributeIndex;
            }
        }

        #endregion
    }
}
