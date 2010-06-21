(function ($) {
    ///<summary>Custom plugin to create the Umbraco context menu, this configures the context menu plugin before it's callbacks eecute</summary>

    $.extend($.tree.plugins, {
        "UmbracoContext": {

            settings: {
                fullMenu: [],
                onBeforeContext: false
            },

            _getContextMenu: function (strMenu) {
                /// <summary>Builds a new context menu object (array) based on the string representation passed in</summary>

                var newMenu = new Array();
                var separatorIndexes = new Array();
                for (var i = 0; i < strMenu.length; i++) {
                    var letter = strMenu.charAt(i);
                    //get a js menu item by letter
                    var menuItem = this._getMenuItemByLetter(letter);
                    if (menuItem == "separator") { separatorIndexes.push(newMenu.length); }
                    else if (menuItem != null) newMenu.push(menuItem);
                }
                for (var i = 0; i < separatorIndexes.length; i++) {
                    if (separatorIndexes[i] > 0) {
                        newMenu[separatorIndexes[i] - 1].separator_after = true;
                    }
                }
                return newMenu;
            },

            _getMenuItemByLetter: function (letter) {

                /// <summary>Finds the menu item in our full menu by the letter and returns object</summary>
                var fullMenu = $.tree.plugins.UmbracoContext.settings.fullMenu;
                //insert selector if it's a comma
                if (letter == ",") return "separator";
                for (var m in fullMenu) {
                    if (fullMenu[m].id == letter) {
                        return fullMenu[m];
                    }
                }
                return null;
            },

            callbacks: {
                oninit: function (t) {
                    $.tree.plugins.UmbracoContext.settings = t.settings.plugins.UmbracoContext;
                    //need to remove the defaults
                    $.tree.plugins.contextmenu.defaults.items = {};
                },
                onrgtclk: function (n, t, e) {
                    ///<summary>Need to set the context menu items for the context menu plugin for the current node</summary>
                    var _this = $.tree.plugins.UmbracoContext;

                    var menu = _this.settings.onBeforeContext.call(this, n, t, e);
                    if (menu != "") {
                        t.settings.plugins.contextmenu.items = _this._getContextMenu(menu);
                    }
                    else {
                        t.settings.plugins.contextmenu.items = [];
                    }
                }
            }
        }
    });
    $(function () {
        //add events to auto hide the menu on a delay
        var pause = true;
        var timer = null;
        $("#jstree-contextmenu").bind("mouseenter", function () {
            pause = true;
            clearTimeout(timer);
        });
        $("#jstree-contextmenu").bind("mouseleave", function () {
            pause = false;
            timer = setTimeout(function () {
                if (!pause) {
                    $.tree.plugins.contextmenu.hide();
                }
            }, 500);
        });
        //disable right clicking the context menu, this is for IE bug
        $("#jstree-contextmenu").bind("contextmenu", function (e) {
            e.preventDefault();
            return false;
        });
    });
})(jQuery);