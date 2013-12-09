(function ($) {

    $.fn.UmbQuickSearch = function (url) {

        var getSearchApp = function () {

            if (UmbClientMgr.mainWindow().location.hash != "") {
                switch (UmbClientMgr.mainWindow().location.hash.toLowerCase().substring(1).toLowerCase()) {
                    case "media":
                        return "Media";
                        break;
                    case "content":
                        return "Content";
                        break;
                    case "member":
                        return "Member";
                        break;
                    default:
                        return "Content";
                }
            }
            return "Content";

            /* return (UmbClientMgr.mainWindow().location.hash != ""
            && UmbClientMgr.mainWindow().location.hash.toLowerCase().substring(1)) == "media".toLowerCase()
            ? "Media"
            : "Content"; */
        };

        var acOptions = {
            minChars: 2,
            max: 100,
            cacheLength: 1,
            dataType: 'json',
            matchCase: true,
            matchContains: false,
            selectFirst: false, // FR: This enabled the search popup to show, otherwise it selects the first item
            extraParams: {
                //return the current app, if it's not media, then it's Content as this is the only searches that are supported.
                app: function () {
                    return getSearchApp();
                },
                rnd: function () {
                    return Umbraco.Utils.generateRandom();
                }
            },
            parse: function (data) {
                var parsed = [];
                for (var i = 0; i < data.length; i++) {
                    parsed[parsed.length] = {
                        data: data[i],
                        value: data[i].Id,
                        result: data[i].Fields.nodeName
                    };
                }
                return parsed;
            },
            formatItem: function (item) {
                return item.Fields.nodeName + " <span class='nodeId'>(" + item.Id + ") </span>";
            },
            focus: function (event, ui) {
                $(ui).attr("title", $(ui).find("span[title]").attr("title"));
            }
        };

        $(this)
              .autocomplete(url, acOptions)
              .result(function (e, data) {

                  var url = "";
                  switch (getSearchApp()) {
                      case "Media":
                          url = "editMedia.aspx";
                          break;
                      case "Content":
                          url = "editContent.aspx";
                          break;
                      case "Member":
                          url = "members/editMember.aspx";
                          break;
                      default:
                          url = "editContent.aspx";
                  }
                  UmbClientMgr.contentFrame().location.href = url + "?id=" + data.Id;
                  $("#umbSearchField").val(UmbClientMgr.uiKeys()["general_typeToSearch"]);
                  right.focus();
              });


        $(this).focus(function () {
            $(this).val('');
        });

        $(this).blur(function () {
            $(this).val(UmbClientMgr.uiKeys()["general_typeToSearch"]);
        });

        $(this).keyup(function (e) {
            if (e.keyCode == 13) {

                UmbClientMgr.openModalWindow('dialogs/search.aspx?rndo=' + Umbraco.Utils.generateRandom() + '&search=' + jQuery(this).val() + '&app=' + getSearchApp(), 'Search', true, 620, 470);
                return false;
            }
        });
    }

})(jQuery);