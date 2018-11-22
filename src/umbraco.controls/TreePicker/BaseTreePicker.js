/// <reference path="/umbraco_client/Application/NamespaceManager.js" />
(function ($) {
    $(document).ready(function () {
        // Tooltip only Text
        $('.umb-tree-picker a.choose').click(function () {
            var that = this;
            var s = $(that).data("section");
            UmbClientMgr.openAngularModalWindow({
                template: 'views/common/dialogs/treepicker.html',
                section: s,
                callback: function (data) {
                    //this returns the content object picked
                    var p = jQuery(that).parent();
                    p.find(".buttons").show();

                    p.find("input").val(data.id);
                    p.find(".treePickerTitle").text(data.name).show();
                    p.find(".clear").show();
                }
            });

            return false;
        });

        $('.umb-tree-picker a.clear').click(function () {
            jQuery(this).parent().parent().find("input").val("-1");
            jQuery(this).parent().parent().find(".treePickerTitle").text("").hide();
            jQuery(this).hide();
        });
    });
})(jQuery);