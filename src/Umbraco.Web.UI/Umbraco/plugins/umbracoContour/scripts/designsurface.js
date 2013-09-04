var DesignSurface = function() {
  
  this.addEventListener = function(eventName, id, listener) {
      var success = true;
      if (!this.Event[eventName])
        success = false;
      else
        this.Event[eventName][id] = listener;
      return success;
    };
  
    this.removeEventListener = function(eventName, id) {
      delete this.Event[eventName][id];
    };
  
    this.fireEvent = function(eventName) {
      var ev   = this.Event[eventName],
          args = Array.prototype.slice.call(arguments);
      args.shift();
      for (id in ev) {
        ev[id].apply(this, args);
      }
  };
  
  this.Event = [];
  this.Event['AddPage'] = [];
  this.Event['DeletePage'] = [];
  this.Event['UpdatePage'] = [];
  this.Event['UpdatePageSortOrder'] = [];
  this.Event['AddFieldset'] = [];
  this.Event['DeleteFieldset'] = [];
  this.Event['UpdateFieldset'] = [];
  this.Event['UpdateFieldsetSortOrder'] = [];
  this.Event['AddField'] = [];
  this.Event['DeleteField'] = [];
  this.Event['UpdateField'] = [];
  this.Event['UpdateFieldSortOrder'] = [];
  this.Event['AddPrevalue'] = [];
  this.Event['DeletePrevalue'] = [];
  
  
  this.AddPage = function(Id, Name)
  {
  	this.fireEvent('AddPage', Id, Name);
  };
  
  this.DeletePage = function(Id)
  {
    	this.fireEvent('DeletePage', Id);
  };
  
  this.UpdatePage = function(Id, Name)
  {
      	this.fireEvent('UpdatePage', Id, Name);
  };
  
  this.UpdatePageSortOrder = function(Sortorder)
  {
        this.fireEvent('UpdatePageSortOrder', Sortorder);
  };
  
  this.AddFieldset = function(PageId, Id, Name)
  {
  	this.fireEvent('AddFieldset', PageId, Id, Name);
  };
  
  this.DeleteFieldset = function(Id)
  {
    	this.fireEvent('DeleteFieldset', Id);
  };
  
  this.UpdateFieldsetSortOrder = function(Id, Sortorder)
  {
      	this.fireEvent('UpdateFieldsetSortOrder', Id, Sortorder);
  };  
  
  this.UpdateFieldset = function(Id, Name)
  {
        	this.fireEvent('UpdateFieldset', Id, Name);
  }; 
  
  this.AddField = function(FieldsetId, Id, Name, Type, Mandatory, Regex)
  {
  	this.fireEvent('AddField', FieldsetId, Id, Name, Type, Mandatory, Regex);
  };
  
  this.DeleteField = function(Id)
  {
    	this.fireEvent('DeleteField', Id);
  };
  
  this.UpdateField = function(Id, Name, Type, Mandatory, Regex)
  {
      	this.fireEvent('UpdateField', Id, Name, Type, Mandatory, Regex);
  };   
  
  this.UpdateFieldSortOrder = function(Id, Sortorder)
  {
        	this.fireEvent('UpdateFieldSortOrder', Id, Sortorder);
  };
}



var myDesignSurface = new DesignSurface();

myDesignSurface.addEventListener("AddPage", "addpage", function(id,name) {
  //alert("Added page " + id + " name:" + name);
});

myDesignSurface.addEventListener("DeletePage", "deletepage", function(id) {
  //alert("Deleted page " + id);
});

myDesignSurface.addEventListener("UpdatePage", "updatepage", function(id,name) {
  //alert("Updated page " + id + " to: " + name);
});

myDesignSurface.addEventListener("UpdatePageSortOrder", "updatepagesortorder", function(sortorder) {
  //alert("Updated page sortorder " + sortorder);
});

myDesignSurface.addEventListener("AddFieldset", "addfieldset", function(pageid,id,name) {
  //alert("Added fieldset " + id + " name: " + name + " to page: " + pageid);
});

myDesignSurface.addEventListener("DeleteFieldset", "deletefieldset", function(id) {
  //alert("Deleted fieldset " + id);
});

myDesignSurface.addEventListener("UpdateFieldset", "updatefieldset", function(id,name) {
  //alert("Updated fieldset " + id + " to: " + name);
});

myDesignSurface.addEventListener("UpdateFieldsetSortOrder", "updatefieldsetsortorder", function(id,sortorder) {
  //alert("Updated fieldset sortorder " + id + " : " + sortorder);
});


