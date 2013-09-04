var myDesignSurface = new DesignSurface();
var pageguid = 0;
var fieldsetguid = 0;
var fieldguid = 0;
var currentpage = 0;
var addfield = true;
var fieldToUpdate;
var fieldToUpdatePrevalues;

$(document).ready(function () {


    //    $("#designsurface").sortable({
    //        handle: '.handle',
    //        update: function() {

    //            var sortorder = $('#designsurface').sortable('serialize');
    //            myDesignSurface.UpdatePageSortOrder(sortorder);
    //        }
    //    });


    $(".fieldsetcontainer").sortable({
        handle: '.handle',
        connectWith: '.fieldsetcontainer'
    });

    $(".fieldcontainer").sortable({
        handle: '.handle',
        connectWith: '.fieldcontainer'
    });

    $("#" + addpage_id).click(function () {

        ShowAddPageDialog();

    });

    ToggleAddFieldsetAction();
    ToggleAddFieldAction();

    $(".CancelModal").click(function () {
        CloseDialog();
    });

    $("#addprevalue").click(function () {

        AddPrevalue();
    });

    $("#editaddprevalue").click(function () {

        EditAddPrevalue();
    });

    $("#fieldpageselect").change(function () {
        $('#fieldfieldsetselect').children().remove();

        $("#" + $("#fieldpageselect").val() + " .fieldset").each(function () {
            var text = $(this).children(".fieldsetname").text();
            var value = $(this).attr("id");
            $("#fieldfieldsetselect").append($("<option></option>").attr("value", value).text(text));
        });
    });

    $("#fieldtype").change(function () {

        //also show additional fieldtype settings
        ShowFieldTypeSpecificSettings($("#fieldtype option:selected").val());

        if ($("#fieldtype option:selected").attr('prevalues') == "1") {
            if (!($('#fieldprevalues').is(':visible'))) {


                $("#fieldprevalues").show('blind', {}, 500);

                //$("#fieldprevalueslist").sortable({
                //    update: function () {

                //        //var sortorder = $('#designsurface').sortable('serialize');
                //        //myDesignSurface.UpdatePageSortOrder(sortorder);
                //    }
                //});
            }
        }
        else {
            if ($('#fieldprevalues').is(':visible')) {
                $("#fieldprevalues").hide('blind', {}, 500);
            }
        }

        if ($("#fieldtype option:selected").attr('regex') == "1") {

            $("#fieldinvaliderrormessagecontainer").show();

            if (!($('#fieldregexcontainer').is(':visible'))) {
                $("#fieldregexcontainer").show('blind', {}, 500);


            }
        }
        else {

            $("#fieldinvaliderrormessagecontainer").hide();

            if ($('#fieldregexcontainer').is(':visible')) {
                $("#fieldregexcontainer").hide('blind', {}, 500);


            }
        }
    });


    $('.editable').editable(function (value, settings) { $(this).html(value); }, { onblur: 'submit', tooltip: 'Click to edit', cssclass: 'inlineEditor' });


    $('#toggleadditionalsettings').click(function () {

        $('#fieldadditionalsettings').toggle();

    });

    $('#toggleconditions').click(function () {

        $('#fieldconditions').toggle();

    });

    if ($('#prevaluestype').children().size() == 1) {
        $('#prevaluetypeselection').hide();

        $('#prevaluetypeselectionselect').hide();
    }

    $('#prevaluestype').change(function () {

        $("#fieldprevalueslist").children().remove();

        if ($("#prevaluestype option:selected").val().length > 0) {
            GetPrevalues($("#prevaluestype option:selected").val());
        }

        if ($("#prevaluestype option:selected").attr('crud') == "1") {


            //$('#prevalueadd').show();
        }
        else {

            //$('#prevalueadd').hide();
        }
    });


    if (currentStep == 0) {
        SetupStepsNavigation();
    }
    else {
        ShowPage(currentStep);
    }

    $("#stepnavnew").click(function () {
        ShowAddPageDialog();
    });

    $("#stepnavnext").click(function () {
        ShowNextPage();
    });

    $("#stepnavprev").click(function () {
        ShowPreviousPage();
    });



    SetFieldHover();

    SetPrevalueHover();
    
    $("#fieldmandatory").click(function () {
        if ($("#fieldmandatory").is(":checked"))
            $("#fieldrequirederrormessagecontainer").show();
        else
            $("#fieldrequirederrormessagecontainer").hide();
    });


    var form = $('#fieldform').get(0);
    $.removeData(form, 'validator');

    $('#fieldform').unbind('submit');

    $("#fieldform").validate({ submitHandler: function () {
        AddUpdateField();
    }
    });


    var form = $('#prevalueform').get(0);
    $.removeData(form, 'validator');
    $('#prevalueform').unbind('submit');

    $("#prevalueform").validate({ submitHandler: function () {
        UpdatePrevalues(fieldToUpdatePrevalues);

        $("#" + fieldToUpdatePrevalues + " .fieldeditprevalues").show();
        $("#" + fieldToUpdatePrevalues + " .fieldControl").show();

    }


    });


    $(".addfieldrule").click(function () {


        var rulefield = $("#fieldruleadd").val();
        var ruleoperator = $("#fieldruleaddoperator").val();
        var rulevalue = $("#fieldruleaddvalue").val();

        var clone = $("#ruleadd").clone();
        $(clone).find("*").removeAttr("id");
        $(clone).find(".addfieldrule").remove();
        $(clone).find(".fieldruleselect").val(rulefield);
        $(clone).find(".fieldruleoperator").val(ruleoperator);
        $(clone).find(".fieldrulevalue").val(rulevalue);
        $(clone).append("<img src='css/img/remove_gray.png' class='removefieldrule' title='remove rule' alt='remove rule' style='cursor:pointer; margin:0 3px;' />");
        $("#fieldrules").append($(clone));

        $("#fieldruleaddoperator").val($("#fieldruleaddoperator option:first").val());
        $("#fieldruleadd").val($("#fieldruleadd option:first").val());
        $("#fieldruleaddvalue").val("");
    });

    $(".removefieldrule").live("click", function () { $(this).parent().remove(); });

    $('#fieldenableconditions').change(function () {
        $("#fieldconditions").toggle();
    });

    FillFieldDropDown();

//    DimHiddenFields();
});

function FillFieldDropDown() {
    $("#fieldruleadd").children().remove();
    $(".field").each(function () {
        $("#fieldruleadd").append("<option value='" + $(this).attr("rel") + "'>" + $(this).attr("fieldcaption") + "</option>");
    });
}
//function DimHiddenFields() {
//    $(".field").each(function () {
//        if ($(this).attr("") == "0" && ($(this).attr("fieldenablecondition") == "True" || $(this).attr("fieldenablecondition") == "true")) {
//            $(".fieldexamplelabel", $(this)).addClass("grey");
//        }
//    });
//}
function AddUpdateField() {

    if (addfield) {

        AddField();
    }
    else {
        UpdateField(fieldToUpdate);
    }
}

