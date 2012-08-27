(function () {

    CodeMirror.razorHints = [];


    function arrayContains(arr, item) {
        if (!Array.prototype.indexOf) {
            var i = arr.length;
            while (i--) {
                if (arr[i] === item) {
                    return true;
                }
            }
            return false;
        }
        return arr.indexOf(item) != -1;
    }


    CodeMirror.razorHint = function (editor, char) {

        if (char.length > 0) {
            var cursor = editor.getCursor();
            editor.replaceSelection(char);
            cursor = { line: cursor.line, ch: cursor.ch + 1 };
            editor.setCursor(cursor);
        }

        // dirty hack for simple-hint to receive getHint event on space
        var getTokenAt = editor.getTokenAt;
        editor.getTokenAt = function () { return 'disabled'; };
        CodeMirror.simpleHint(editor, getHint);
        editor.getTokenAt = getTokenAt;

        //return scriptHint(editor, pigKeywordsU, char, function (e, cur) { return e.getTokenAt(cur); });
    }

    var getHint = function (cm) {

        var cursor = cm.getCursor();
        if (cursor.ch > 0) {

            var text = cm.getRange({ line: 0, ch: 0 }, cursor);
            var typed = '';
            var simbol = '';

            for (var i = text.length - 1; i >= 0; i--) {
                if (text[i] == '@' || text[i] == '.') {
                    simbol = text[i];
                    break;
                }
                else {
                    typed = text[i] + typed;
                }
            }
        }

        text = text.slice(0, text.length - typed.length);

        var hints = getCompletions(text, simbol, typed);
        return {
            list: hints,
            from: { line: cursor.line, ch: cursor.ch - typed.length },
            to: cursor
        };
    };

    function searchHints(text, trigger, search) {
        if (search == "") {
            var hints = CodeMirror.razorHints[text];
            if (typeof hints === 'undefined')
                hints = CodeMirror.razorHints[simbol];

            return hints;
        } else {

        }
    }

    function getCompletions(text, trigger, search) {
        var found = [];

        function maybeAdd(str) {
            var match = str;
            if (typeof match === 'string') {
                match = [str, str];
            }

            if (search == "" && !arrayContains(found, match)) found.push(match);
            else if (match[0].toLowerCase().indexOf(search) == 0 && !arrayContains(found, match)) found.push(match);
        }

        forEach(CodeMirror.razorHints[text], maybeAdd);
        forEach(CodeMirror.razorHints[trigger], maybeAdd);

        return found.sort(function (a, b) {
            var nameA = a[0].toLowerCase(), nameB = b[0].toLowerCase()
            if (nameA < nameB)
                return -1
            if (nameA > nameB)
                return 1

            return 0 //default return value (no sorting)
        })
    }

    function forEach(arr, f) {
        if (typeof arr != 'undefined')
            for (var i = 0, e = arr.length; i < e; ++i) f(arr[i]);
    }


    function toTitleCase(str) {
        return str.replace(/(?:^|\s)\w/g, function (match) {
            return match.toUpperCase();
        });
    }

})();
