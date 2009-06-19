
var ie = document.all ? true : false;
var x, y;
var currentid = 1;

var menuHeight;
var menuWidth = 150;
var menutop;
var menuleft;

// helpfunction locate cursor
function getMousePos(e) {
    if (ie) {
        x = window.event.clientX + window.top.document.body.scrollLeft + 20;
        y = window.event.clientY + window.top.document.body.scrollTop + 80;
    }
    else {
        x = e.pageX + 20;
        y = e.pageY - jQuery(document).scrollTop() + 80;
    }
}



function WinHeightY() {
    var winY;
    if (typeof (window.innerWidth) == 'number') {
        winY = window.top.innerHeight;
    } else if (document.documentElement &&
				(document.documentElement.clientWidth || document.documentElement.clientHeight)) {
        winY = window.top.document.documentElement.clientHeight;
    } else if (document.body && (document.body.clientWidth || document.body.clientHeight)) {
        winY = window.top.document.body.clientHeight;
    }
    return winY;
}

// initialize listener.
if (!ie) { document.captureEvents(Event.MOUSEMOVE); }
document.onmousemove = getMousePos;

// Contextmenu class
function ContextMenu() {

    var menuEl;
    var Delay = 500;
    

    this.el = null;
    this.Show = function(menuitems) {
        // tell global variable that the menu is open
        menuActive = true;
        var ihtml = "";
        var i;
        menuHeight = 0;
        // add new menu items
        for (i = 0; i < menuitems.length; i++) {
            if (!menuitems[i].isSplitter) {
                if (menuitems[i].icon.substring(0, 6) != 'images')
                    ihtml += "<div onmouseover=\"this.className='menuiconon'\" onmouseout=\"this.className='menuicon'\" class=\"menuicon\" onclick=\"window.tree.menu.Hide(); " + menuitems[i].action + "\"><div class=\"menuSpr " + menuitems[i].icon + "\">&nbsp;</div><div style='padding-top:3px;'>" + menuitems[i].text + "</div></div>";
                else
                    ihtml += "<div onmouseover=\"this.className='menuiconon'\" onmouseout=\"this.className='menuicon'\" class=\"menuicon\" onclick=\"window.tree.menu.Hide(); " + menuitems[i].action + "\"><img src=\"" + menuitems[i].icon + "\" align=\"left\"><div style='padding-top:3px;'>" + menuitems[i].text + "</div></div>";
                menuHeight += 30;
            }
            else {
                ihtml += "<img src=\"css/splitter.gif\" class=\"splitter\">";
                menuHeight += 3;
            }
        }

        menutop = (y - 10);
        menuleft = (x - 10);


        if (y + menuHeight > WinHeightY()) {
            // We need to move the menu up - since there are no place for it
            // in the current screen
            menutop -= (WinHeightY() - y + 10);
            
            // Check if menu is outside of screen area
            if (menutop < 0) {
                menutop = 10;
            } else if (menutop + menuHeight > WinHeightY()) {
                menutop -= (menutop + menuHeight - WinHeightY() - 10);
            }
        }

        getMenuElement().innerHTML = ihtml;
        getMenuElement().style.display = 'block';
        getMenuElement().style.top = menutop + "px";
        getMenuElement().style.left = menuleft + "px";

        window.setTimeout("menu.validateContextMenu();", 100);
        return false;
    }

    this.Hide = function() {
        getMenuElement().innerHTML = "";
        getMenuElement().style.display = 'none';
    }

    function ensureDiv() {
        if (window.top.document.getElementById("cmenu") == null) {
            var bodyel = window.top.document.getElementsByTagName("body")[0];
            var x = window.top.document.createElement("div");
            x.setAttribute("id", "cmenu");
            bodyel.appendChild(x);
            getMenuElement().style.position = "absolute";
            //getMenuElement().style.width = menuWidth + "px";
        }
    }

    function getMenuElement() {
        ensureDiv();
        if (menuEl == null) {
            menuEl = window.top.document.getElementById('cmenu');
        }
        return menuEl;
    }

    this.validateContextMenu = function() {
        //alert(" y: " + y + " top: " + top + " menuHeight: " + menuHeight);
        //alert((y >= menutop) + " y: " + y + " top: " + menutop);
        if (  (x >= menuleft) &&  (x <= (menuleft + menuWidth)) && (y >= menutop)  &&  (y <= (menutop + menuHeight)) ) {
            window.setTimeout("menu.validateContextMenu();", 100);
        } else {
            this.Hide();
            menuActive = '';
        }
    }
}

// MenuItem class
function MenuItem(text, action, icon, isSplitter) {
    this.text = text;
    this.action = action;
    this.icon = icon;
    this.isSplitter = isSplitter;
}

// Create a contextmenu for the page.
var menu = new ContextMenu();
