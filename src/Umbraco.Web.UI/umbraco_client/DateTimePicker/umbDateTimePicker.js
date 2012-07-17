(function($) {

    $.fn.umbDateTimePicker = function(showTime, chooseDateTxt, noDateTxt, removeDateTxt) {
        return $(this).each(function() {
            //create the date/time picker
            $(this).datepicker({
                duration: "",
                showTime: showTime,
                constrainInput: true,
                buttonText: "<span>" + chooseDateTxt + "</span>",
                showOn: 'button',
                changeYear: true,
                dateFormat: 'yy-mm-dd',
                time24h: true,
                onClose: function(dateText, inst) { if (dateText == '') return; $(this).nextAll('div').remove(); }
            });
            //simple method to create the no date selected text block
            var addNoDate = function(obj) {
                if (obj.siblings('div').length == 0) {
                    obj.siblings('button').after('<div>' + noDateTxt + '</div>');
                }
                obj.nextAll('a').remove();
            }
            //simple method to handle the clear date button click
            var clearDate = function() {
                $(this).siblings('input').val('');
                addNoDate($(this));
                $(this).remove();
            }
            //wire up the textbox event, we'll create/remove items when it has values or not.
            $(this).change(function() {
                if ($(this).val() == '') {
                    addNoDate($(this));
                }
                else {
                    if ($(this).nextAll('a').length == 0) {
                        $('<a>' + removeDateTxt + '</a>').insertAfter($(this).nextAll('button')).click(clearDate);
                    }
                    $(this).nextAll('div').remove();
                }
            });
            //wire up anchor click
            $(this).nextAll('a').click(clearDate);
        });
    };

})(jQuery);