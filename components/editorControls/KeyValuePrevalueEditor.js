
jQuery(document).ready(function () {

    jQuery("#prevalues .text").valuesInlineEdit();

    jQuery("#prevalues tbody").sortable({
        items: "tr:not(.header)",
        handle: '.handle',
        update: function () {

            ResetValues();

        }
    });



});


function ResetValues() {

    var val = "";
   
    jQuery("#prevalues .row").each(function () {

        var text = jQuery(".text", this).html();
        var value = jQuery(".value", this).html();


        val += value + "|" + text + ";";

    });

    jQuery(".valuesHiddenInput").val(val);
}

//inline edit
(function ($) {

    $.fn.valuesInlineEdit = function (options) {

        options = $.extend({
            hover: 'hover',
            value: '',
            save: '',
            placeholder: 'Click to edit'
        }, options);

        return $.each(this, function () {
            $.valuesInlineEdit(this, options);
        });
    }

    $.valuesInlineEdit = function (obj, options) {

        var self = $(obj),
            placeholderHtml = '<span class="inlineEdit-placeholder">' + options.placeholder + '</span>';

        self.value = function (newValue) {
            if (arguments.length) {
                self.data('value', $(newValue).hasClass('inlineEdit-placeholder') ? '' : newValue);
            }
            return self.data('value');
        }

        self.value($.trim(self.text()) || options.value);

        self.bind('click', function (event) {
            var $this = $(event.target);

            if ($this.is(self[0].tagName) || $this.hasClass('inlineEdit-placeholder')) {
                self
                    .html('<input type="text" value="' + self.value() + '">')
                    .find('input')
                        .bind('blur', function () {
                            if ($this.children('input').val().length > 0) {

                                try {
                                    self.value($this.children('input').val());



                                } catch (err) { }


                            }
                            else {
                                self.value('Click to edit');
                            }
                            if (self.timer) {
                                window.clearTimeout(self.timer);
                            }
                            self.timer = window.setTimeout(function () {
                                self.html(self.value() || placeholderHtml);
                                self.removeClass(options.hover);
                                ResetValues();
                            }, 200);
                        })
                        .focus();
            }
        })
        .hover(
            function () {
                $(this).addClass(options.hover);
            },
            function () {
                $(this).removeClass(options.hover);
            }
        );

        if (!self.value()) {
            self.html($(placeholderHtml));
        }
    }

})(jQuery);