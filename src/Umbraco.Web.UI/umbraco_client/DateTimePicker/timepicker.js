/*!
 * jQuery UI Timepicker 0.2.1
 *
 * Copyright (c) 2009 Martin Milesich (http://milesich.com/)
 *
 * Some parts are
 *   Copyright (c) 2009 AUTHORS.txt (http://jqueryui.com/about)
 *
 * $Id: timepicker.js 28 2009-08-11 20:31:23Z majlo $
 *
 * Depends:
 *  ui.core.js
 *  ui.datepicker.js
 *  ui.slider.js
 */
(function($) {

/**
 * Extending default values
 */
$.extend($.datepicker._defaults, {
    'stepMinutes': 1, // Number of minutes to step up/down
    'stepHours': 1, // Number of hours to step up/down
    'time24h': false, // True if 24h time
    'showTime': false, // Show timepicker with datepicker
    'altTimeField': '' // Selector for an alternate field to store time into
});

/**
 * _hideDatepicker must be called with null
 */
$.datepicker._connectDatepickerOverride = $.datepicker._connectDatepicker;
$.datepicker._connectDatepicker = function(target, inst) {
    $.datepicker._connectDatepickerOverride(target, inst);

    // showButtonPanel is required with timepicker
    if (this._get(inst, 'showTime')) {
        inst.settings['showButtonPanel'] = true;
    }

    var showOn = this._get(inst, 'showOn');

    if (showOn == 'button' || showOn == 'both') {
        // Unbind all click events
        inst.trigger.unbind('click');

        // Bind new click event
        inst.trigger.click(function() {
            if ($.datepicker._datepickerShowing && $.datepicker._lastInput == target)
                $.datepicker._hideDatepicker(null); // This override is all about the "null"
            else
                $.datepicker._showDatepicker(target);
            return false;
        });
    }
};

/**
 * Datepicker does not have an onShow event so I need to create it.
 * What I actually doing here is copying original _showDatepicker
 * method to _showDatepickerOverload method.
 */
$.datepicker._showDatepickerOverride = $.datepicker._showDatepicker;
$.datepicker._showDatepicker = function (input) {
    // keep the current value
    var originalval = input.value;

    // Keep the first 10 chars for now yyyy-mm-dd - this removes the time part which was breaking the standardDatePicker parsing code
    input.value = originalval.length>10 ? originalval.substring(0, 10) : originalval;


    // Call the original method which will show the datepicker
    $.datepicker._showDatepickerOverride(input);

    // Put it back
    input.value = originalval;

    input = input.target || input;

    // find from button/image trigger
    if (input.nodeName.toLowerCase() != 'input') input = $('input', input.parentNode)[0];

    // Do not show timepicker if datepicker is disabled
    if ($.datepicker._isDisabledDatepicker(input)) return;

    // Get instance to datepicker
    var inst = $.datepicker._getInst(input);

    var showTime = $.datepicker._get(inst, 'showTime');

    // If showTime = True show the timepicker
    if (showTime) $.timepicker.show(input);
};

/**
 * Same as above. Here I need to extend the _checkExternalClick method
 * because I don't want to close the datepicker when the sliders get focus.
 */
$.datepicker._checkExternalClickOverride = $.datepicker._checkExternalClick;
$.datepicker._checkExternalClick = function (event) {
    if (!$.datepicker._curInst) return;
    var $target = $(event.target);

    if (($target.parents('#' + $.timepicker._mainDivId).length == 0)) {
        $.datepicker._checkExternalClickOverride(event);
    }
};

/**
 * Datepicker has onHide event but I just want to make it simple for you
 * so I hide the timepicker when datepicker hides.
 */
$.datepicker._hideDatepickerOverride = $.datepicker._hideDatepicker;
$.datepicker._hideDatepicker = function(input, duration) {
    // Some lines from the original method
    var inst = this._curInst;

    if (!inst || (input && inst != $.data(input, PROP_NAME))) return;

    // Get the value of showTime property
    var showTime = this._get(inst, 'showTime');

    if (input === undefined && showTime) {
        if (inst.input) {
            inst.input.val(this._formatDate(inst));
            inst.input.trigger('change'); // fire the change event
        }

        this._updateAlternate(inst);

        if (showTime) $.timepicker.update(this._formatDate(inst));
    }

    // Hide datepicker
    $.datepicker._hideDatepickerOverride(input, duration);

    // Hide the timepicker if enabled
    if (showTime) {
        $.timepicker.hide();
    }
};

/**
 * This is a complete replacement of the _selectDate method.
 * If showed with timepicker do not close when date is selected.
 */
$.datepicker._selectDate = function(id, dateStr) {
    var target = $(id);
    var inst = this._getInst(target[0]);
    var showTime = this._get(inst, 'showTime');
    dateStr = (dateStr != null ? dateStr : this._formatDate(inst));
    if (!showTime) {
        if (inst.input)
            inst.input.val(dateStr);
        this._updateAlternate(inst);
    }
    var onSelect = this._get(inst, 'onSelect');
    if (onSelect)
        onSelect.apply((inst.input ? inst.input[0] : null), [dateStr, inst]);  // trigger custom callback
    else if (inst.input && !showTime)
        inst.input.trigger('change'); // fire the change event
    if (inst.inline)
        this._updateDatepicker(inst);
    else if (!inst.stayOpen) {
        if (showTime) {
            this._updateDatepicker(inst);
        } else {
            this._hideDatepicker(null, this._get(inst, 'duration'));
            this._lastInput = inst.input[0];
            if (typeof(inst.input[0]) != 'object')
                inst.input[0].focus(); // restore focus
            this._lastInput = null;
        }
    }
};

/**
 * We need to resize the timepicker when the datepicker has been changed.
 */
$.datepicker._updateDatepickerOverride = $.datepicker._updateDatepicker;
$.datepicker._updateDatepicker = function(inst) {
    $.datepicker._updateDatepickerOverride(inst);
    $.timepicker.resize();
};

function Timepicker() {}

Timepicker.prototype = {
    init: function()
    {
        this._mainDivId = 'ui-timepicker-div';
        this._inputId   = null;
        this._orgValue  = null;
        this._orgHour   = null;
        this._orgMinute = null;
        this._colonPos  = -1;
        this._visible   = false;
        this.tpDiv      = $('<div id="' + this._mainDivId + '" class="ui-datepicker ui-widget ui-widget-content ui-helper-clearfix ui-corner-all ui-helper-hidden-accessible" style="width: 100px; display: none; position: absolute;"></div>');
        this._generateHtml();
    },

    show: function (input)
    {
        // Get instance to datepicker
        var inst = $.datepicker._getInst(input);

        this._time24h = $.datepicker._get(inst, 'time24h');
        this._altTimeField = $.datepicker._get(inst, 'altTimeField');

        var stepMinutes = parseInt($.datepicker._get(inst, 'stepMinutes'), 10) || 1;
        var stepHours   = parseInt($.datepicker._get(inst, 'stepHours'), 10)   || 1;

        if (60 % stepMinutes != 0) { stepMinutes = 1; }
        if (24 % stepHours != 0)   { stepHours   = 1; }

        $('#hourSlider').slider('option', 'max', 24 - stepHours);
        $('#hourSlider').slider('option', 'step', stepHours);

        $('#minuteSlider').slider('option', 'max', 60 - stepMinutes);
        $('#minuteSlider').slider('option', 'step', stepMinutes);

        this._inputId = input.id;

        if (!this._visible) {
            this._parseTime();
            this._orgValue = $('#' + this._inputId).val();
        }

        this.resize();

        $('#' + this._mainDivId).show();

        this._visible = true;

        var dpDiv     = $('#' + $.datepicker._mainDivId);
        var dpDivPos  = dpDiv.position();

        var viewWidth = (window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth) + $(document).scrollLeft();
        var tpRight   = this.tpDiv.offset().left + this.tpDiv.outerWidth();

        if (tpRight > viewWidth) {
            dpDiv.css('left', dpDivPos.left - (tpRight - viewWidth) - 5);
            this.tpDiv.css('left', dpDiv.offset().left + dpDiv.outerWidth() + 'px');
        }
    },

    update: function (fd)
    {
        var curTime = $('#' + this._mainDivId + ' span.fragHours').text()
                    + ':'
                    + $('#' + this._mainDivId + ' span.fragMinutes').text();

        if (!this._time24h) {
            curTime += ' ' + $('#' + this._mainDivId + ' span.fragAmpm').text();
        }

        var curDate = $('#' + this._inputId).val();

        $('#' + this._inputId).val(fd + ' ' + curTime);

        if (this._altTimeField) {
            $(this._altTimeField).each(function() { $(this).val(curTime); });
        }
    },

    hide: function ()
    {
        this._visible = false;
        $('#' + this._mainDivId).hide();
    },

    resize: function ()
    {
        var dpDiv = $('#' + $.datepicker._mainDivId);
        var dpDivPos = dpDiv.position();

        var hdrHeight = $('#' + $.datepicker._mainDivId +  ' > div.ui-datepicker-header:first-child').height();

        $('#' + this._mainDivId + ' > div.ui-datepicker-header:first-child').css('height', hdrHeight);

        this.tpDiv.css({
            'height': dpDiv.height(),
            'top'   : dpDivPos.top,
            'left'  : dpDivPos.left + dpDiv.outerWidth() + 'px'
        });

        $('#hourSlider').css('height',   this.tpDiv.height() - (3.5 * hdrHeight));
        $('#minuteSlider').css('height', this.tpDiv.height() - (3.5 * hdrHeight));
    },

    _generateHtml: function ()
    {
        var html = '';

        html += '<div class="ui-datepicker-header ui-widget-header ui-helper-clearfix ui-corner-all">';
        html += '<div class="ui-datepicker-title" style="margin:0">';
        html += '<span class="fragHours">08</span><span class="delim">:</span><span class="fragMinutes">45</span> <span class="fragAmpm"></span></div></div><table>';
        html += '<tr><th>Hour</th><th>Minute</th></tr>';
        html += '<tr><td align="center"><div id="hourSlider" class="slider"></div></td><td align="center"><div id="minuteSlider" class="slider"></div></td></tr>';
        html += '</table>';

        this.tpDiv.empty().append(html);
        $('body').append(this.tpDiv);

        var self = this;

        $('#hourSlider').slider({
            orientation: "vertical",
            range: 'min',
            min: 0,
            max: 23,
            step: 1,
            slide: function(event, ui) {
                self._writeTime('hour', ui.value);
            },
            stop: function(event, ui) {
                $('#' + self._inputId).focus();
            }
        });

        $('#minuteSlider').slider({
            orientation: "vertical",
            range: 'min',
            min: 0,
            max: 59,
            step: 1,
            slide: function(event, ui) {
                self._writeTime('minute', ui.value);
            },
            stop: function(event, ui) {
                $('#' + self._inputId).focus();
            }
        });

        $('#hourSlider > a').css('padding', 0);
        $('#minuteSlider > a').css('padding', 0);
    },

    _writeTime: function (type, value)
    {
        if (type == 'hour') {
            if (!this._time24h) {
                if (value < 12) {
                    $('#' + this._mainDivId + ' span.fragAmpm').text('am');
                } else {
                    $('#' + this._mainDivId + ' span.fragAmpm').text('pm');
                    value -= 12;
                }

                if (value == 0) value = 12;
            } else {
                $('#' + this._mainDivId + ' span.fragAmpm').text('');
            }

            if (value < 10) value = '0' + value;
            $('#' + this._mainDivId + ' span.fragHours').text(value);
        }

        if (type == 'minute') {
            if (value < 10) value = '0' + value;
            $('#' + this._mainDivId + ' span.fragMinutes').text(value);
        }
    },

    _parseTime: function ()
    {
        var dt = $('#' + this._inputId).val();

        this._colonPos = dt.search(':');

        var m = 0, h = 0, a = '';

        if (this._colonPos != -1) {
            h = parseInt(dt.substr(this._colonPos - 2, 2), 10);
            m = parseInt(dt.substr(this._colonPos + 1, 2), 10);
            a = jQuery.trim(dt.substr(this._colonPos + 3, 3));
        }

        a = a.toLowerCase();

        if (a != 'am' && a != 'pm') {
            a = '';
        }

        if (h < 0) h = 0;
        if (m < 0) m = 0;

        if (h > 23) h = 23;
        if (m > 59) m = 59;

        if (a == 'pm' && h  < 12) h += 12;
        if (a == 'am' && h == 12) h  = 0;

        this._setTime('hour',   h);
        this._setTime('minute', m);

        this._orgHour   = h;
        this._orgMinute = m;
    },

    _setTime: function (type, value)
    {
        if (isNaN(value)) value = 0;
        if (value < 0)    value = 0;
        if (value > 23 && type == 'hour')   value = 23;
        if (value > 59 && type == 'minute') value = 59;

        if (type == 'hour') {
            $('#hourSlider').slider('value', value);
        }

        if (type == 'minute') {
            $('#minuteSlider').slider('value', value);
        }

        this._writeTime(type, value);
    }
};

$.timepicker = new Timepicker();
$('document').ready(function () {$.timepicker.init();});

})(jQuery);