myDesignSurface.addEventListener("AddField", "addfield", function(fieldsetid,id,name,type,mandatory,regex) {
  //alert("Added field " + id + " name:" + name + " type: " + type + " mandatory: " + mandatory + " regex: " + regex);
});

myDesignSurface.addEventListener("DeleteField", "deletefield", function(id) {
  //alert("Deleted field " + id);
});

myDesignSurface.addEventListener("UpdateField", "updatefield", function(id,name,type,mandatory,regex) {
  //alert("Updated field " + id + " to: name:" + name + " type: " + type + " mandatory: " + mandatory + " regex: " + regex);
});


myDesignSurface.addEventListener("UpdateFieldSortOrder", "updatefieldsortorder", function(id,sortorder) {
  //alert("Updated field sortorder " + id + " : " + sortorder);
});

//Additional Validator methods

jQuery.validator.addMethod(
  "selectNone",
  function(value, element) {
      if (element.value == "none") {
          return false;
      }
      else return true;
  },
  "Please select an option."
);


$(document).ready(function() {

    $("#designsurface").sortable({
        handle: '.handle',
        update: function() {

            var sortorder = $('#designsurface').sortable('serialize');
            myDesignSurface.UpdatePageSortOrder(sortorder);
        }
    });


    $("#AddPage").click(function() {

        /* Set Captions */
        $('#PageModal h1').text(lang_addpage);
        $('#PageModal #pageform .submit').attr('value', lang_addpage);

        /* Clear Fields */
        $('#NewPageName').val('');

        /* Show Modal */
        $('#PageModal').modal();

        /* Set Focus */
        $('#NewPageName').focus();

        $("#pageform").validate({ submitHandler: function() {
            AddPage();

        }
        });

    });

    ToggleAddFieldsetAction();

    $(".CancelModal").click(function() {
        $.modal.close();
    });

    $("#addprevalue").click(function() {

        AddPrevalue();
    });



    $("#fieldtype").change(function() {
        if ($("#fieldtype option:selected").attr('prevalues') == "1") {
            if (!($('#fieldprevalues').is(':visible'))) {
                $("#fieldprevalues").show('blind', {}, 500);

                $("#fieldprevalueslist").sortable({
                    update: function() {

                        //var sortorder = $('#designsurface').sortable('serialize');
                        //myDesignSurface.UpdatePageSortOrder(sortorder);
                    }
                });
            }
        }
        else {
            if ($('#fieldprevalues').is(':visible')) {
                $("#fieldprevalues").hide('blind', {}, 500);
            }
        }
    });
    
    
   


});

function ToggleAddFieldsetAction() {

    $("#AddFieldset").unbind('click'); 
    
    if ($('#designsurface').children().size() > 0) {
        $("#AddFieldset").removeClass("disabled");
        $("#AddFieldset").click(function() {
            ShowAddFieldsetDialog(null);
        });
    }
    else {

        $("#AddFieldset").addClass("disabled");
        
        $("#AddFieldset").click(function(e) {
            e.preventDefault();            
        });
        
    }
}


function AddPage()
{		
	var pagenumber = $('#designsurface').children().size() + 1; 
	var pagename = 	$("#NewPageName").val();
	var pageid = "page_" + pagenumber;
	
	$("#designsurface").append("<div class='page' id='"+pageid+"'><strong class='pagename'>"+ pagename +"</strong> <a href='javascript:ShowAddFieldsetDialog(\""+pageid+"\");'>add fieldset</a> <a href='javascript:ShowUpdatePageDialog(\""+pageid+"\");'>"+lang_update+"</a> <a href='javascript:DeletePage(\""+pageid+"\");' class='delete'>"+lang_delete+"</a> <span class='handle'>handle</span> <div class='fieldsetcontainer' id='fieldsetcontainerpage_"+pagenumber+"'></div></div>");
	
	$(".fieldsetcontainer").sortable({ 
			connectWith: '.fieldsetcontainer',
			handle : '.handle', 
			update : function () { 
				var sortorder = $('#fieldsetcontainer' + pageid).sortable('serialize');
				myDesignSurface.UpdateFieldsetSortOrder(pageid, sortorder);
			} 
  	});

  	ToggleAddFieldsetAction();
  	
	$.modal.close();
	
	myDesignSurface.AddPage(pageid,pagename);
	
}

