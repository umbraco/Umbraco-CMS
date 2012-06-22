function umbracoUpdateDatePicker(fieldName) {
	var newDate = '';
	var theForm = document.forms[0];

	
	// vi løber igennem de forskellige items og ser om der er noget i dem
	if (theForm[fieldName+'_days'].selectedIndex > 0 
			&& theForm[fieldName+'_months'].selectedIndex > 0
				&& theForm[fieldName+'_years'].selectedIndex > 0)
			newDate = 	theForm[fieldName+'_days'][theForm[fieldName+'_days'].selectedIndex].value + ' ' +
						theForm[fieldName+'_months'][theForm[fieldName+'_months'].selectedIndex].value + ' ' + 
						theForm[fieldName+'_years'][theForm[fieldName+'_years'].selectedIndex].value
						
	// vi skal lige se om der også er sat klokkeslet
	if (theForm[fieldName+'_hours']) {
		if (theForm[fieldName+'_hours'].selectedIndex > 0 
				&& theForm[fieldName+'_minutes'].selectedIndex > 0)
			newDate += ' ' + theForm[fieldName+'_hours'][theForm[fieldName+'_hours'].selectedIndex].value + ':' +
						theForm[fieldName+'_minutes'][theForm[fieldName+'_minutes'].selectedIndex].value
	}	
	
	theForm[fieldName].value = newDate;
}
