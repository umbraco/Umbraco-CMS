		var activeDragId = "";
		function expandCollapse(theId) {
		    
		    var edit = document.getElementById("edit" + theId);
		   
		    if (edit.style.display == 'none') {
		        edit.style.display = 'block';
		        document.getElementById("desc" + theId).style.display = 'none';
		    }
		    else {
		        edit.style.display = 'none';
		        document.getElementById("desc" + theId).style.display = 'block';
		    }
		}