function SetFieldHover() {
    
    $('.field').hover(function() {

        $(this).addClass('activefield');

    }, function() {

        $(this).removeClass('activefield');

    });
    
}

function SetPrevalueHover() {

  

    $("#editprevaluelist li").live({
        mouseenter:
           function () {
               $(this).addClass('activeprevalue');
           },
        mouseleave:
           function () {
               $(this).removeClass('activeprevalue');
           }
    }
    );
    
}


function ShowPage(page) {
    currentpage = page;
    $(".page").hide();
    $(".page:nth-child(" + currentpage + ")").show();

    if (currentpage == 1) {
        $("#stepnavprev").hide();

    } else {
        $("#stepnavprev").show();
    }
    if ($(".page").size() == currentpage) {
        $("#stepnavnext").hide();
        $("#stepnavnew").show();
    }
    else {
        $("#stepnavnext").show();
        $("#stepnavnew").hide();
    }

}
function ShowNextPage() {

   
    
    $(".page:nth-child(" + currentpage + ")").hide();
    currentpage++;
    $(".page:nth-child(" + currentpage + ")").show();

    if (currentpage > 1) {
        $("#stepnavprev").show();
    }
    else {
        $("#stepnavprev").hide();
    }
    if ($(".page").size() == currentpage) {
        $("#stepnavnext").hide();
        $("#stepnavnew").show();
    }
}

function ShowPreviousPage() {

    if (currentpage > 1) {
        $(".page:nth-child(" + currentpage + ")").hide();
        currentpage--;
    }
    $(".page:nth-child(" + currentpage + ")").show();

    if($(".page").size() == currentpage)
    {
        $("#stepnavnext").hide();
        $("#stepnavnew").show();
    }
    else
    {
        $("#stepnavnext").show();
        $("#stepnavnew").hide();
    }
    if (currentpage == 1) {
        $("#stepnavprev").hide();

    }
}
function SetupStepsNavigation() {

    if($(".page").size() == 0)
    {
        $("#stepnavprev").hide();
        $("#stepnavnext").hide();
        $("#stepnavnew").show();
    }
    else if ($(".page").size() == 1) {

        currentpage = 1;
        $(".page:nth-child(1)").show();
        $("#stepnavprev").hide();
        $("#stepnavnext").hide();
        $("#stepnavnew").show();

        $(".page .pageheader .delete").hide();
    }
    else {

        currentpage = 1;
        $(".page:nth-child(1)").show();
        $("#stepnavprev").hide();
        $("#stepnavnew").hide();
        $("#stepnavnext").show();
    }
    
}

function ToggleAddFieldsetAction() {

    $("#" + addfieldset_id).unbind('click'); 
    
    if ($('#designsurface').children().size() > 0) {
        $("#" + addfieldset_id).removeClass("disabled");
        $("#" + addfieldset_id).click(function() {
            ShowAddFieldsetDialog(null);
        });
    }
    else {

        $("#" + addfieldset_id).addClass("disabled");

        $("#" + addfieldset_id).click(function(e) {
            e.preventDefault();            
        });
        
    }
}

function ToggleAddFieldAction() {

    $("#" + addfield_id).unbind('click');

    if ($(".fieldset").size() > 0) {
        $("#" + addfield_id).removeClass("disabled");
        $("#" + addfield_id).click(function() {
            ShowAddFieldDialog(null);
        });
    }
    else {

        $("#" + addfield_id).addClass("disabled");

        $("#" + addfield_id).click(function(e) {
            e.preventDefault();
        });

    }
}

function CloseDialog() {
    $.modal.close();
}

function ShowAddPageDialog() 
{
    /* Set Captions */
    $('#PageModal h1').text(lang_addpage);
    $('#PageModal #pageform .submit').attr('value', lang_addpage);

    /* Clear Fields */
    $('#NewPageName').val('');

    /* Show Modal */
    //$('#PageModal').modal();

    //auto add
    
    //$('#PageModal').appendTo("#designsurface");
    //$('#PageModal').show();

    $('#cancelpageaction').click(function() {
        $('#PageModal').prependTo("#modals");
    });
    
    /* Set Focus */
    $('#NewPageName').focus();

    var form = $('#pageform').get(0);
    $.removeData(form, 'validator');
    $('#pageform').unbind('submit');
    
    $("#pageform").validate({ submitHandler: function() {
        AddPage();

    }
    });

    //auto add
    $("#NewPageName").val("Edit form step")
    AddPage();
}

function AddPage() {

    var pagenumber = pageguid;
    pageguid = pageguid + 1;
    
	var pagename = 	$("#NewPageName").val();
	var pageid = "page_" + pagenumber;

	$("#designsurface").append("<div class='page' id='" + pageid + "'> <div class='pageheader'><strong class='pagename editable'>" + pagename + "</strong> <a class='add iconButton' href='#' onclick='javascript:ShowAddFieldsetDialog(\"" + pageid + "\");'>add fieldset</a> <a class='update iconButton' href='#' onclick='javascript:ShowUpdatePageDialog(\"" + pageid + "\");' style='display: none'>" + lang_update + "</a> <a href='#' onclick='javascript:DeletePage(\"" + pageid + "\");' class='delete iconButton'>" + lang_delete + "</a> <span class='handle' style='display:none'>handle</span> </div><div class='pageeditcontainer'></div><div class='fieldsetcontainer' id='fieldsetcontainerpage_" + pagenumber + "'></div></div>");
	
	$(".fieldsetcontainer").sortable({ 
			connectWith: '.fieldsetcontainer',
			handle : '.handle', 
			update : function () { 
				var sortorder = $('#fieldsetcontainer' + pageid).sortable('serialize');
				myDesignSurface.UpdateFieldsetSortOrder(pageid, sortorder);
			} 
  	});

  	ToggleAddFieldsetAction();
  	
	//CloseDialog();
  	$('#PageModal').prependTo("#modals");
  	
	 $('.editable').editable(function(value, settings) {$(this).html(value);},{ onblur: 'submit', tooltip: 'Click to edit', cssclass: 'inlineEditor'});
	
	myDesignSurface.AddPage(pageid,pagename);

	ShowNextPage();

	$(".page .pageheader .delete").show();
	
	//auto inline edit;
	$('#' + pageid + " .pageheader .editable").click();
	
}

function ShowUpdatePageDialog(page) {

    
	/* Set Captions */
	$('#PageModal h1').text(lang_updatepage);
	$('#PageModal #pageform .submit').attr('value',lang_updatepage);
	
	/* Set Current Values */
	$('#NewPageName').val($('#' + page + ' .pagename').text());
	
	
	//$('#PageModal').modal();
	$('#PageModal').prependTo("#" + page + " .pageeditcontainer");
	$('#PageModal').show();

	$('#cancelpageaction').click(function() {
	    $('#PageModal').prependTo("#modals");
    });
	
	/* Set Focus */
	$('#NewPageName').focus();


    var form = $('#pageform').get(0);
    $.removeData(form, 'validator');
    $('#pageform').unbind('submit');


	$("#pageform").validate({ submitHandler: function() {
	   
	    UpdatePage(page);

	}
	});
}

