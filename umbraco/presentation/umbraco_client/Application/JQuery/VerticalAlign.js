(function($) {
    $.fn.VerticalAlign = function(opts) {
        return this.each(function() {
            var top = (($(this).parent().height() - $(this).height()) / 2);
            $(this).css('margin-top', top);
        });
    };
})(jQuery);