function ShowUpdatePageDialog(page)
{
	/* Set Captions */
	$('#PageModal h1').text(lang_updatepage);
	$('#PageModal #pageform .submit').attr('value',lang_updatepage);
	
	/* Set Current Values */
	$('#NewPageName').val($('#' + page + ' .pagename').text());
	
	$('#PageModal').modal();
	
	/* Set Focus */
	$('#NewPageName').focus();
		
	$("#pageform").validate({submitHandler: function() { 
		UpdatePage(page);
	            		
        }});
}

function UpdatePage(page)
{
	var newname = $('#NewPageName').val();
	
	$('#' + page + ' .pagename').text(newname);
	
	$.modal.close();
	
	myDesignSurface.UpdatePage(page,newname);
}

function DeletePage(page)
{
	if(ConfirmDelete())
	{
	    $("#" + page).hide('blind', {}, 500, function() {
	        $("#" + page).remove();
	        ToggleAddFieldsetAction();
	        myDesignSurface.DeletePage(page);
	    });
		
		
	}
}


function ConfirmDelete()
{
      if (confirm(lang_deleteconfirm)==true)
        return true;
      else
        return false;
}

function ShowAddFieldsetDialog(page)
{	
	/* Set Captions */
	$('#FieldsetModal h1').text(lang_addfieldset);
	$('#FieldsetModal #fieldsetform .submit').attr('value',lang_addfieldset);
			
	/* Clear Fields */
	$('#NewFieldsetName').val('');
	
	$('#FieldsetModal').modal();
	
	/* Set Focus */
	$('#NewFieldsetName').focus();

	$(".page").each(function() {
	    var text = $(this).children(".pagename").text();
	    var value = $(this).attr("id");
	    $("#pageselect").append($("<option></option>").attr("value", value).text(text));
	});

	if (page != null) {
	    $("#pageselect").val(page);
	}
	$("#fieldsetform").validate({submitHandler: function() { 
		AddFieldset();
	            		
	}});
	
}

function AddFieldset()
{

    var page = $("#pageselect").val();
   
    var fieldsetnumber = $('#' + page + ' .fieldsetcontainer').children().size() + 1;

    var fieldsetcontainer = '#' + page + ' .fieldsetcontainer';

    var newfieldsetid = page.replace('_', '') + "fieldset_" + fieldsetnumber;
	
	var fieldsetname = $("#NewFieldsetName").val();
	
	$(fieldsetcontainer).append("<div class='fieldset' id='"+newfieldsetid+"'><strong class='fieldsetname'>"+ $("#NewFieldsetName").val() +"</strong> <a href='javascript:ShowAddFieldDialog(\""+newfieldsetid+"\");'>add field</a> <a href='javascript:ShowUpdateFieldsetDialog(\""+newfieldsetid+"\");'>"+lang_update+"</a> <a href='javascript:DeleteFieldset(\""+newfieldsetid+"\");' class='delete'>"+lang_delete +"</a> <span class='handle'>handle</span> <div class='fieldcontainer'></div></div>");
	

		
	
	$(".fieldcontainer").sortable({ 
			connectWith: '.fieldcontainer',
			handle : '.handle', 
			update : function () { 
				//$('#designsurface').sortable('serialize'); 
				//alert('field sort order updated');
				myDesignSurface.UpdateFieldSortOrder("todo","todo");
			} 
  	}); 
  	 	
  	$.modal.close();
  	
  	myDesignSurface.AddFieldset(page,newfieldsetid,fieldsetname);
}	

function ShowUpdateFieldsetDialog(fieldset)
{
	/* Set Captions */
	$('#FieldsetModal h1').text(lang_updatefieldset);
	$('#FieldsetModal #fieldsetform .submit').attr('value',lang_updatefieldset);
	
	/* Set Current Values */
	$('#NewFieldsetName').val($('#' + fieldset + ' .fieldsetname').text());
	
	$('#FieldsetModal').modal();
	
	/* Set Focus */
	$('#NewFieldsetName').focus();
	
	$("#fieldsetform").validate({submitHandler: function() { 
		UpdateFieldset(fieldset);
	            		
        }});
}

function UpdateFieldset(fieldset)
{
	var newname = $('#NewFieldsetName').val();
		
	$('#' + fieldset+ ' .fieldsetname').text(newname);
		
	$.modal.close();
	
	myDesignSurface.UpdateFieldset(fieldset,newname);
}

function DeleteFieldset(fieldset)
{
	if(ConfirmDelete())
	{
		$("#" +fieldset).hide('blind',{},500,function() {
				$("#" +fieldset).remove();
				myDesignSurface.DeleteFieldset(fieldset);
			});
		
		
	}
}


