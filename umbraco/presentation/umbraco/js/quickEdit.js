var acOptions = {
    minChars: 2,
    max: 100,
    cacheLength: 1,
    dataType: 'json',
    matchCase: true,
    matchContains: false,
    parse: function(data) {
        var parsed = [];
        data = data.results;

        for (var i = 0; i < data.length; i++) {
            parsed[parsed.length] = {
                data: data[i],
                value: data[i].NodeId,
                result: data[i].Title
            };
        }

        return parsed;
    },
    formatItem: function(item) {
        return item.Title + " <small>(" + item.NodeId + ")</small>";
    }
};

jQuery(document).ready(function($) {
    jQuery("#umbSearchField")
          .autocomplete("dashboard/search.aspx", acOptions)
          .result(function(e, data) {
              UmbClientMgr.contentFrame().location.href = "editContent.aspx?id=" + data.NodeId;
              jQuery("#umbSearchField").val(uiKeys["general_typeToSearch"]);
              right.focus();
          });

    jQuery("#umbSearchField").focus(function() {
        jQuery(this).val('');
    });

    jQuery("#umbSearchField").blur(function() {
        jQuery(this).val(uiKeys["general_typeToSearch"]);
    });

});
