(function($) {
    
    $.fn.UmbQuickSearch = function(url) {
        var acOptions = {
            minChars: 2,
            max: 100,
            cacheLength: 1,
            dataType: 'json',
            matchCase: true,
            matchContains: false,
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
    }
        
})(jQuery);