function UpdatePage(page)
{
	var newname = $('#NewPageName').val();
	
	$('#' + page + ' .pagename').text(newname);
	
	//CloseDialog();
	$('#PageModal').prependTo("#modals");

	
	 $('.editable').editable(function(value, settings) {$(this).html(value);},{ onblur: 'submit', tooltip: 'Click to edit', cssclass: 'inlineEditor'});
	
	myDesignSurface.UpdatePage(page,newname);
}

function DeletePage(page)
{
	if(ConfirmDelete())
	{
	    $("#" + page).hide('blind', {}, 500, function() {


	        $('#PageModal').prependTo("#modals");
	        $('#FieldSetModal').prependTo("#modals");
	        $('#FieldModal').prependTo("#modals");

	        $("#" + page).remove();

	        if ($(".page").size() == 1) {
	            $(".page .pageheader .delete").hide();
	        }
	        
	        ShowPreviousPage();
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

function ShowAddFieldsetDialog(page) {

    /* Show page selection */
    $("#fieldsetpageselectcontainer").show();
    
	/* Set Captions */
	$('#FieldsetModal h1').text(lang_addfieldset);
	$('#FieldsetModal #fieldsetform .submit').attr('value',lang_addfieldset);
			
	/* Clear Fields */
	$('#NewFieldsetName').val('');

	if (page != null) {

	    $('#fieldsetpageselectcontainer').hide();
	    
	    $('#cancelfieldsetaction').click(function() {
	        $('#FieldsetModal').prependTo("#modals");
	        $('#FieldsetModal').hide();
	    });
	    
	    //auto add
	    //$('#FieldsetModal').appendTo("#" + page);
	    //$('#FieldsetModal').show();
	}
	else {
	    // auto add
	    //$('#fieldsetpageselectcontainer').show();
	    //$('#FieldsetModal').modal();
	}
	
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

	var form = $('#fieldsetform').get(0);
	$.removeData(form, 'validator');
	$('#fieldsetform').unbind('submit');
	
	$("#fieldsetform").validate({submitHandler: function() { 
		AddFieldset();
	            		
	}});

	// auto add
	$("#NewFieldsetName").val("Edit fieldset");
	AddFieldset();
}

function AddFieldset()
{

    var page = $("#pageselect").val();

    var fieldsetnumber = fieldsetguid;
    fieldsetguid = fieldsetguid + 1;

    var fieldsetcontainer = '#' + page + ' .fieldsetcontainer';

    var newfieldsetid = page.replace('_', '') + "fieldset_" + fieldsetnumber;
	
	var fieldsetname = $("#NewFieldsetName").val();

	$(fieldsetcontainer).append("<div class='fieldset' id='" + newfieldsetid + "'><div class='fieldsetheader'><strong class='fieldsetname editable'>" + $("#NewFieldsetName").val() + "</strong> <a class='add iconButton' href='#' onclick='javascript:ShowAddFieldDialog(\"" + newfieldsetid + "\");' style='display:none'>add field</a> <a class='update iconButton' href='#' onclick='javascript:ShowUpdateFieldsetDialog(\"" + newfieldsetid + "\");' style='display: none'>" + lang_update + "</a> <a href='#' onclick='javascript:DeleteFieldset(\"" + newfieldsetid + "\");' class='delete iconButton'>" + lang_delete + "</a> <span class='handle'>handle</span> </div> <div class='fieldseteditcontainer'></div><div class='fieldcontainer'></div> <button class='addfield' onclick='javascript:ShowAddFieldDialog(\"" + newfieldsetid + "\"); return false;'><img style='vertical-align: middle;' src='css/img/add_small.png'/><span>Add Field</span></button></div>");
	

		
	
	$(".fieldcontainer").sortable({ 
			connectWith: '.fieldcontainer',
			handle : '.handle', 
			update : function () { 
				//$('#designsurface').sortable('serialize'); 
				//alert('field sort order updated');
				myDesignSurface.UpdateFieldSortOrder("todo","todo");
			} 
  	});

  	ToggleAddFieldAction();
  	
  	CloseDialog();
  	$('#FieldsetModal').prependTo("#modals");
  	$('#FieldsetModal').hide();

  	
  	 $('.editable').editable(function(value, settings) {$(this).html(value);},{ onblur: 'submit', tooltip: 'Click to edit', cssclass: 'inlineEditor'});


    //auto inline edit;
  	$('#' + newfieldsetid + " .fieldsetheader .editable").click();
  	
  
  	myDesignSurface.AddFieldset(page,newfieldsetid,fieldsetname);
}	

function ShowUpdateFieldsetDialog(fieldset) {

    /* Hide page selection */
     $("#fieldsetpageselectcontainer").hide();
     
    /* Set Captions */
	$('#FieldsetModal h1').text(lang_updatefieldset);
	$('#FieldsetModal #fieldsetform .submit').attr('value',lang_updatefieldset);
	
	/* Set Current Values */
	$('#NewFieldsetName').val($('#' + fieldset + ' .fieldsetname').text());
	
	//$('#FieldsetModal').modal();
	$('#FieldsetModal').prependTo("#" + fieldset + " .fieldseteditcontainer");
	$('#FieldsetModal').show();

	$('#cancelfieldsetaction').click(function() {
	    $('#FieldsetModal').prependTo("#modals");
	    $('#FieldsetModal').hide();
	});
	
	/* Set Focus */
	$('#NewFieldsetName').focus();

	var form = $('#fieldsetform').get(0);
	$.removeData(form, 'validator');
	$('#fieldsetform').unbind('submit');
	
	$("#fieldsetform").validate({submitHandler: function() { 
		UpdateFieldset(fieldset);
	            		
        }});
}

function UpdateFieldset(fieldset)
{
	var newname = $('#NewFieldsetName').val();
		
	$('#' + fieldset+ ' .fieldsetname').text(newname);
		
	//CloseDialog();
	$('#FieldsetModal').prependTo("#modals");
	$('#FieldsetModal').hide();
	
	 $('.editable').editable(function(value, settings) {$(this).html(value);},{ onblur: 'submit', tooltip: 'Click to edit', cssclass: 'inlineEditor'});
	
	myDesignSurface.UpdateFieldset(fieldset,newname);
}

function DeleteFieldset(fieldset)
{
	if(ConfirmDelete())
	{
	    $("#" + fieldset).hide('blind', {}, 500, function() {
	            $('#FieldsetModal').prependTo("#modals");
	            $('#FieldModal').prependTo("#modals");
		        $("#" + fieldset).remove();
		        ToggleAddFieldAction();
				myDesignSurface.DeleteFieldset(fieldset);
			});
		
		
	}
}


function ShowAddFieldDialog(fieldset) {

    /* Show fieldset selection */
    $("#fieldfieldsetselectcontainer").show();

    //hide additional settings
    $('#fieldadditionalsettings').hide();

    $('#fieldconditions').hide();

	/* Set Captions */
	$('#FieldModal h1').text(lang_addfield);
	$('#FieldModal #fieldform .submit').attr('value',lang_addfield);
	
	/* Clear Fields */
	$('#fieldcaption').val('');
	//$('#fieldtype').selectedIndex = -1;
    
	//$("#fieldtype option:first").attr('selected', 'selected');
	$("#fieldtype option:[value='3f92e01b-29e2-4a30-bf33-9df5580ed52c']").attr('selected', 'selected');

    //show fieldtype specific settings
	ShowFieldTypeSpecificSettings('3f92e01b-29e2-4a30-bf33-9df5580ed52c');


	$('#fieldmandatory').attr('checked', false);
	$('#fieldregex').val('');
	$('#fieldprevalueslist').children().remove();

	$('#fieldprevalues').hide();

	$("#fieldtooltip").val('');

	$("#fieldrequirederrormessage").val('');
	$("#fieldinvaliderrormessage").val('');
	
	$("#fieldrequirederrormessagecontainer").hide();

    //Set standard prevalue type
	$('#prevaluestype').val('');

	if (fieldset != null) {

	    $("#fieldfieldsetselectcontainer").hide();
	    
	    $('#cancelfieldaction').click(function() {
	        $('#FieldModal').prependTo("#modals");
	    });

	    $('#FieldModal').appendTo("#" + fieldset);
	    $('#FieldModal').show();
	}
	else {
	    $("#fieldfieldsetselectcontainer").show();
	    $('#FieldModal').modal();
	}

    //Scroll to field modal
	var fmPos = $("#FieldModal").position().top;
	jQuery(".tabpagescrollinglayer").animate({ scrollTop: fmPos }, "slow");
	
	/* Set Focus */
	$('#fieldcaption').focus();

	$(".page").each(function() {
	    var text = $(this).children(".pagename").text();
	    var value = $(this).attr("id");
	    if ($(this).children(".fieldsetcontainer").children().size() > 0) {
	        $("#fieldpageselect").append($("<option></option>").attr("value", value).text(text));
	    }
	});

	$("#fieldenableconditions").attr('checked', false);
	$("#fieldconditionactiontype option:[value='0']").attr('selected', 'selected');
	$("#fieldconditionlogictype option:[value='0']").attr('selected', 'selected');
	$("#fieldruleadd").val($("#fieldruleadd option:first").val());
	$("#fieldruleaddoperator").val($("#fieldruleaddoperator option:first").val());
    $("#fieldruleaddvalue").val("");
	$("#fieldrules").children().remove();
    
	FieldSetFieldsetSelection(fieldset);


//	var form = $('#fieldform').get(0);
//	$.removeData(form, 'validator');
//	
//    $('#fieldform').unbind('submit');

//	$("#fieldform").validate({ submitHandler: function () {
//	    AddField();
//	}
//	});

	addfield = true;

	myDesignSurface.ShowAddFieldDialog();
}

function AddField()
{
    var fieldsetid = $('#fieldfieldsetselect').val(); //$('#fieldsetid').text();
	var fieldcaption = $("#fieldcaption").val();
	var fieldtype = $("#fieldtype").val();
	var fieldmandatory = $("#fieldmandatory").is(':checked');
  	var fieldregex = $("#fieldregex").val();
  	var fieldtooltip = $("#fieldtooltip").val();

  	var fieldrequirederrormessage = $("#fieldrequirederrormessage").val();
  	var fieldinvaliderrormessage = $("#fieldinvaliderrormessage").val();
  	
  	var fieldnumber = fieldguid;
  	fieldguid = fieldguid + 1;

  	var fieldcontainer = '#' + $('#fieldfieldsetselect').val() +' .fieldcontainer';

  	var newfieldid = $('#fieldfieldsetselect').val().replace('_', '') + "field_" + fieldnumber;


  	var fieldenablecondition = $("#fieldenableconditions").is(':checked');
  	var fieldconditionactiontype = $("#fieldconditionactiontype").val();
  	var fieldconditionlogictype = $("#fieldconditionlogictype").val();
  	

	var mandatorycontent = "";
	if (fieldmandatory) {
	    mandatorycontent = "*";
	}
	var mandatory = "<span class='mandatory'>" + mandatorycontent + "</span>";



	$(fieldcontainer).append("<div class='field' id='" + newfieldid + "'> <a class='update iconButton' href='#' onclick='javascript:ShowUpdateFieldDialog(\"" + newfieldid + "\");'>" + lang_update + "</a> <a class='copy iconButton' href='#' onclick='javascript:CopyField(\"" + newfieldid + "\");'>" + lang_copy + "</a> <a href='#' onclick='javascript:DeleteField(\"" + newfieldid + "\");' class='delete iconButton'>" + lang_delete + "</a> <span class='handle'>handle</span> <div class='fieldprevalues' style='display:none'></div><div class='fieldadditionalsettings' style='display:none'></div><div class='fieldconditionrules' style='display:none'></div><div class='fieldexample'></div>  <div style='clear: both;'></div></div>");

	$(fieldcontainer).append("<div class='fieldeditcontainer' id='fieldeditcontainer" + newfieldid + "'></div>");


	$("#"+newfieldid).attr('fieldcaption',fieldcaption);
	$("#"+newfieldid).attr('fieldtype',fieldtype);
	$("#"+newfieldid).attr('fieldmandatory',fieldmandatory);
	$("#" + newfieldid).attr('fieldregex', fieldregex);
	$("#" + newfieldid).attr('fieldtooltip', fieldtooltip);

	$("#" + newfieldid).attr('fieldrequirederrormessage', fieldrequirederrormessage);
	$("#" + newfieldid).attr('fieldinvaliderrormessage', fieldinvaliderrormessage);
	
	//does it allow regex
	if ($("#fieldtype option:selected").attr('regex') == "1") {
	    $("#" + newfieldid).attr('fieldsupportsregex', '1');
	}
	else {
	    $("#" + newfieldid).attr('fieldsupportsregex', '0');
	}
	
	//does it have prevalues
	if($("#fieldtype option:selected").attr('prevalues') == "1")
	{
	    $("#"+newfieldid).attr('fieldsupportsprevalues','1');
	}
	else
	{
	    $("#" + newfieldid).attr('fieldsupportsprevalues', '0');
	    $("#" + newfieldid + " .fieldeditprevalues").hide();
	     
    	}

	$("#" + newfieldid).attr('fieldenablecondition', fieldenablecondition);
	$("#" + newfieldid).attr('fieldconditionactiontype', fieldconditionactiontype);
	$("#" + newfieldid).attr('fieldconditionlogictype', fieldconditionlogictype);
    
    
	myDesignSurface.AddField(fieldsetid,newfieldid,fieldcaption,fieldtype,fieldmandatory,fieldregex);

	//condition rules
	var conditionrulescontainer = '#' + newfieldid + ' .fieldconditionrules';
    
	$("#fieldrules").children().each(function () {
	    var rulefield = $(".fieldruleselect", $(this)).val();
	    var ruleoperator = $(".fieldruleoperator", $(this)).val();
	    var rulevalue = $(".fieldrulevalue", $(this)).val();

	    $(conditionrulescontainer).append("<div class='conditionrule' field='" + rulefield + "' operator='" + ruleoperator + "'>" + rulevalue + "</div>");
	});

	
    
	var prevaluecontainer = '#' + newfieldid + ' .fieldprevalues';
	


	var myPrevalues = new Array();
	var prevaluecount = 0;

	if ($("#fieldtype option:selected").attr('prevalues') == "1") {
	    $(prevaluecontainer).attr('type', $("#prevaluestype option:selected").val());
	    $(prevaluecontainer).attr('crud', $("#prevaluestype option:selected").attr('crud'));
	}
	
	$("#fieldprevalueslist").children().each(function() {
	    var child = $(this);

	    var relattr = "";
	    if (child.attr("rel") != undefined) {
	        relattr = "rel='" + child.attr("rel") + "'";
	    };
	    
	    var prevalue = child.children(":first-child").text();
	    var prevalueid = child.attr("id");

	    $(prevaluecontainer).append("<div id='" + prevalueid + "' class='prevalue' " + relattr + ">" + prevalue + "</div>");


	    //todo
	    //myDesignSurface.AddPrevalue(fieldid,prevalueid,value);

	    myPrevalues[prevaluecount] = prevalue;
	    prevaluecount++;
	});



	//additional settings
    SaveAdditionalSettings(newfieldid);



	//Field Preview
	$("#" + newfieldid + " .fieldexample").append("<label class='fieldexamplelabel'><span>" + fieldcaption + "</span>" + " " + mandatory + "</label><div class='fieldControl'> <div class='fieldeditprevalues'><a href='#' onclick='javascript:ShowUpdatePrevaluesDialog(\"" + newfieldid + "\")'>Edit items</a></div></div><br style='clear: both;' />");

//    if (fieldconditionactiontype == "0" && fieldenablecondition) {
//	    $(".fieldexamplelabel", $("#" + newfieldid + " .fieldexample")).addClass("grey");
//	}
    
    GetFieldPreview(newfieldid, fieldtype, myPrevalues);

	//does it have prevalues
	if ($("#fieldtype option:selected").attr('prevalues') == "1") {

	    if ($("#prevaluestype option:selected").attr('crud') == "0")
	    {
	        $("#" + newfieldid + " .fieldeditprevalues").hide();
	    }
	}
	else {
	    
	    $("#" + newfieldid + " .fieldeditprevalues").hide();
	}

	CloseDialog();
	$('#FieldModal').prependTo("#modals");

	SetFieldHover();
}


function ShowUpdateFieldDialog(field) {

    $(".field").show();
    
    /* Hide fieldset selection */
    $("#fieldfieldsetselectcontainer").hide();

    //hide additional settings
    $('#fieldadditionalsettings').hide();

    $('#fieldconditions').hide();
    
	/* Set Captions */
	$('#FieldModal h1').text(lang_updatefield);
	$('#FieldModal #fieldform .submit').attr('value',lang_updatefield);
	
	/* Set Current Values */
	var fieldcaption = $("#"+field).attr('fieldcaption');
	$("#fieldcaption").val(fieldcaption);

	var fieldtooltip = $("#" + field).attr('fieldtooltip');
	$("#fieldtooltip").val(fieldtooltip);

	var fieldrequirederrormessage = $("#" + field).attr('fieldrequirederrormessage');
	$("#fieldrequirederrormessage").val(fieldrequirederrormessage);

	var fieldinvaliderrormessage = $("#" + field).attr('fieldinvaliderrormessage');
	$("#fieldinvaliderrormessage").val(fieldinvaliderrormessage);
	
	var fieldtype = $("#"+field).attr('fieldtype');
	$("#fieldtype").val(fieldtype);

    //show fieldtype specific settings
	ShowFieldTypeSpecificSettings(fieldtype);
	LoadAdditionalSettings(field);

	var fieldmandatory = $("#"+field).attr('fieldmandatory');
	if (fieldmandatory.toString().toLowerCase() == 'true' || fieldmandatory == '1')
	{
	    $("#fieldmandatory").attr('checked', true);
	    $("#fieldrequirederrormessagecontainer").show();
	}
	else
	{
	    $("#fieldmandatory").attr('checked', false);
	    $("#fieldrequirederrormessagecontainer").hide();
	}
	
	var fieldregex = $("#"+field).attr('fieldregex');
	$("#fieldregex").val(fieldregex);

    //condition
	var fieldenablecondition = $("#" + field).attr('fieldenablecondition');

	$("#fieldenableconditions").attr('checked', fieldenablecondition == "True" || fieldenablecondition == "true");
	if ($('#fieldenableconditions').is(':checked')) {
	    $('#fieldconditions').show();
	}

    var fieldconditionactiontype = $("#" + field).attr('fieldconditionactiontype');
	$("#fieldconditionactiontype").val(fieldconditionactiontype);

	var fieldconditionlogictype = $("#" + field).attr('fieldconditionlogictype');
	$("#fieldconditionlogictype").val(fieldconditionlogictype);

	$("#fieldruleadd").val($("#fieldruleadd option:first").val());
	$("#fieldruleaddoperator").val($("#fieldruleaddoperator option:first").val());
	$("#fieldruleaddvalue").val("");
    
    //condition rules
	$("#fieldrules").children().remove();
	$('#' + field + " .fieldconditionrules").children().each(function () {
	    var rulefield = $(this).attr("field");
	    var ruleoperator = $(this).attr("operator");
	    var rulevalue = $(this).html();

	    var clone = $("#ruleadd").clone();
	    $(clone).find("*").removeAttr("id");
	    $(clone).find(".addfieldrule").remove();
	    $(clone).find(".fieldruleselect").val(rulefield);
	    $(clone).find(".fieldruleoperator").val(ruleoperator);
	    $(clone).find(".fieldrulevalue").val(rulevalue);
	    $(clone).append("<img src='css/img/remove_gray.png' class='removefieldrule' title='remove rule' alt='remove rule' style='cursor:pointer; margin:0 3px;' />");
	    $("#fieldrules").append($(clone));
    });

	$('#cancelfieldaction').click(function() {
	    $('#FieldModal').prependTo("#modals");
	    $('#FieldModal').hide();
	    $("#" + field).show();
	});

	//$('#FieldModal').modal();

	$("#fieldeditcontainer" + field).insertAfter("#" + field);
	
	$('#FieldModal').prependTo("#fieldeditcontainer" + field + "");
	$('#FieldModal').show();
	
	
	/* Regex */
	if ($("#" + field).attr('fieldsupportsregex') == "0") {
	    $("#fieldregexcontainer").hide();
	    $("#fieldinvaliderrormessagecontainer").hide();
	}
	else {
	    $("#fieldregexcontainer").show();
	    $("#fieldinvaliderrormessagecontainer").show();
	}
	
	/* Prevalues */
	$('#fieldprevalueslist').children().remove();
	if ($("#" + field).attr('fieldsupportsprevalues') == "1") {


	    //Show prevalue part
	    $("#fieldprevalues").show();

	    //Set Type
	    var prevaluetype = $("#" + field + " .fieldprevalues").attr('type');
	    $("#prevaluestype").val(prevaluetype);

	    var prevaluecrud = $("#" + field + " .fieldprevalues").attr('crud');

	    $('#prevalueadd').hide();
//	    if (prevaluecrud == "1") {

//	        $('#prevalueadd').show();
//	    }
//	    else {

//	        $('#prevalueadd').hide();
//	    }
	    //Populate prevalues

	    var pvseed = 0;
	    $('#' + field + " .fieldprevalues").children().each(function() {
	        child = $(this);

	        var prevalue = child.text();
	        var prevalueid = "pvid" + pvseed;

	        if (child.attr("rel") != undefined) {
	            var relattr = "rel='" + child.attr("rel") + "'";
	        };

	        var hidecrud = "";
	        if (prevaluecrud != "1") {
	            hidecrud = " style='display:none'";
	        }

	        $("#fieldprevalueslist").append("<li id='" + prevalueid + "' " + relattr + "><span class='prevaluetext'>" + prevalue + "</span> <a href='javascript:DeletePrevalue(\"" + prevalueid + "\");' class='delete' " + hidecrud + ">" + lang_delete + "</a> <span class='handle' " + hidecrud + ">handle</span></li>");

	        pvseed++;
	    });


	    //$("#fieldprevalueslist").sortable({
	    //    handle: '.handle',
	    //    update: function() {

	    //        //var sortorder = $('#designsurface').sortable('serialize');
	    //        //myDesignSurface.UpdatePageSortOrder(sortorder);
	    //    }
	    //});
	}
	else {
	    $("#fieldprevalues").hide();
	}
	
	
	/* Set Focus */
	$('#fieldcaption').focus();


//	var form = $('#fieldform').get(0);
//	$.removeData(form, 'validator');
//	$('#fieldform').unbind('submit');
//	
//	$("#fieldform").validate({submitHandler: function() { 
//		UpdateField(field);
//	            		
//        }});

	fieldToUpdate = field;

	addfield = false;

    $("#" + field).hide();
        
    myDesignSurface.ShowUpdateFieldDialog(field);
}

function UpdateField(field)
{
	var newcaption = $("#fieldcaption").val();
	var newtype = $("#fieldtype").val();
	var newmandatory = $("#fieldmandatory").is(':checked');
  	var newregex = $("#fieldregex").val();
  	var newtooltip = $("#fieldtooltip").val();
  	var newrequirederrormessage = $("#fieldrequirederrormessage").val();
  	var newinvaliderrormessage = $("#fieldinvaliderrormessage").val();
  	var fieldenablecondition = $("#fieldenableconditions").is(':checked');
  	var fieldconditionactiontype = $("#fieldconditionactiontype").val();
  	var fieldconditionlogictype = $("#fieldconditionlogictype").val();
    
	$("#"+field).attr('fieldcaption',newcaption);
	$("#"+field).attr('fieldtype',newtype);
	$("#"+field).attr('fieldmandatory',newmandatory);
	$("#" + field).attr('fieldregex', newregex);
	$("#" + field).attr('fieldtooltip', newtooltip);
	$("#" + field).attr('fieldrequirederrormessage', newrequirederrormessage);
	$("#" + field).attr('fieldinvaliderrormessage', newinvaliderrormessage);

	$("#" + field + ' .fieldexample label').text(newcaption);

	$("#" + field).attr('fieldenablecondition', fieldenablecondition);
	$("#" + field).attr('fieldconditionactiontype', fieldconditionactiontype);
	$("#" + field).attr('fieldconditionlogictype', fieldconditionlogictype);

   
    
	//clear prevalues
	var prevaluecontainer = '#' + field + ' .fieldprevalues';
	$(prevaluecontainer).children().remove();
    
    //clear examples
	$('#' + field + ' .fieldexample').children().remove();
	$('#' + field + ' .fieldexample').text("");

	//clear condition rules
	var conditionrulescontainer = '#' + field + ' .fieldconditionrules';
	$(conditionrulescontainer).children().remove();
    
    //mandatory
	var mandatorycontent = "";
	if (newmandatory) {
	    mandatorycontent = "*";
	}
	var mandatory = "<span class='mandatory'>" + mandatorycontent + "</span>";
	
   
	//does it allow regex
	if ($("#fieldtype option:selected").attr('regex') == "1") {
	    $("#" + field).attr('fieldsupportsregex', '1');
	}
	else {
	    $("#" + field).attr('fieldsupportsregex', '0');
	}

	//condition rules
	$("#fieldrules").children().each(function () {
	    var rulefield = $(".fieldruleselect", $(this)).val();
	    var ruleoperator = $(".fieldruleoperator", $(this)).val();
	    var rulevalue = $(".fieldrulevalue", $(this)).val();

	    $(conditionrulescontainer).append("<div class='conditionrule' field='" + rulefield + "' operator='" + ruleoperator + "'>" + rulevalue + "</div>");
	});
    
	var myPrevalues = new Array();
	var prevaluecount = 0;
	//does it have prevalues
	if($("#fieldtype option:selected").attr('prevalues') == "1") {
	    
	    //$("#" + field + " .fieldeditprevalues").show();
	    $("#" + field).attr('fieldsupportsprevalues', '1');

	    if ($("#fieldtype option:selected").attr('prevalues') == "1") {
	        $("#" + field + " .fieldprevalues").attr('type', $("#prevaluestype option:selected").val());
	        $("#" + field + " .fieldprevalues").attr('crud', $("#prevaluestype option:selected").attr('crud'));
	    }



	    $("#fieldprevalueslist").children().each(function() {
	        var child = $(this);

	        var prevalue = child.children(":first-child").text();
	        var prevalueid = child.attr("id");

	        var relattr = "";
	        if (child.attr("rel") != undefined) {
	            relattr = "rel='" + child.attr("rel") + "'";
	        };

	        $(prevaluecontainer).append("<div id='" + prevalueid + "' class='prevalue' " + relattr + ">" + prevalue + "</div>");


	        //todo
	        //myDesignSurface.AddPrevalue(fieldid,prevalueid,value);

	        myPrevalues[prevaluecount] = prevalue;
	        prevaluecount++;

	    });

		
		

		
		
	}
	else
	{
	    $("#" + field).attr('fieldsupportsprevalues', '0');
	    //$("#" + field + " .fieldeditprevalues").hide();
    }

    $('#' + field + ' .fieldexample').append("<label class='fieldexamplelabel'><span>" + newcaption + "</span>" + " " + mandatory + "</label><div class='fieldControl'><div class='fieldeditprevalues'><a href='#' onclick='javascript:ShowUpdatePrevaluesDialog(\"" + field + "\")'>Edit items</a></div></div><br style='clear: both;' />");

    if ($("#fieldtype option:selected").attr('prevalues') == "1") {

        
        
        if ($("#prevaluestype option:selected").attr('crud') == "0")
	    {
	        $("#" + field + " .fieldeditprevalues").hide();
	    }
	    else
	    {
	        $("#" + field + " .fieldeditprevalues").show();
	    }
    }
    else {
        $("#" + field + " .fieldeditprevalues").hide();
    }


    //additional settings
    SaveAdditionalSettings(field);

    GetFieldPreview(field, newtype, myPrevalues)


	//CloseDialog();
    $('#FieldModal').prependTo("#modals");
    $('#FieldModal').hide();

    $("#" + field).show();
    
	myDesignSurface.UpdateField(field,newcaption,newtype,newmandatory,newregex);
}

function FieldSetFieldsetSelection(fieldset) {
    var fieldsetid;
    var pageid;

    if (fieldset == null) {
        fieldsetid = $(".fieldset")[0].id;
    }
    else {
        fieldsetid = fieldset;
    }

    pageid = $("#" + fieldsetid).parent().parent().attr("id");

    $("#" + pageid + " .fieldset").each(function() {
        var text = $(this).children(".fieldsetname").text();
        var value = $(this).attr("id");
        $("#fieldfieldsetselect").append($("<option></option>").attr("value", value).text(text));
    });
    
    $("#fieldpageselect").val(pageid);
    $("#fieldfieldsetselect").val(fieldsetid);

}

function DeleteField(field)
{
	if(ConfirmDelete())
	{
	    $("#" + field).hide('blind', {}, 500, function() {

	        $('#FieldModal').prependTo("#modals");

	        $('#PreValueModal').prependTo("#modals");

	        $("#" + field).remove();

	        $("#fieldeditcontainer" + field).remove();
	        
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
	    $("#fieldprevalueslist").append("<li id='" + prevalueid + "'><span class='prevaluetext'>" + prevalue + "</span> <a href='#' onclick='javascript:DeletePrevalue(\"" + prevalueid + "\");' class='delete'>" + lang_delete + "</a> <a href='#' onclick='return false;' class='move iconButton' >handle</a></li>");
		$("#fieldnewprevalue").val('');
	}
    
}


function DeletePrevalue(prevalue) {
    if (ConfirmDelete()) {
        $("#" + prevalue).hide('blind', {}, 500, function() {
            $("#" + prevalue).remove();
            //myDesignSurface.DeletePage(page);
        });

    }
}

function SaveDesign() {
    myDesignSurface.SaveDesign(formguid, $("#designsurface").html());
}


function EditAddPrevalue() {
    var prevalue = $("#editnewprevalue").val();

    var prevaluenumber = $("#editprevaluelist").children().size() + 1;

    var prevalueid = "pvidedit" + prevaluenumber;

    if (prevalue.length > 0) {
        $("#editprevaluelist").append("<li id='" + prevalueid + "'><span class='prevaluetext editable'>" + prevalue + "</span> <a href='javascript:DeletePrevalue(\"" + prevalueid + "\");' class='delete iconButton'>" + lang_delete + "</a> <a class='iconButton move' href='#' onclick='return false;' >handle</a></li>");
        $("#editnewprevalue").val('');
    }

    $('.editable').editable(function (value, settings) { $(this).html(value); }, { onblur: 'submit', tooltip: 'Click to edit', cssclass: 'inlineEditor' });
    
}


var currentPrevaluesUpdateField = "";


function ShowUpdatePrevaluesDialog(field) {


   
    if ($('#PreValueModal').is(':visible') &&  currentPrevaluesUpdateField != "") {
       
        $('#PreValueModal').prependTo("#modals");
        $("#" + currentPrevaluesUpdateField + " .fieldeditprevalues").show();
        $("#" + currentPrevaluesUpdateField + " .fieldControl").show();
    }

    currentPrevaluesUpdateField = field;

    $('#cancelprevalueaction').click(function () {
        $('#PreValueModal').prependTo("#modals");
        //$('#PreValueModal').hide();
        //$("#" + field).show();
        $("#" + field + " .fieldeditprevalues").show();
        $("#" + field + " .fieldControl").show();
        currentPrevaluesUpdateField = "";
    });

    $('#editprevaluelist').children().remove();


    //Populate prevalues
    var pvseed = 0;
    $('#' + field + " .fieldprevalues").children().each(function () {
        child = $(this);

        var prevalue = child.text();
        var prevalueid = "pvidedit" + pvseed;

        if (child.attr("rel") != undefined) {
            var relattr = "rel='" + child.attr("rel") + "'";
        };

        $("#editprevaluelist").append("<li id='" + prevalueid + "' " + relattr + "><span class='prevaluetext editable'>" + prevalue + "</span> <a href='#' onclick='javascript:DeletePrevalue(\"" + prevalueid + "\");' class='delete iconButton'>" + lang_delete + "</a> <a href='#' onclick='return false;' class='move iconButton'>handle</a></li>");

        pvseed++;
    });



    $("#editprevaluelist").sortable({
        handle: '.move'
    });


    fieldToUpdatePrevalues = field;

   
    $('#PreValueModal').insertAfter("#" + field + " .fieldControl");
    $('#PreValueModal').show();

   

    $("#" + field + " .fieldeditprevalues").hide();
    $("#" + field + " .fieldControl").hide();
    //$("#" + field).hide();

     $('.editable').editable(function(value, settings) {$(this).html(value);},{ onblur: 'submit', tooltip: 'Click to edit', cssclass: 'inlineEditor'});
    
}

function UpdatePrevalues(field) {

    //clear prevalues
    var prevaluecontainer = '#' + field + ' .fieldprevalues';
    $(prevaluecontainer).children().remove();


    var myPrevalues = new Array();
    var prevaluecount = 0;

    $("#editprevaluelist").children().each(function () {
        var child = $(this);

        var prevalue = "";
        //check if item isn't in edit mode
        if (child.children(":first-child").children().length > 0) {
            prevalue = child.children(":first-child").children(":first-child").attr("value");
        }
        else {
            prevalue = child.children(":first-child").text();
        }

        var prevalueid = "x" + child.attr("id");

        var relattr = "";
        if (child.attr("rel") != undefined) {
            relattr = "rel='" + child.attr("rel") + "'";
        };

        $(prevaluecontainer).append("<div id='" + prevalueid + "' class='prevalue' " + relattr + ">" + prevalue + "</div>");


        myPrevalues[prevaluecount] = prevalue;
        prevaluecount++;

    });


    $('#PreValueModal').prependTo("#modals");

    var label = $('#' + field + ' .fieldexample .fieldexamplelabel');

    $('#' + field + ' .fieldexample').children().remove();
    $('#' + field + ' .fieldexample').text("");

    $('#' + field + ' .fieldexample').append(label);

    $('#' + field + ' .fieldexample').append("<div class='fieldControl'> <div class='fieldeditprevalues'><a href='#' onclick='javascript:ShowUpdatePrevaluesDialog(\"" + field + "\")'>Edit items</a></div></div><br style='clear: both;' />");

    currentPrevaluesUpdateField = "";

    GetFieldPreview(field, $("#"+field).attr('fieldtype'), myPrevalues)
}


function CopyField(fieldId) {

    fieldguid = fieldguid + 1;

    var cloneId = fieldId + "field_" + fieldguid;

    var clone = $('#' + fieldId).clone();

    var origName = clone.attr('fieldcaption');


    clone.attr('id', cloneId);
    clone.removeAttr('rel');
    clone.removeClass('activefield');

    clone.html(clone.html().replace(new RegExp(fieldId, "g"), cloneId));

    clone.attr('fieldcaption', origName + ' ' + lang_copy);

    $('.fieldexample label span:not(.mandatory)', clone).text(origName + ' ' + lang_copy);

    $('.fieldprevalues .prevalue', clone).removeAttr('rel');

    clone.insertAfter($('#fieldeditcontainer' + fieldId));

    var cloneEditContainer = $("<div class='fieldeditcontainer' id='fieldeditcontainer" + cloneId + "'></div>");

    cloneEditContainer.insertAfter(clone);

    SetFieldHover();
}

function ShowFieldTypeSpecificSettings(fieldtype) {

    // move previous onesjquery 
    $("#ftSpecificSettingsContainer").append($("#fieldadditionalsettings .ftSpecificSettings"));

    //clear values
    $("#ftSettingsContainer" + fieldtype + " .ftAdditionalSetting .formControl").each(function () {

        var tagname = $('":first-child', this).tagName().toLowerCase();


        switch (tagname) {
            case "input":
                if ($('input', this).attr('type') == 'text') {
                    $('input', this).val('');
                }

                if ($('input', this).attr('type') == 'checkbox') {
                    $('input', this).attr('checked', false);
                }
                break;
            case "span":

               

                        var pickerId = $('input', this).attr('id').replace('_ContentIdValue_1', '');
                        pickerId = pickerId.replace('_ContentIdValue_2', '');
                        pickerId = pickerId.replace('_ContentIdValue_3', '');
                        pickerId = pickerId.replace('_ContentIdValue_4', '');
                        pickerId = pickerId.replace('_ContentIdValue_5', '');
                        pickerId = pickerId.replace('_ContentIdValue_0', '');
                        pickerId = pickerId.replace('_ContentIdValue', '');
                        clearPickerValue(pickerId);
                
                break;
            default:
                try { $('":first-child', this).val(''); } catch (err) { }
        }

    });

    //show correct ones
    $('#fieldadditionalsettings').append($('#ftSettingsContainer' + fieldtype));

   
}

function LoadAdditionalSettings(field) {

    $("#" + field + " .fieldadditionalsettings").children().each(function () {


        SetInputValue($(":first-child", "#fieldadditionalsettings .ftAdditionalSetting:[rel=" + $(this).attr('rel') + "] .formControl"), $(this).html());
    });
}

function SetInputValue(input, value) {

    var tagname =input.tagName().toLowerCase();
   
    switch (tagname) {
        case "input":
            if (input.attr('type') == 'text') {
                input.val(value);
            }

            if (input.attr('type') == 'checkbox') {
                if (value == "true" || value == "checked") {
                    input.attr('checked', true);
                } else {
                    input.attr('checked', false);
                }
            }
            break;
        case "select":
                input.val(value);
            break;
        case "span":
           
            if ($(':first-child', input).attr('class') == "picker") {
                var picker = $(':first-child', input);

                if(value == null || value == '') {
                    var pickerId = $('input', picker).attr('id').replace('_ContentIdValue_1', '');
                    pickerId = pickerId.replace('_ContentIdValue_2', '');
                    pickerId = pickerId.replace('_ContentIdValue_3', '');
                    pickerId = pickerId.replace('_ContentIdValue_4', '');
                    pickerId = pickerId.replace('_ContentIdValue_5', '');
                    pickerId = pickerId.replace('_ContentIdValue_0', '');
                    pickerId = pickerId.replace('_ContentIdValue', '');
                    clearPickerValue(pickerId);
                }
                else
                {
                    var pickerId = $('input', picker).attr('id').replace('_ContentIdValue_1', '');
                    pickerId = pickerId.replace('_ContentIdValue_2', '');
                    pickerId = pickerId.replace('_ContentIdValue_3', '');
                    pickerId = pickerId.replace('_ContentIdValue_4', '');
                    pickerId = pickerId.replace('_ContentIdValue_5', '');
                    pickerId = pickerId.replace('_ContentIdValue_0', '');
                    pickerId = pickerId.replace('_ContentIdValue', '');
                    setPickerValue(pickerId, value);
                }
            }
            break;
        default:
            try{input.val(value);}catch(err){}
            break;
    }
}

function clearPickerValue(pickerClientId) {

    if (Umbraco.Controls.TreePicker.inst[pickerClientId] != null) {
        Umbraco.Controls.TreePicker.inst[pickerClientId].ClearSelection();
    }
}

function setPickerValue(pickerClientId, value) {

    if (Umbraco.Controls.TreePicker.inst[pickerClientId] != null) {
        var currentpicker = Umbraco.Controls.TreePicker.inst[pickerClientId];

        var jsonObj = [];
        jsonObj.outVal = value;
        currentpicker.SaveSelection(jsonObj);
    }

    //$("#" + currentpicker._itemIdValueClientID).val(value);

    //$.ajax({
    //    type: "POST",
    //    url: currentpicker._webServiceUrl,
    //    data: '{ "nodeId": ' + value + ' }',
    //    contentType: "application/json; charset=utf-8",
    //    dataType: "json",
    //    success: function (msg) {
    //        $("#" + currentpicker._itemTitleClientID).html(msg.d);
    //        $("#" + currentpicker._itemTitleClientID).parent().show();
    //    }
    //});
    

}

function SaveAdditionalSettings(field) {

    $("#" + field + " .fieldadditionalsettings").children().remove();

    $("#fieldadditionalsettings .ftAdditionalSetting .formControl").each(function () {

        var settingvalue = "";

        var tagname = $(':first-child', this).tagName().toLowerCase();

      
        switch (tagname) {
            case "input":
                if ($(':first-child', this).attr('type') == 'text') {
                    settingvalue = $('input', this).val();
                }

                if ($(':first-child', this).attr('type') == 'checkbox') {
                    settingvalue = $('input', this).attr('checked');
                }
                break;
            case "span":
                if ($(':first-child', $(':first-child', this)).attr('class') == "picker") {
                    var picker = $(':first-child', $(':first-child', this));

                    settingvalue = $('input', picker).val();
                }
                break
            case "select":
                settingvalue = $('select', this).val();
                break;
            default:
                try { settingvalue = $(':first-child', this).val(); } catch (err) { }
        }


        $("#" + field + " .fieldadditionalsettings").append("<div class='additionalsetting' rel='" + $(this).parent().attr('rel') + "'>" + settingvalue + "</div>");
    });
}


$.fn.tagName = function () {
    return this.get(0).tagName;
}
