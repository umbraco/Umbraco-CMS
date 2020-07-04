
function dualSelectBoxShift(id) {
    var posVal = document.getElementById(id + "_posVals");
	var selVal = document.getElementById(id + "_selVals");
	    	
	// First check the possible items
	for (var i=0;i<posVal.options.length;i++) {
		if (posVal.options[i].selected) {
			var selNew = document.createElement('option');
			selNew.text = posVal[i].text;
			selNew.value = posVal[i].value;
			try {
				selVal.add(selNew, null);
			}
			catch(ex) {
				selVal.add(selNew);
			}
			posVal.remove(i);
			i--;
		}
	}
	
	// do the same with the selected items, to return them
	for (var i=0;i<selVal.options.length;i++) {
		if (selVal.options[i].selected) {
			var selNew = document.createElement('option');
			selNew.text = selVal[i].text;
			selNew.value = selVal[i].value;
			try {
				posVal.add(selNew, null);
			}
			catch(ex) {
				posVal.add(selNew);
			}
			selVal.remove(i);
			i--;
		}
	}
	
	// update hidden value field with all values
	var hiddenVal = "";
	for (var i=0;i<selVal.options.length;i++) {
		hiddenVal += selVal.options[i].value + ",";
	}
	if (hiddenVal != "")
		hiddenVal = hiddenVal.substring(0, hiddenVal.length-1);
	document.getElementById(id + "_theValue").value = hiddenVal;
}