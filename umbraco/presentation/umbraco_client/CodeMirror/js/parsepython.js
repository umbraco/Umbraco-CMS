var PythonParser = Editor.Parser = (function() {
    function wordRegexp(words) {
        return new RegExp("^(?:" + words.join("|") + ")$");
    }
    var DELIMITERCLASS = 'py-delimiter';
    var LITERALCLASS = 'py-literal';
    var ERRORCLASS = 'py-error';
    var OPERATORCLASS = 'py-operator';
    var IDENTIFIERCLASS = 'py-identifier';
    var STRINGCLASS = 'py-string';
    var BYTESCLASS = 'py-bytes';
    var UNICODECLASS = 'py-unicode';
    var RAWCLASS = 'py-raw';
    var NORMALCONTEXT = 'normal';
    var STRINGCONTEXT = 'string';
    var singleOperators = '+-*/%&|^~<>';
    var doubleOperators = wordRegexp(['==', '!=', '\\<=', '\\>=', '\\<\\>',
                                      '\\<\\<', '\\>\\>', '\\/\\/', '\\*\\*']);
    var singleDelimiters = '()[]{}@,:.`=;';
    var doubleDelimiters = ['\\+=', '\\-=', '\\*=', '/=', '%=', '&=', '\\|=',
                            '\\^='];
    var tripleDelimiters = wordRegexp(['//=','\\>\\>=','\\<\\<=','\\*\\*=']);
    var singleStarters = singleOperators + singleDelimiters + '=!';
    var doubleStarters = '=<>*/';
    var identifierStarters = /[_A-Za-z]/;

    var wordOperators = wordRegexp(['and', 'or', 'not', 'is', 'in']);
    var commonkeywords = ['as', 'assert', 'break', 'class', 'continue',
                          'def', 'del', 'elif', 'else', 'except', 'finally',
                          'for', 'from', 'global', 'if', 'import',
                          'lambda', 'pass', 'raise', 'return',
                          'try', 'while', 'with', 'yield'];
    var commontypes = ['bool', 'classmethod', 'complex', 'dict', 'enumerate',
                       'float', 'frozenset', 'int', 'list', 'object',
                       'property', 'reversed', 'set', 'slice', 'staticmethod',
                       'str', 'super', 'tuple', 'type'];
    var py2 = {'types': ['basestring', 'buffer', 'file', 'long', 'unicode',
                         'xrange'],
               'keywords': ['exec', 'print'],
               'version': 2 };
    var py3 = {'types': ['bytearray', 'bytes', 'filter', 'map', 'memoryview',
                         'open', 'range', 'zip'],
               'keywords': ['nonlocal'],
               'version': 3};

    var py, keywords, types, stringStarters, stringTypes, config;

    function configure(conf) {
        if (!conf.hasOwnProperty('pythonVersion')) {
            conf.pythonVersion = 2;
        }
        if (!conf.hasOwnProperty('strictErrors')) {
            conf.strictErrors = true;
        }
        if (conf.pythonVersion != 2 && conf.pythonVersion != 3) {
            alert('CodeMirror: Unknown Python Version "' +
                  conf.pythonVersion +
                  '", defaulting to Python 2.x.');
            conf.pythonVersion = 2;
        }
        if (conf.pythonVersion == 3) {
            py = py3;
            stringStarters = /[\'\"rbRB]/;
            stringTypes = /[rb]/;
            doubleDelimiters.push('\\-\\>');
        } else {
            py = py2;
            stringStarters = /['"RUru]/;
            stringTypes = /[ru]/;
        }
        config = conf;
        keywords = wordRegexp(commonkeywords.concat(py.keywords));
        types = wordRegexp(commontypes.concat(py.types));
        doubleDelimiters = wordRegexp(doubleDelimiters);
    }

    var tokenizePython = (function() {
        function normal(source, setState) {
            var stringDelim, threeStr, temp, type, word, possible = {};
            var ch = source.next();
            
            function filterPossible(token, styleIfPossible) {
                if (!possible.style && !possible.content) {
                    return token;
                } else if (typeof(token) == STRINGCONTEXT) {
                    token = {content: source.get(), style: token};
                }
                if (possible.style || styleIfPossible) {
                    token.style = styleIfPossible ? styleIfPossible : possible.style;
                }
                if (possible.content) {
                    token.content = possible.content + token.content;
                }
                possible = {};
                return token;
            }

            // Handle comments
            if (ch == '#') {
                while (!source.endOfLine()) {
                    source.next();
                }
                return 'py-comment';
            }
            // Handle special chars
            if (ch == '\\') {
                if (source.peek() != '\n') {
                    var whitespace = true;
                    while (!source.endOfLine()) {
                        if(!(/\s/.test(source.next()))) {
                            whitespace = false;
                        }
                    }
                    if (!whitespace) {
                        return ERRORCLASS;
                    }
                }
                return 'py-special';
            }
            // Handle operators and delimiters
            if (singleStarters.indexOf(ch) != -1) {
                if (doubleStarters.indexOf(source.peek()) != -1) {
                    temp = ch + source.peek();
                    // It must be a double delimiter or operator or triple delimiter
                    if (doubleOperators.test(temp)) {
                        source.next();
                        if (tripleDelimiters.test(temp + source.peek())) {
                            source.next();
                            return DELIMITERCLASS;
                        } else {
                            return OPERATORCLASS;
                        }
                    } else if (doubleDelimiters.test(temp)) {
                        source.next();
                        return DELIMITERCLASS;
                    }
                }
                // It must be a single delimiter or operator
                if (singleOperators.indexOf(ch) != -1) {
                    return OPERATORCLASS;
                } else if (singleDelimiters.indexOf(ch) != -1) {
                    if (ch == '@' && /\w/.test(source.peek())) {
                        possible = {style:'py-decorator',
                                    content: source.get()};
                        ch = source.next();
                    } else if (ch == '.' && /\d/.test(source.peek())) {
                        possible = {style:LITERALCLASS,
                                    content: source.get()};
                        ch = source.next();
                    } else {
                        return DELIMITERCLASS;
                    }
                } else {
                    return ERRORCLASS;
                }
            }
            // Handle number literals
            if (/\d/.test(ch)) {
                if (ch === '0' && !source.endOfLine()) {
                    switch (source.peek()) {
                        case 'o':
                        case 'O':
                            source.next();
                            source.nextWhileMatches(/[0-7]/);
                            return filterPossible(LITERALCLASS, ERRORCLASS);
                        case 'x':
                        case 'X':
                            source.next();
                            source.nextWhileMatches(/[0-9A-Fa-f]/);
                            return filterPossible(LITERALCLASS, ERRORCLASS);
                        case 'b':
                        case 'B':
                            source.next();
                            source.nextWhileMatches(/[01]/);
                            return filterPossible(LITERALCLASS, ERRORCLASS);
                    }
                }
                source.nextWhileMatches(/\d/);
                if (source.peek() == '.') {
                    source.next();
                    source.nextWhileMatches(/\d/);
                }
                // Grab an exponent
                if (source.peek().toLowerCase() == 'e') {
                    source.next();
                    if (source.peek() == '+' || source.peek() == '-') {
                        source.next();
                    }
                    if (/\d/.test(source.peek())) {
                        source.nextWhileMatches(/\d/);
                    } else {
                        return filterPossible(ERRORCLASS);
                    }
                }
                // Grab a complex number
                if (source.peek().toLowerCase() == 'j') {
                    source.next();
                }

                return filterPossible(LITERALCLASS);
            }
            // Handle strings
            if (stringStarters.test(ch)) {
                var peek = source.peek();
                var stringType = STRINGCLASS;
                if ((stringTypes.test(ch)) && (peek == '"' || peek == "'")) {
                    switch (ch.toLowerCase()) {
                        case 'b':
                            stringType = BYTESCLASS;
                            break;
                        case 'r':
                            stringType = RAWCLASS;
                            break;
                        case 'u':
                            stringType = UNICODECLASS;
                            break;
                    }
                    ch = source.next();
                    stringDelim = ch;
                    if (source.peek() != stringDelim) {
                        setState(inString(stringType, stringDelim));
                        return null;
                    } else {
                        source.next();
                        if (source.peek() == stringDelim) {
                            source.next();
                            threeStr = stringDelim + stringDelim + stringDelim;
                            setState(inString(stringType, threeStr));
                            return null;
                        } else {
                            return stringType;
                        }
                    }
                } else if (ch == "'" || ch == '"') {
                    stringDelim = ch;
                    if (source.peek() != stringDelim) {
                        setState(inString(stringType, stringDelim));
                        return null;
                    } else {
                        source.next();
                        if (source.peek() == stringDelim) {
                            source.next();
                            threeStr = stringDelim + stringDelim + stringDelim;
                            setState(inString(stringType, threeStr));
                            return null;
                        } else {
                            return stringType;
                        }
                    }
                }
            }
            // Handle Identifier
            if (identifierStarters.test(ch)) {
                source.nextWhileMatches(/[\w\d]/);
                word = source.get();
                if (wordOperators.test(word)) {
                    type = OPERATORCLASS;
                } else if (keywords.test(word)) {
                    type = 'py-keyword';
                } else if (types.test(word)) {
                    type = 'py-type';
                } else {
                    type = IDENTIFIERCLASS;
                    while (source.peek() == '.') {
                        source.next();
                        if (identifierStarters.test(source.peek())) {
                            source.nextWhileMatches(/[\w\d]/);
                        } else {
                            type = ERRORCLASS;
                            break;
                        }
                    }
                    word = word + source.get();
                }
                return filterPossible({style: type, content: word});
            }

            // Register Dollar sign and Question mark as errors. Always!
            if (/\$\?/.test(ch)) {
                return filterPossible(ERRORCLASS);
            }

            return filterPossible(ERRORCLASS);
        }

        function inString(style, terminator) {
            return function(source, setState) {
                var matches = [];
                var found = false;
                while (!found && !source.endOfLine()) {
                    var ch = source.next(), newMatches = [];
                    // Skip escaped characters
                    if (ch == '\\') {
                        if (source.peek() == '\n') {
                            break;
                        }
                        ch = source.next();
                        ch = source.next();
                    }
                    if (ch == terminator.charAt(0)) {
                        matches.push(terminator);
                    }
                    for (var i = 0; i < matches.length; i++) {
                        var match = matches[i];
                        if (match.charAt(0) == ch) {
                            if (match.length == 1) {
                                setState(normal);
                                found = true;
                                break;
                            } else {
                                newMatches.push(match.slice(1));
                            }
                        }
                    }
                    matches = newMatches;
                }
                return style;
            };
        }

        return function(source, startState) {
            return tokenizer(source, startState || normal);
        };
    })();

    function parsePython(source) {
        if (!keywords) {
            configure({});
        }

        var tokens = tokenizePython(source);
        var lastToken = null;
        var column = 0;
        var context = {prev: null,
                       endOfScope: false,
                       startNewScope: false,
                       level: 0,
                       next: null,
                       type: NORMALCONTEXT
                       };

        function pushContext(level, type) {
            type = type ? type : NORMALCONTEXT;
            context = {prev: context,
                       endOfScope: false,
                       startNewScope: false,
                       level: level,
                       next: null,
                       type: type
                       };
        }

        function popContext(remove) {
            remove = remove ? remove : false;
            if (context.prev) {
                if (remove) {
                    context = context.prev;
                    context.next = null;
                } else {
                    context.prev.next = context;
                    context = context.prev;
                }
            }
        }

        function indentPython(context) {
            var temp;
            return function(nextChars, currentLevel, direction) {
                if (direction === null || direction === undefined) {
                    if (nextChars) {
                        while (context.next) {
                            context = context.next;
                        }
                    }
                    return context.level;
                }
                else if (direction === true) {
                    if (currentLevel == context.level) {
                        if (context.next) {
                            return context.next.level;
                        } else {
                            return context.level;
                        }
                    } else {
                        temp = context;
                        while (temp.prev && temp.prev.level > currentLevel) {
                            temp = temp.prev;
                        }
                        return temp.level;
                    }
                } else if (direction === false) {
                    if (currentLevel > context.level) {
                        return context.level;
                    } else if (context.prev) {
                        temp = context;
                        while (temp.prev && temp.prev.level >= currentLevel) {
                            temp = temp.prev;
                        }
                        if (temp.prev) {
                            return temp.prev.level;
                        } else {
                            return temp.level;
                        }
                    }
                }
                return context.level;
            };
        }

        var iter = {
            next: function() {
                var token = tokens.next();
                var type = token.style;
                var content = token.content;

                if (lastToken) {
                    if (lastToken.content == 'def' && type == IDENTIFIERCLASS) {
                        token.style = 'py-func';
                    }
                    if (lastToken.content == '\n') {
                        var tempCtx = context;
                        // Check for a different scope
                        if (type == 'whitespace' && context.type == NORMALCONTEXT) {
                            if (token.value.length < context.level) {
                                while (token.value.length < context.level) {
                                    popContext();
                                }

                                if (token.value.length != context.level) {
                                    context = tempCtx;
                                    if (config.strictErrors) {
                                        token.style = ERRORCLASS;
                                    }
                                } else {
                                    context.next = null;
                                }
                            }
                        } else if (context.level !== 0 &&
                                   context.type == NORMALCONTEXT) {
                            while (0 !== context.level) {
                                popContext();
                            }

                            if (context.level !== 0) {
                                context = tempCtx;
                                if (config.strictErrors) {
                                    token.style = ERRORCLASS;
                                }
                            }
                        }
                    }
                }

                // Handle Scope Changes
                switch(type) {
                    case STRINGCLASS:
                    case BYTESCLASS:
                    case RAWCLASS:
                    case UNICODECLASS:
                        if (context.type !== STRINGCONTEXT) {
                            pushContext(context.level + 1, STRINGCONTEXT);
                        }
                        break;
                    default:
                        if (context.type === STRINGCONTEXT) {
                            popContext(true);
                        }
                        break;
                }
                switch(content) {
                    case '.':
                    case '@':
                        // These delimiters don't appear by themselves
                        if (content !== token.value) {
                            token.style = ERRORCLASS;
                        }
                        break;
                    case ':':
                        // Colons only delimit scope inside a normal scope
                        if (context.type === NORMALCONTEXT) {
                            context.startNewScope = context.level+indentUnit;
                        }
                        break;
                    case '(':
                    case '[':
                    case '{':
                        // These start a sequence scope
                        pushContext(column + content.length, 'sequence');
                        break;
                    case ')':
                    case ']':
                    case '}':
                        // These end a sequence scope
                        popContext(true);
                        break;
                    case 'pass':
                    case 'return':
                        // These end a normal scope
                        if (context.type === NORMALCONTEXT) {
                            context.endOfScope = true;
                        }
                        break;
                    case '\n':
                        // Reset our column
                        column = 0;
                        // Make any scope changes
                        if (context.endOfScope) {
                            context.endOfScope = false;
                            popContext();
                        } else if (context.startNewScope !== false) {
                            var temp = context.startNewScope;
                            context.startNewScope = false;
                            pushContext(temp, NORMALCONTEXT);
                        }
                        // Newlines require an indentation function wrapped in a closure for proper context.
                        token.indentation = indentPython(context);
                        break;
                }

                // Keep track of current column for certain scopes.
                if (content != '\n') {
                    column += token.value.length;
                }

                lastToken = token;
                return token;
            },

            copy: function() {
                var _context = context, _tokenState = tokens.state;
                return function(source) {
                    tokens = tokenizePython(source, _tokenState);
                    context = _context;
                    return iter;
                };
            }
        };
        return iter;
    }

    return {make: parsePython,
            electricChars: "",
            configure: configure};
})();
