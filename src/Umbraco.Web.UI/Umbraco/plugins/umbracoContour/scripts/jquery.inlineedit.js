/*
* jQuery inlineEdit
*
* Copyright (c) 2009 Ca-Phun Ung <caphun at yelotofu dot com>
* Dual licensed under the MIT (MIT-LICENSE.txt)
* and GPL (GPL-LICENSE.txt) licenses.
*
* http://yelotofu.com/labs/jquery/snippets/inlineEdit/
*
* Inline (in-place) editing.
*/

(function($) {

    $.fn.inlineEdit = function(options) {

        options = $.extend({
            hover: 'hover',
            value: '',
            save: '',
            placeholder: 'Click to edit'
        }, options);

        return $.each(this, function() {
            $.inlineEdit(this, options);
        });
    }

    $.inlineEdit = function(obj, options) {

        var self = $(obj),
            placeholderHtml = '<span class="inlineEdit-placeholder">' + options.placeholder + '</span>';

        self.value = function(newValue) {
            if (arguments.length) {
                self.data('value', $(newValue).hasClass('inlineEdit-placeholder') ? '' : newValue);
            }
            return self.data('value');
        }

        self.value($.trim(self.text()) || options.value);

        self.bind('click', function(event) {
            var $this = $(event.target);

            if ($this.is(self[0].tagName) || $this.hasClass('inlineEdit-placeholder')) {
                self
                    .html('<input type="text" value="' + self.value() + '">')
                    .find('input')
                        .bind('blur', function() {
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
                            self.timer = window.setTimeout(function() {
                                self.html(self.value() || placeholderHtml);
                                self.removeClass(options.hover);
                            }, 200);
                        })
                        .focus();
            }
        })
        .hover(
            function() {
                $(this).addClass(options.hover);
            },
            function() {
                $(this).removeClass(options.hover);
            }
        );

        if (!self.value()) {
            self.html($(placeholderHtml));
        }
    }

})(jQuery);