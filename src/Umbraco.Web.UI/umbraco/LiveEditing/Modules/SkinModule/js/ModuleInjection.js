var umbModuleToInsertAlias;


function umbMakeModulesSortable() {

    if (jQuery('.umbModuleContainer').length > 0) {
        jQuery('.umbModuleContainer').sortable({
            connectWith: '.umbModuleContainer',
            items: '.umbModule',
            stop: function (event, ui) {

                UmbracoCommunicator.SendClientMessage("movemodule", ui.item.attr('id') + ";" + ui.item.parent().attr('id') + ";" + jQuery('.umbModule', ui.item.parent()).index(ui.item));
            }
        });
    }

}

function umbSelectModule(alias,sender) {
    jQuery('#modules').hide();
    jQuery('#moduleSelect').show();
    umbShowModuleContainerSelectors(jQuery('span', sender).html());
    umbModuleToInsertAlias = alias;

    jQuery('.selectedModule').html(jQuery('span',sender).html());

}
function umbInstallModuleAndGetAlias(guid,name,sender) {

    jQuery('#modules').hide();
    jQuery('.selectedModule').html(name);
    jQuery("#installingModule").show();

    jQuery.post(umbCurrentUmbracoDir + "/LiveEditing/Modules/SkinModule/ModuleInstaller.aspx?guid=" + guid + "&name=" + name,
     function (data) {

         if (data == "error") {

         }
         else {
             jQuery("#installingModule").hide();
             jQuery('#moduleSelect').show();
             umbShowModuleContainerSelectors(jQuery('span', sender).html());
             umbModuleToInsertAlias = data;

             jQuery(sender).attr("onclick", "");

             jQuery(sender).click(function () {
                 umbSelectModule(data, this); 
                 return false;
             });
         }


     });

}
function umbShowModuleSelection() {

    umbRemoveModuleContainerSelectors();

    jQuery("#moduleSelect").hide();
    jQuery("#modules").show();

    jQuery(".ModuleSelector").show();

}

function umbShowModuleContainerSelectors(moduleName) {

    jQuery(".umbModuleContainer").each(function () {

        if (jQuery(this).children().size() > 0) {
            jQuery(this).prepend("<div class='umbModuleContainerSelector' rel='prepend'>Insert module here</div>");
        }

        jQuery(this).append("<div class='umbModuleContainerSelector' rel='append'>Insert module here</div>");

    });

    jQuery(".umbModuleContainerSelector").click(function () {

        jQuery(".ModuleSelector").hide();
        Umbraco.Controls.ModalWindow().open(umbCurrentUmbracoDir + '/LiveEditing/Modules/SkinModule/ModuleInjector.aspx?macroAlias=' + umbModuleToInsertAlias + '&target=' + jQuery(this).parent().attr('id') + "&type=" + jQuery(this).attr('rel'), 'Insert ' + moduleName + ' module', true, 550, 550, 50, 0, ['.modalbuton'], null);

    });
}

function umbRemoveModuleContainerSelectors() {
    jQuery(".umbModuleContainerSelector").remove();
}

function umbInsertModule(container,macro,type) {
    umbRemoveModuleContainerSelectors();

    var working = "<div class='umbModuleContainerPlaceHolder'><img src='" + umbCurrentUmbracoDir + "/LiveEditing/Modules/SkinModule/images/loader.gif' />Inserting module...</div>";
    
    if (type == "append") {
        jQuery("#" + container).append(working);
    } else {
        jQuery("#" + container).prepend(working);
    }

    var moduleguid = guid();

   

    UmbracoCommunicator.SendClientMessage("injectmodule", container + ";" + "<div id='"+ moduleguid +"' class='umbModule'>" + macro + "</div>;" + type);

    //need to lose these replace calls

    jQuery.post(umbCurrentUmbracoDir + "/LiveEditing/Modules/SkinModule/ModuleInjectionMacroRenderer.aspx?tag=" + macro.replace('>', '').replace('<', '').replace('</umbraco:Macro>', '') + "&umbPageID=" + umbCurrentPageId,
     function (data) {
         jQuery(".umbModuleContainerPlaceHolder").html("<div id='" + moduleguid + "' class='umbModule'>" + data + "</div>;");

         UmbSpeechBubble.ShowMessage("Info", "Module", "Module inserted");
    });

 }


 function S4() {
     return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
 }
 function guid() {
     return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
 }

 //startup stuff
 umbMakeModulesSortable();