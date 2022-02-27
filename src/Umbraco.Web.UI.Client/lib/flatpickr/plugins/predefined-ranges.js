(function () {
  'use strict';

  if (typeof window.umbFlatpickrPlugins === 'undefined') {
    window.umbFlatpickrPlugins = [];
  }

  var predefinedRanges = function () {
    return function (fp) {

      let rangesNav = document.createElement('ul');
      rangesNav.className = "nav flex-column flatpickr-predefined-ranges";

      const pluginData = {
        ranges: typeof fp.config.ranges !== 'undefined' ? fp.config.ranges : {},
        rangesOnly: typeof fp.config.rangesOnly === 'undefined' || fp.config.rangesOnly,
        rangesAllowCustom: typeof fp.config.rangesAllowCustom === 'undefined' || fp.config.rangesAllowCustom,
        rangesCustomLabel: typeof fp.config.rangesCustomLabel !== 'undefined' ? fp.config.rangesCustomLabel : 'Custom Range',
        rangesNav: rangesNav,
        rangesButtons: {}
      };

      /**
       * @param {string} label
       * @returns HTML Element
       */
      const addRangeButton = function (label) {

        let button = document.createElement('button');
        button.type = "button";
        button.className = "nav-link btn btn-link";
        button.innerText = label;

        pluginData.rangesButtons[label] = button;

        let item = document.createElement('li');
        item.className = "nav-item d-grid";

        item.appendChild(pluginData.rangesButtons[label]);

        pluginData.rangesNav.appendChild(item);

        console.log("pluginData.rangesNav", pluginData.rangesNav);
        console.log("rangeButton", pluginData.rangesButtons[label]);

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
        let current = pluginData.rangesNav.querySelector('.active');

        if (current) {
          current.classList.remove('active');
        }

        if (selectedDates.length > 0) {
          let startDate = moment(selectedDates[0]);
          let endDate = selectedDates.length > 1 ? moment(selectedDates[1]) : startDate;
          for (const [label, range] of Object.entries(pluginData.ranges)) {
            if (startDate.isSame(moment(range[0]), 'day') && endDate.isSame(moment(range[1]), 'day')) {
              pluginData.rangesButtons[label].classList.add('active');
              isPredefinedRange = true;
              break;
            }
          }
        }

        if (selectedDates.length > 0 &&
          !isPredefinedRange &&
          pluginData.rangesButtons.hasOwnProperty(pluginData.rangesCustomLabel)
        ) {
          pluginData.rangesButtons[pluginData.rangesCustomLabel].classList.add('active');
          fp.calendarContainer.classList.remove('flatpickr-predefined-ranges-only');
        } else if (pluginData.rangesOnly) {
          fp.calendarContainer.classList.add('flatpickr-predefined-ranges-only');
        }
      };

      return {
        /**
         * Loop the ranges and add buttons for each range which a click handler to set the date.
         * Also adds a custom range button if the rangesAllowCustom option is set to true.
         */
        onReady(selectedDates) {
          for (const [label, range] of Object.entries(pluginData.ranges)) {
            addRangeButton(label).addEventListener('click', function() {
                this.blur();
                fp.setDate([moment(range[0]).toDate(), moment(range[1]).toDate()], true,);
                fp.close();
              });
          }

          if (pluginData.rangesNav.children.length > 0) {
            if (pluginData.rangesOnly && pluginData.rangesAllowCustom) {
              addRangeButton(pluginData.rangesCustomLabel).addEventListener('click', function () {
                  this.blur();
                  pluginData.rangesNav.querySelector('.active').classList.remove('active');
                  this.classList.add('active');
                  fp.calendarContainer.classList.remove('flatpickr-predefined-ranges-only');
                });
            }

            fp.calendarContainer.prepend(pluginData.rangesNav);
            fp.calendarContainer.classList.add('flatpickr-has-predefined-ranges');
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

})();
