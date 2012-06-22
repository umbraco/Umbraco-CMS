jQuery(document).ready(function () {



    jQuery(".valuesDataEditorSettingType").each(function () {

        var vals = jQuery(".valuesInput", this).val();

        var values = jQuery(".values", this);

        var vals_array = vals.split(";");
        var part_num = 0;
        while (part_num < vals_array.length) {

            if (vals_array[part_num] != "") {
                var existingValue = "<div class='value'><span>" + vals_array[part_num] + "</span> <a class='del'>Delete</a> <a class='handle'>Drag</a></div>";

                values.append(existingValue);
            }
            part_num++;
        }

    });

    jQuery(".valuesDataEditorSettingType .value .del").click(function () {
        jQuery(this).parent().remove();
        valuesDataEditorSettingTypeResetValues();
    });


    jQuery(".valuesDataEditorSettingType .value span").valuesInlineEdit();


    jQuery(".valuesDataEditorSettingType .values").sortable({ 
			handle : '.handle', 
			update : function () { 
				
                valuesDataEditorSettingTypeResetValues();
				
			} 
  	});

});

function valuesDataEditorSettingTypeAddValue(sender) {

   
    var valuesDataEditorSettingType = jQuery(sender).parent();

    var value = jQuery(".valueInput", valuesDataEditorSettingType).val();

    if (value != "") {

        var values = jQuery(".values", valuesDataEditorSettingType);

        var newValue = "<div class='value'><span>" + value + "</span> <a class='del'>Delete</a> <a class='handle'>Drag</a></div>";

        values.append(newValue);

        jQuery(".valuesDataEditorSettingType .value span").valuesInlineEdit();

        jQuery(".valueInput", valuesDataEditorSettingType).val("");


        valuesDataEditorSettingTypeSetValues(values, jQuery(".valuesInput", valuesDataEditorSettingType));
    }


    jQuery(".valuesDataEditorSettingType .value .del").click(function () {
        jQuery(this).parent().remove();
        valuesDataEditorSettingTypeResetValues();
    });

    jQuery(".valuesDataEditorSettingType .values").sortable({ 
			handle : '.handle', 
			update : function () { 
				
                valuesDataEditorSettingTypeResetValues();
				
			} 
  	});

}

function valuesDataEditorSettingTypeSetValues(valuesContainer,valuesInput) {

    var vals = "";
    jQuery(".value", valuesContainer).each(function () {
 
        vals += jQuery("span",this).html() +  ";";
    });

    valuesInput.val(vals);
}

function valuesDataEditorSettingTypeResetValues() {
    jQuery(".valuesDataEditorSettingType").each(function () {

        valuesDataEditorSettingTypeSetValues(jQuery(".values", this), jQuery(".valuesInput", this));
    });
}



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
                                valuesDataEditorSettingTypeResetValues();
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