function ShowAddFieldDialog(fieldset)
{	
	/* Set Captions */
	$('#FieldModal h1').text(lang_addfield);
	$('#FieldModal #fieldform .submit').attr('value',lang_addfield);
	
	/* Clear Fields */
	$('#fieldcaption').val('');
	$('#fieldtype').selectedIndex = -1;
	$('#fieldmandatory').attr('checked', false);
	$('#fieldregex').val('');
	$('#fieldprevalueslist').children().remove();
	$('#FieldModal').modal();
	
	/* Set Focus */
	$('#fieldcaption').focus();
	
	$('#fieldsetid').text(fieldset);
	
	$("#fieldform").validate({submitHandler: function() { 
		AddField();
		            		
	}});
}

function AddField()
{

	var fieldsetid = $('#fieldsetid').text();
	var fieldcaption = $("#fieldcaption").val();
	var fieldtype = $("#fieldtype").val();
	var fieldmandatory = $("#fieldmandatory").attr('checked');
  	var fieldregex = $("#fieldregex").val();
  	
	var fieldnumber = $('#' + $('#fieldsetid').text() + ' .fieldcontainer').children().size() + 1; 
	
	var fieldcontainer = '#' + $('#fieldsetid').text() + ' .fieldcontainer';
	
	var newfieldid = $('#fieldsetid').text().replace('_','')+"field_"+fieldnumber;
	
	//var examplefield = "<p><label for='"+$('#fieldsetid').text()+'field'+fieldnumber+"example'>"+$("#fieldcaption").val()+"</label><input type='text' id='"+$('#fieldsetid').text()+'field'+fieldnumber+"example'/></p>";
	
	var examplefield = GetFieldPreview(fieldtype).replace("{caption}",fieldcaption);
	
	$(fieldcontainer).append("<div class='field' id='"+newfieldid+"'> <a href='javascript:ShowUpdateFieldDialog(\""+newfieldid+"\");'>"+lang_update+"</a> <a href='javascript:DeleteField(\""+newfieldid+"\");' class='delete'>"+lang_delete+"</a> <span class='handle'>handle</span> <div class='fieldprevalues' style='display:none'></div><div class='fieldexample'></div></div>");
	
	$("#"+newfieldid).attr('fieldcaption',fieldcaption);
	$("#"+newfieldid).attr('fieldtype',fieldtype);
	$("#"+newfieldid).attr('fieldmandatory',fieldmandatory);
	$("#"+newfieldid).attr('fieldregex',fieldregex);
	
	//does it have prevalues
	if($("#fieldtype option:selected").attr('prevalues') == "1")
	{
	    $("#"+newfieldid).attr('fieldhasprevalues','1');
	}
	else
	{
	     $("#"+newfieldid).attr('fieldhasprevalues','0');
    	}
    	
	myDesignSurface.AddField(fieldsetid,newfieldid,fieldcaption,fieldtype,fieldmandatory,fieldregex);


	var prevaluecontainer = '#' + newfieldid + ' .fieldprevalues';
	
	var previewprevalue = "";
	
	$("#fieldprevalueslist").children().each(function() {
		var child = $(this);
		
		var prevalue = child.children(":first-child").text();
		var prevalueid = child.attr("id");
		
		$(prevaluecontainer).append("<div id='"+prevalueid+"'>"+prevalue+"</div>");
		
		previewprevalue += GetFieldPrevaluePreview(fieldtype).replace("{caption}",prevalue);
		//todo
		//myDesignSurface.AddPrevalue(fieldid,prevalueid,value);
		
	});
	
	examplefield = examplefield.replace("{prevalues}",previewprevalue);
	
	
	$("#"+newfieldid + " .fieldexample").append(examplefield);
	
	$.modal.close();
	
	
  	
  			
  	
  	
}


