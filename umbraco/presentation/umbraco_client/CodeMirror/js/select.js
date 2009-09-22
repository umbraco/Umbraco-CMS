/* Functionality for finding, storing, and restoring selections
 *
 * This does not provide a generic API, just the minimal functionality
 * required by the CodeMirror system.
 */

// Namespace object.
var select = {};

(function() {
  select.ie_selection = document.selection && document.selection.createRangeCollection;

  // Find the 'top-level' (defined as 'a direct child of the node
  // passed as the top argument') node that the given node is
  // contained in. Return null if the given node is not inside the top
  // node.
  function topLevelNodeAt(node, top) {
    while (node && node.parentNode != top)
      node = node.parentNode;
    return node;
  }

  // Find the top-level node that contains the node before this one.
  function topLevelNodeBefore(node, top) {
    while (!node.previousSibling && node.parentNode != top)
      node = node.parentNode;
    return topLevelNodeAt(node.previousSibling, top);
  }

  var fourSpaces = "\u00a0\u00a0\u00a0\u00a0";

  select.scrollToNode = function(element) {
    if (!element) return;
    var doc = element.ownerDocument, body = doc.body,
        win = (doc.defaultView || doc.parentWindow),
        html = doc.documentElement,
        atEnd = !element.nextSibling || !element.nextSibling.nextSibling
                || !element.nextSibling.nextSibling.nextSibling;
    // In Opera (and recent Webkit versions), BR elements *always*
    // have a scrollTop property of zero.
    var compensateHack = 0;
    while (element && !element.offsetTop) {
      compensateHack++;
      element = element.previousSibling;
    }
    // atEnd is another kludge for these browsers -- if the cursor is
    // at the end of the document, and the node doesn't have an
    // offset, just scroll to the end.
    if (compensateHack == 0) atEnd = false;

    var y = compensateHack * (element ? element.offsetHeight : 0), x = 0, pos = element;
    while (pos && pos.offsetParent) {
      y += pos.offsetTop;
      // Don't count X offset for <br> nodes
      if (pos.nodeName != "BR")
        x += pos.offsetLeft;
      pos = pos.offsetParent;
    }

    var scroll_x = body.scrollLeft || html.scrollLeft || 0,
        scroll_y = body.scrollTop || html.scrollTop || 0,
        screen_x = x - scroll_x, screen_y = y - scroll_y, scroll = false;

    if (screen_x < 0 || screen_x > (win.innerWidth || html.clientWidth || 0)) {
      scroll_x = x;
      scroll = true;
    }
    if (screen_y < 0 || atEnd || screen_y > (win.innerHeight || html.clientHeight || 0) - 50) {
      scroll_y = atEnd ? 1e10 : y;
      scroll = true;
    }
    if (scroll) win.scrollTo(scroll_x, scroll_y);
  };

  select.scrollToCursor = function(container) {
    select.scrollToNode(select.selectionTopNode(container, true) || container.firstChild);
  };

  // Used to prevent restoring a selection when we do not need to.
  var currentSelection = null;

  select.snapshotChanged = function() {
    if (currentSelection) currentSelection.changed = true;
  };

  // This is called by the code in editor.js whenever it is replacing
  // a text node. The function sees whether the given oldNode is part
  // of the current selection, and updates this selection if it is.
  // Because nodes are often only partially replaced, the length of
  // the part that gets replaced has to be taken into account -- the
  // selection might stay in the oldNode if the newNode is smaller
  // than the selection's offset. The offset argument is needed in
  // case the selection does move to the new object, and the given
  // length is not the whole length of the new node (part of it might
  // have been used to replace another node).
  select.snapshotReplaceNode = function(from, to, length, offset) {
    if (!currentSelection) return;

    function replace(point) {
      if (from == point.node) {
        currentSelection.changed = true;
        if (length && point.offset > length) {
          point.offset -= length;
        }
        else {
          point.node = to;
          point.offset += (offset || 0);
        }
      }
    }
    replace(currentSelection.start);
    replace(currentSelection.end);
  };

  select.snapshotMove = function(from, to, distance, relative, ifAtStart) {
    if (!currentSelection) return;

    function move(point) {
      if (from == point.node && (!ifAtStart || point.offset == 0)) {
        currentSelection.changed = true;
        point.node = to;
        if (relative) point.offset = Math.max(0, point.offset + distance);
        else point.offset = distance;
      }
    }
    move(currentSelection.start);
    move(currentSelection.end);
  };

  // Most functions are defined in two ways, one for the IE selection
  // model, one for the W3C one.
  if (select.ie_selection) {
    function selectionNode(win, start) {
      var range = win.document.selection.createRange();
      range.collapse(start);

      function nodeAfter(node) {
        var found = null;
        while (!found && node) {
          found = node.nextSibling;
          node = node.parentNode;
        }
        return nodeAtStartOf(found);
      }

      function nodeAtStartOf(node) {
        while (node && node.firstChild) node = node.firstChild;
        return {node: node, offset: 0};
      }

      var containing = range.parentElement();
      if (!isAncestor(win.document.body, containing)) return null;
      if (!containing.firstChild) return nodeAtStartOf(containing);

      var working = range.duplicate();
      working.moveToElementText(containing);
      working.collapse(true);
      for (var cur = containing.firstChild; cur; cur = cur.nextSibling) {
        if (cur.nodeType == 3) {
          var size = cur.nodeValue.length;
          working.move("character", size);
        }
        else {
          working.moveToElementText(cur);
          working.collapse(false);
        }

        var dir = range.compareEndPoints("StartToStart", working);
        if (dir == 0) return nodeAfter(cur);
        if (dir == 1) continue;
        if (cur.nodeType != 3) return nodeAtStartOf(cur);

        working.setEndPoint("StartToEnd", range);
        return {node: cur, offset: size - working.text.length};
      }
      return nodeAfter(containing);
    }

    select.markSelection = function(win) {
      currentSelection = null;
      var sel = win.document.selection;
      if (!sel) return;
      var start = selectionNode(win, true),
          end = selectionNode(win, false);
      if (!start || !end) return;
      currentSelection = {start: start, end: end, window: win, changed: false};
    };

    select.selectMarked = function() {
      if (!currentSelection || !currentSelection.changed) return;
      var win = currentSelection.window, doc = win.document;

      function makeRange(point) {
        var range = doc.body.createTextRange(),
            node = point.node;
        if (!node) {
          range.moveToElementText(currentSelection.window.document.body);
          range.collapse(false);
        }
        else if (node.nodeType == 3) {
          range.moveToElementText(node.parentNode);
          var offset = point.offset;
          while (node.previousSibling) {
            node = node.previousSibling;
            offset += (node.innerText || "").length;
          }
          range.move("character", offset);
        }
        else {
          range.moveToElementText(node);
          range.collapse(true);
        }
        return range;
      }

      var start = makeRange(currentSelection.start), end = makeRange(currentSelection.end);
      start.setEndPoint("StartToEnd", end);
      start.select();
    };

    // Get the top-level node that one end of the cursor is inside or
    // after. Note that this returns false for 'no cursor', and null
    // for 'start of document'.
    select.selectionTopNode = function(container, start) {
      var selection = container.ownerDocument.selection;
      if (!selection) return false;

      var range = selection.createRange(), range2 = range.duplicate();
      range.collapse(start);
      var around = range.parentElement();
      if (around && isAncestor(container, around)) {
        // Only use this node if the selection is not at its start.
        range2.moveToElementText(around);
        if (range.compareEndPoints("StartToStart", range2) == 1)
          return topLevelNodeAt(around, container);
      }

      // Move the start of a range to the start of a node,
      // compensating for the fact that you can't call
      // moveToElementText with text nodes.
      function moveToNodeStart(range, node) {
        if (node.nodeType == 3) {
          var count = 0, cur = node.previousSibling;
          while (cur && cur.nodeType == 3) {
            count += cur.nodeValue.length;
            cur = cur.previousSibling;
          }
          if (cur) {
            try{range.moveToElementText(cur);}
            catch(e){}
            range.collapse(false);
          }
          else range.moveToElementText(node.parentNode);
          if (count) range.move("character", count);
        }
        else range.moveToElementText(node);
      }

      // Do a binary search through the container object, comparing
      // the start of each node to the selection
      var start = 0, end = container.childNodes.length - 1;
      while (start < end) {
        var middle = Math.ceil((end + start) / 2), node = container.childNodes[middle];
        if (!node) return false; // Don't ask. IE6 manages this sometimes.
        moveToNodeStart(range2, node);
        if (range.compareEndPoints("StartToStart", range2) == 1)
          start = middle;
        else
          end = middle - 1;
      }
      return container.childNodes[start] || null;
    };

    // Place the cursor after this.start. This is only useful when
    // manually moving the cursor instead of restoring it to its old
    // position.
    select.focusAfterNode = function(node, container) {
      var range = container.ownerDocument.body.createTextRange();
      range.moveToElementText(node || container);
      range.collapse(!node);
      range.select();
    };

    select.somethingSelected = function(win) {
      var sel = win.document.selection;
      return sel && (sel.createRange().text != "");
    };

    function insertAtCursor(window, html) {
      var selection = window.document.selection;
      if (selection) {
        var range = selection.createRange();
        range.pasteHTML(html);
        range.collapse(false);
        range.select();
      }
    }

    // Used to normalize the effect of the enter key, since browsers
    // do widely different things when pressing enter in designMode.
    select.insertNewlineAtCursor = function(window) {
      insertAtCursor(window, "<br>");
    };

    select.insertTabAtCursor = function(window) {
      insertAtCursor(window, fourSpaces);
    };

    // Get the BR node at the start of the line on which the cursor
    // currently is, and the offset into the line. Returns null as
    // node if cursor is on first line.
    select.cursorPos = function(container, start) {
      var selection = container.ownerDocument.selection;
      if (!selection) return null;

      var topNode = select.selectionTopNode(container, start);
      while (topNode && topNode.nodeName != "BR")
        topNode = topNode.previousSibling;

      var range = selection.createRange(), range2 = range.duplicate();
      range.collapse(start);
      if (topNode) {
        range2.moveToElementText(topNode);
        range2.collapse(false);
      }
      else {
        // When nothing is selected, we can get all kinds of funky errors here.
        try { range2.moveToElementText(container); }
        catch (e) { return null; }
        range2.collapse(true);
      }
      range.setEndPoint("StartToStart", range2);

      return {node: topNode, offset: range.text.length};
    };

    select.setCursorPos = function(container, from, to) {
      function rangeAt(pos) {
        var range = container.ownerDocument.body.createTextRange();
        if (!pos.node) {
          range.moveToElementText(container);
          range.collapse(true);
        }
        else {
          range.moveToElementText(pos.node);
          range.collapse(false);
        }
        range.move("character", pos.offset);
        return range;
      }

      var range = rangeAt(from);
      if (to && to != from)
        range.setEndPoint("EndToEnd", rangeAt(to));
      range.select();
    }

    // Some hacks for storing and re-storing the selection when the editor loses and regains focus.
    select.selectionCoords = function (win) {
      var selection = win.document.selection;
      if (!selection) return null;
      var start = selection.createRange(), end = start.duplicate();
      start.collapse(true);
      end.collapse(false);

      var body = win.document.body;
      return {start: {x: start.boundingLeft + body.scrollLeft - 1,
                      y: start.boundingTop + body.scrollTop},
              end: {x: end.boundingLeft + body.scrollLeft - 1,
                    y: end.boundingTop + body.scrollTop}};
    };

    // Restore a stored selection.
    select.selectCoords = function(win, coords) {
      if (!coords) return;

      var range1 = win.document.body.createTextRange(), range2 = range1.duplicate();
      // This can fail for various hard-to-handle reasons.
      try {
        range1.moveToPoint(coords.start.x, coords.start.y);
        range2.moveToPoint(coords.end.x, coords.end.y);
        range1.setEndPoint("EndToStart", range2);
        range1.select();
      } catch(e) {}
    };
  }
  // W3C model
  else {
    // Store start and end nodes, and offsets within these, and refer
    // back to the selection object from those nodes, so that this
    // object can be updated when the nodes are replaced before the
    // selection is restored.
    select.markSelection = function (win) {
      var selection = win.getSelection();
      if (!selection || selection.rangeCount == 0)
        return (currentSelection = null);
      var range = selection.getRangeAt(0);

      currentSelection = {
        start: {node: range.startContainer, offset: range.startOffset},
        end: {node: range.endContainer, offset: range.endOffset},
        window: win,
        changed: false
      };

      // We want the nodes right at the cursor, not one of their
      // ancestors with a suitable offset. This goes down the DOM tree
      // until a 'leaf' is reached (or is it *up* the DOM tree?).
      function normalize(point){
        while (point.node.nodeType != 3 && point.node.nodeName != "BR") {
          var newNode = point.node.childNodes[point.offset] || point.node.nextSibling;
          point.offset = 0;
          while (!newNode && point.node.parentNode) {
            point.node = point.node.parentNode;
            newNode = point.node.nextSibling;
          }
          point.node = newNode;
          if (!newNode)
            break;
        }
      }

      normalize(currentSelection.start);
      normalize(currentSelection.end);
    };

    select.selectMarked = function () {
      if (!currentSelection || !currentSelection.changed) return;
      var win = currentSelection.window, range = win.document.createRange();

      function setPoint(point, which) {
        if (point.node) {
          // Some magic to generalize the setting of the start and end
          // of a range.
          if (point.offset == 0)
            range["set" + which + "Before"](point.node);
          else
            range["set" + which](point.node, point.offset);
        }
        else {
          range.setStartAfter(win.document.body.lastChild || win.document.body);
        }
      }

      setPoint(currentSelection.end, "End");
      setPoint(currentSelection.start, "Start");
      selectRange(range, win);
    };

    // Helper for selecting a range object.
    function selectRange(range, window) {
      var selection = window.getSelection();
      selection.removeAllRanges();
      selection.addRange(range);
    };
    function selectionRange(window) {
      var selection = window.getSelection();
      if (!selection || selection.rangeCount == 0)
        return false;
      else
        return selection.getRangeAt(0);
    }

    // Finding the top-level node at the cursor in the W3C is, as you
    // can see, quite an involved process.
    select.selectionTopNode = function(container, start) {
      var range = selectionRange(container.ownerDocument.defaultView);
      if (!range) return false;

      var node = start ? range.startContainer : range.endContainer;
      var offset = start ? range.startOffset : range.endOffset;
      // Work around (yet another) bug in Opera's selection model.
      if (window.opera && !start && range.endContainer == container && range.endOffset == range.startOffset + 1 &&
          container.childNodes[range.startOffset] && container.childNodes[range.startOffset].nodeName == "BR")
        offset--;

      // For text nodes, we look at the node itself if the cursor is
      // inside, or at the node before it if the cursor is at the
      // start.
      if (node.nodeType == 3){
        if (offset > 0)
          return topLevelNodeAt(node, container);
        else
          return topLevelNodeBefore(node, container);
      }
      // Occasionally, browsers will return the HTML node as
      // selection. If the offset is 0, we take the start of the frame
      // ('after null'), otherwise, we take the last node.
      else if (node.nodeName == "HTML") {
        return (offset == 1 ? null : container.lastChild);
      }
      // If the given node is our 'container', we just look up the
      // correct node by using the offset.
      else if (node == container) {
        return (offset == 0) ? null : node.childNodes[offset - 1];
      }
      // In any other case, we have a regular node. If the cursor is
      // at the end of the node, we use the node itself, if it is at
      // the start, we use the node before it, and in any other
      // case, we look up the child before the cursor and use that.
      else {
        if (offset == node.childNodes.length)
          return topLevelNodeAt(node, container);
        else if (offset == 0)
          return topLevelNodeBefore(node, container);
        else
          return topLevelNodeAt(node.childNodes[offset - 1], container);
      }
    };

    select.focusAfterNode = function(node, container) {
      var win = container.ownerDocument.defaultView,
          range = win.document.createRange();
      range.setStartBefore(container.firstChild || container);
      // In Opera, setting the end of a range at the end of a line
      // (before a BR) will cause the cursor to appear on the next
      // line, so we set the end inside of the start node when
      // possible.
      if (node && !node.firstChild)
        range.setEndAfter(node);
      else if (node)
        range.setEnd(node, node.childNodes.length);
      else
        range.setEndBefore(container.firstChild || container);
      range.collapse(false);
      selectRange(range, win);
    };

    select.somethingSelected = function(win) {
      var range = selectionRange(win);
      return range && !range.collapsed;
    };

    function insertNodeAtCursor(window, node) {
      var range = selectionRange(window);
      if (!range) return;

      range.deleteContents();
      range.insertNode(node);
      webkitLastLineHack(window.document.body);
      range = window.document.createRange();
      range.selectNode(node);
      range.collapse(false);
      selectRange(range, window);
    }

    select.insertNewlineAtCursor = function(window) {
      insertNodeAtCursor(window, window.document.createElement("BR"));
    };

    select.insertTabAtCursor = function(window) {
      insertNodeAtCursor(window, window.document.createTextNode(fourSpaces));
    };

    select.cursorPos = function(container, start) {
      var range = selectionRange(window);
      if (!range) return;

      var topNode = select.selectionTopNode(container, start);
      while (topNode && topNode.nodeName != "BR")
        topNode = topNode.previousSibling;

      range = range.cloneRange();
      range.collapse(start);
      if (topNode)
        range.setStartAfter(topNode);
      else
        range.setStartBefore(container);
      return {node: topNode, offset: range.toString().length};
    };

    select.setCursorPos = function(container, from, to) {
      var win = container.ownerDocument.defaultView,
          range = win.document.createRange();

      function setPoint(node, offset, side) {
        if (!node)
          node = container.firstChild;
        else
          node = node.nextSibling;

        if (!node)
          return;

        if (offset == 0) {
          range["set" + side + "Before"](node);
          return true;
        }

        var backlog = []
        function decompose(node) {
          if (node.nodeType == 3)
            backlog.push(node);
          else
            forEach(node.childNodes, decompose);
        }
        while (true) {
          while (node && !backlog.length) {
            decompose(node);
            node = node.nextSibling;
          }
          var cur = backlog.shift();
          if (!cur) return false;

          var length = cur.nodeValue.length;
          if (length >= offset) {
            range["set" + side](cur, offset);
            return true;
          }
          offset -= length;
        }
      }

      to = to || from;
      if (setPoint(to.node, to.offset, "End") && setPoint(from.node, from.offset, "Start"))
        selectRange(range, win);
    };
  }
})();
