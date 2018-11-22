(function ($) {
	// jquery plugin for the multiple textstring
	$.fn.MultipleTextstring = function (hiddenId, minimum, maximum) {
		var $this = $(this),
			$hidden = $(hiddenId),
			$inputs = $this.find('.umbEditorTextField');

		$this.sortable({
			axis: 'y',
			containment: $this.closest('.propertyItemContent'),
			items: '.textstring-row',
			handle: '.textstring-row-sort',
			forcePlaceholderSize: true,
			placeholder: 'textstring-row-placeholder',
			stop: function (event, ui) {
				// re-populate the hidden field
				populateHiddenField();
			}
		});

		$this.find('.add_row').click(function () {
			var $parent = $(this).parent().parent();
			var $row = $parent.clone(true); // clone the row
			var $input = $row.find('.umbEditorTextField');

			if ($inputs.length < maximum || maximum <= 0) {

				// clear the text field
				$input.val('');

				// append the new row
				$row.insertAfter($parent);

				// set the focus
				$input.focus();

				// re-populate the hidden field
				populateHiddenField();
			}

			return false;
		});

		$this.find('.remove_row').click(function () {

			// make sure the user wants to remove the row
			if (confirm('Are you sure you want to delete this row?')) {

				//var $input = $this.find('.umbEditorTextField');

				// check if this is the last row...
				if ($inputs.length == 1) {

					// ... if so, just clear it.
					$inputs.val('').focus();

				} else if ($inputs.length > minimum) {

					var $parent = $(this).parent().parent();

					// set the focus
					$parent.prev().find('.umbEditorTextField').focus();

					// remove the row
					$parent.remove();
				}

				// re-populate the hidden field
				populateHiddenField();
			}

			return false;
		});

		$inputs.blur(function () {
			// re-populate the hidden field
			populateHiddenField();
		});

		$inputs.keydown(function (e) {
			var keyCode = e.keyCode || e.which;

			// if ENTER is pressed
			if (keyCode == 13) {

				e.preventDefault();

				// add a new row
				return $(this).parent().find('.add_row').click();
			}

			// if BACKSPACE if pressed and the textstring value is empty
			if (keyCode == 8 && $(this).val() == '') {

				e.preventDefault();

				// remove the row
				return $(this).parent().find('.remove_row').click();
			}

		});

		function populateHiddenField() {
			var values = []; // initialise an array of values

			// re-bind the text inputs
			$inputs = $this.find('.umbEditorTextField');

			// loop through each of the testxtring elements (needs to be a live query)
			$inputs.each(function () {

				// add the value to the array
				values.push(this.value);

			});

			// implode the array into the hidden field
			$hidden.val(values.join('\n'));
		}

	}
})(jQuery);