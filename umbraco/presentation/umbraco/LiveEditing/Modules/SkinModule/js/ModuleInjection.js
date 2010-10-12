var umbModuleToInsertAlias;

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

    UmbracoCommunicator.SendClientMessage("injectmodule", container + ";" + macro + ";" + type);

    //need to lose these replace calls

    jQuery.post(umbCurrentUmbracoDir + "/LiveEditing/Modules/SkinModule/ModuleInjectionMacroRenderer.aspx?tag=" + macro.replace('>', '').replace('<', '').replace('</umbraco:Macro>', '') + "&umbPageID=" + umbCurrentPageId,
     function (data) {
         jQuery(".umbModuleContainerPlaceHolder").html(data);

         UmbSpeechBubble.ShowMessage("Info", "Module", "Module inserted");
    });

}