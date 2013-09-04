var contourFieldValues;

(function ($) {
    $.validator.setDefaults({
        ignore: ":hidden"
    });
    
    $.validator.unobtrusive.adapters.addBool("requiredcb", "required");

    $.validator.addMethod('contour_selectonefromlist', function (value, element) {
        var valid = false;
        $("input", $(element).closest(".checkboxlist")).each(function (i) {
            if ($(this).is(':checked')) { valid = true; }
        });
        return valid;
    });

    $.validator.unobtrusive.adapters.addBool("requiredlist", "contour_selectonefromlist");

    $.validator.addMethod('contour_regex', function (value, element) {

        var regex = $(element).attr("data-regex");
        var val = $(element).val();
        if (val.length == 0) { return true; }
        return val.match(regex);
    });

    $.validator.unobtrusive.adapters.addBool("regex", "contour_regex");

    $(function () {

        $("#PreviousClicked").val("");
        
        $(".datepickerfield").datepicker({ dateFormat: contourDateFormat });
        
        $(".cancel").click(function () {
            $("#PreviousClicked").val("clicked");
        });
        
        $(".contourPage input").change(function() {
  			PopulateFieldValues();
            CheckRules();
	    });
        
        $(".contourPage select").change(function () {
            PopulateFieldValues();
            CheckRules();
        });
        
        PopulateFieldValues();
        CheckRules();
    });
    
} (jQuery));

function PopulateFieldValues()
{
	//fill field values
	//contourFieldValues = new Array();
	PopulateRecordValues();

    $(".contourPage select").each(function() {
        contourFieldValues[$(this).attr("id")] = $("option[value='" + $(this).val() + "']", $(this)).text();
    });
    
	$(".contourPage input").each(function() {

	    
	    
	 	if($(this).attr('type') == "text"){
	 		contourFieldValues[$(this).attr("id")] = $(this).val();
	 	}

		if($(this).attr('type') == "radio"){
			if($(this).is(':checked'))
			{
				contourFieldValues[$(this).attr("name")] = $(this).val();
			}
		}

		if($(this).attr('type') == "checkbox"){

			if($(this).attr('id') != $(this).attr('name'))
			{
				if($(this).is(':checked'))
				{
					if(contourFieldValues[$(this).attr("name")] == null)
					{
                        contourFieldValues[$(this).attr("name")] = $(this).val();
					}
					else
					{
						contourFieldValues[$(this).attr("name")] += "," + $(this).val();
					}
				}
			} else {

			    contourFieldValues[$(this).attr("name")] = $(this).is(':checked').toString();
			}
		}

	});    
}