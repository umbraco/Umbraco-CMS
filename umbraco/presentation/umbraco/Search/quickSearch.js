(function($) {

    $.fn.UmbQuickSearch = function(url) {

        var getSearchApp = function() {
            return (UmbClientMgr.mainWindow().location.hash != "" && UmbClientMgr.mainWindow().location.hash.toLowerCase().substring(1)) == "media".toLowerCase()
                ? "Media"
                : "Content";
        };

        var acOptions = {
            minChars: 2,
            max: 100,
            cacheLength: 1,
            dataType: 'json',
            matchCase: true,
            matchContains: false,
            extraParams: {
                //return the current app, if it's not media, then it's Content as this is the only searches that are supported.
                app: function() {
                    return getSearchApp();
                },
                rnd: function() {
                    return Umbraco.Utils.generateRandom();
                }
            },
            parse: function(data) {
                var parsed = [];
                //data = data.results;

                for (var i = 0; i < data.length; i++) {
                    parsed[parsed.length] = {
                        data: data[i],
                        value: data[i].Id,
                        result: data[i].Fields.nodeName
                    };
                }

                return parsed;
            },
            formatItem: function(item) {
                return item.Fields.nodeName + " <span class='nodeId'>(" + item.Id + ")</span>";
            }
        };

        $(this)
              .autocomplete(url, acOptions)
              .result(function(e, data) {
                  UmbClientMgr.contentFrame().location.href = "editContent.aspx?id=" + data.Id;
                  $("#umbSearchField").val(UmbClientMgr.uiKeys()["general_typeToSearch"]);
                  right.focus();
              });

        $(this).focus(function() {
            $(this).val('');
        });

        $(this).blur(function() {
            $(this).val(UmbClientMgr.uiKeys()["general_typeToSearch"]);
        });

        $(this).keyup(function(e) {
            if (e.keyCode == 13) {

                UmbClientMgr.openModalWindow('dialogs/search.aspx?rndo=' + Umbraco.Utils.generateRandom() + '&search=' + jQuery(this).val() + '&app=' + getSearchApp(), 'Search', true, 620, 470);
                return false;
            }
        });
    }

})(jQuery);