function ShowUpdateFieldDialog(field)
{
	/* Set Captions */
	$('#FieldModal h1').text(lang_updatefield);
	$('#FieldModal #fieldform .submit').attr('value',lang_updatefield);
	
	/* Set Current Values */
	var fieldcaption = $("#"+field).attr('fieldcaption');
	$("#fieldcaption").val(fieldcaption);
	
	var fieldtype = $("#"+field).attr('fieldtype');
	$("#fieldtype").val(fieldtype);
	
	var fieldmandatory = $("#"+field).attr('fieldmandatory');
	if(fieldmandatory == 'true' || fieldmandatory == '1')
	{
		$("#fieldmandatory").attr('checked',  true);
	}
	else
	{
		$("#fieldmandatory").attr('checked',  false);
	}
	
	var fieldregex = $("#"+field).attr('fieldregex');
	$("#fieldregex").val(fieldregex);
	
	$('#FieldModal').modal();

	/* Prevalues */
	$('#fieldprevalueslist').children().remove();
	if($("#"+field).attr('fieldhasprevalues') == "1")
	{	
		//Show prevalue part
		$("#fieldprevalues").show();
		
		//Populate prevalues
		
		$('#' + field + " .fieldprevalues").children().each(function() {
			child = $(this);
			
			var prevalue =child.text();
			var prevalueid = "dialog" + child.attr("id");
			
			$("#fieldprevalueslist").append("<li id='"+prevalueid+"'><span class='prevaluetext'>" + prevalue  + "</span> <a href='javascript:DeletePrevalue(\""+prevalueid+"\");' class='delete'>"+lang_delete+"</a></li>");
		});
		
		
		$("#fieldprevalueslist").sortable({ 
			update : function () { 
					    
				//var sortorder = $('#designsurface').sortable('serialize');
				//myDesignSurface.UpdatePageSortOrder(sortorder);
			} 
  		});
	}
	
	
	/* Set Focus */
	$('#fieldcaption').focus();
	
	$("#fieldform").validate({submitHandler: function() { 
		UpdateField(field);
	            		
        }});
}

function UpdateField(field)
{
	var newcaption = $("#fieldcaption").val();
	var newtype = $("#fieldtype").val();
	var newmandatory = $("#fieldmandatory").attr('checked');
  	var newregex = $("#fieldregex").val();
			
	$("#"+field).attr('fieldcaption',newcaption);
	$("#"+field).attr('fieldtype',newtype);
	$("#"+field).attr('fieldmandatory',newmandatory);
	$("#"+field).attr('fieldregex',newregex);
		
	$("#"+field + ' label').text(newcaption);


	//clear prevalues
	var prevaluecontainer = '#' + field + ' .fieldprevalues';
	$(prevaluecontainer).children().remove();
    
    //clear examples
	$('#' + field + ' .fieldexample').children().remove();
	$('#' + field + ' .fieldexample').text("");
	
    // set new example
	var examplefield = GetFieldPreview(newtype).replace("{caption}", newcaption);
	
	//does it have prevalues
	if($("#fieldtype option:selected").attr('prevalues') == "1")
	{
	    $("#" + field).attr('fieldhasprevalues', '1');

	    

		var previewprevalue = "";
		$("#fieldprevalueslist").children().each(function() {
		    var child = $(this);

		    var prevalue = child.children(":first-child").text();
		    var prevalueid = child.attr("id");

		    $(prevaluecontainer).append("<div id='" + prevalueid + "'>" + prevalue + "</div>");

		    previewprevalue += GetFieldPrevaluePreview(newtype).replace("{caption}", prevalue);
		    //todo
		    //myDesignSurface.AddPrevalue(fieldid,prevalueid,value);

		});

		
		examplefield = examplefield.replace("{prevalues}", previewprevalue);

		
		
	}
	else
	{
		$("#"+field).attr('fieldhasprevalues','0');
    }

    $('#' + field + ' .fieldexample').append(examplefield);
    	
	$.modal.close();
		
	myDesignSurface.UpdateField(field,newcaption,newtype,newmandatory,newregex);
}

function DeleteField(field)
{
	if(ConfirmDelete())
	{
		$("#" +field).hide('blind',{},500,function() {
			$("#" +field).remove();
			myDesignSurface.DeleteField(field);
			});
		
		
	}
}

function AddPrevalue()
{
	var prevalue = $("#fieldnewprevalue").val();
	
	var prevaluenumber = $("#fieldprevalueslist").children().size() + 1; 
	
	var prevalueid = "prevalue_" + prevaluenumber;
		         
	if(prevalue.length > 0)
	{
		$("#fieldprevalueslist").append("<li id='"+prevalueid+"'><span class='prevaluetext'>" + prevalue  + "</span> <a href='javascript:DeletePrevalue(\""+prevalueid+"\");' class='delete'>"+lang_delete+"</a></li>");
		$("#fieldnewprevalue").val('');
	}
}


function DeletePrevalue(prevalue)
{
	if(ConfirmDelete())
	{
		$("#" +prevalue).hide('blind',{},500,function() {
				$("#" +prevalue).remove();
				//myDesignSurface.DeletePage(page);
			});
		
		
	}
}