(function ($) {

  if (typeof window.myFlatpickrPlugins === 'undefined') {
    window.umbFlatpickrPlugins = [];
  }

  var predefinedRanges = function () {
    return function (fp) {
      const pluginData = {
        ranges: typeof fp.config.ranges !== 'undefined' ? fp.config.ranges : {},
        rangesOnly: typeof fp.config.rangesOnly === 'undefined' || fp.config.rangesOnly,
        rangesAllowCustom: typeof fp.config.rangesAllowCustom === 'undefined' || fp.config.rangesAllowCustom,
        rangesCustomLabel: typeof fp.config.rangesCustomLabel !== 'undefined' ? fp.config.rangesCustomLabel : 'Custom Range',
        rangesNav: $('<ul>').addClass('nav flex-column flatpickr-predefined-ranges'),
        rangesButtons: {}
      };

      /**
       * @param {string} label
       * @returns {jQuery}
       */
      const addRangeButton = function (label) {
        pluginData.rangesButtons[label] = $('<button>')
          .addClass('nav-link btn btn-link')
          .attr('type', 'button')
          .text(label);

        pluginData.rangesNav.append(
          $('<li>').addClass('nav-item d-grid').append(pluginData.rangesButtons[label])
        );
        return pluginData.rangesButtons[label];
      };

      /**
       * Loop the ranges and check for one that matches the selected dates, adding
       * an active class to its corresponding button.
       *
       * If there are selected dates and a range is not matched, check for a custom
       * range button and set it to active.
       *
       * If there are no selected dates or a range is not matched, check if the
       * rangeOnly option is true and if so hide the calendar.
       *
       * @param {Array} selectedDates
       */
      const selectActiveRangeButton = function (selectedDates) {
        let isPredefinedRange = false;
        pluginData.rangesNav.find('.active').removeClass('active');

        if (selectedDates.length > 0) {
          let startDate = moment(selectedDates[0]);
          let endDate = selectedDates.length > 1 ? moment(selectedDates[1]) : startDate;
          for (const [label, range] of Object.entries(pluginData.ranges)) {
            if (startDate.isSame(moment(range[0]), 'day') && endDate.isSame(moment(range[1]), 'day')) {
              pluginData.rangesButtons[label].addClass('active');
              isPredefinedRange = true;
              break;
            }
          }
        }

        if (selectedDates.length > 0 &&
          !isPredefinedRange &&
          pluginData.rangesButtons.hasOwnProperty(pluginData.rangesCustomLabel)
        ) {
          pluginData.rangesButtons[pluginData.rangesCustomLabel].addClass('active');
          $(fp.calendarContainer).removeClass('flatpickr-predefined-ranges-only');
        } else if (pluginData.rangesOnly) {
          $(fp.calendarContainer).addClass('flatpickr-predefined-ranges-only');
        }
      };

      return {
        /**
         * Loop the ranges and add buttons for each range which a click handler to set the date.
         * Also adds a custom range button if the rangesAllowCustom option is set to true.
         */
        onReady(selectedDates) {
          for (const [label, range] of Object.entries(pluginData.ranges)) {
            addRangeButton(label)
              .on('click', function () {
                $(this).blur();
                fp.setDate([moment(range[0]).toDate(), moment(range[1]).toDate()], true, );
                fp.close();
              });
          }

          if (pluginData.rangesNav.children().length > 0) {
            if (pluginData.rangesOnly && pluginData.rangesAllowCustom) {
              addRangeButton(pluginData.rangesCustomLabel)
                .on('click', function () {
                  $(this).blur();
                  pluginData.rangesNav.find('.active').removeClass('active');
                  $(this).addClass('active');
                  $(fp.calendarContainer).removeClass('flatpickr-predefined-ranges-only');
                });
            }

            $(fp.calendarContainer).prepend(pluginData.rangesNav);
            $(fp.calendarContainer).addClass('flatpickr-has-predefined-ranges');
            // make sure the right range button is active for the default value
            selectActiveRangeButton(selectedDates);
          }
        },

        /**
         * Make sure the right range button is active when a value is manually entered
         *
         * @param {Array} selectedDates
         */
        onValueUpdate(selectedDates) {
          selectActiveRangeButton(selectedDates);
        }
      };
    };
  }

  window.umbFlatpickrPlugins.push(new predefinedRanges());

})(jQuery);
