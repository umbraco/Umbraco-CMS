var umbModuleToInsertAlias = 'CWS_twitter&target'

function umbShowModuleContainerSelectors() {

    jQuery(".umbModuleContainer").each(function () {

        if (jQuery(this).children().size() > 0) {
            jQuery(this).prepend("<div class='umbModuleContainerSelector' rel='prepend'>Insert module here</div>");
        }

        jQuery(this).append("<div class='umbModuleContainerSelector' rel='append'>Insert module here</div>");

    });

    jQuery(".umbModuleContainerSelector").click(function () {

        Umbraco.Controls.ModalWindow().open('/umbraco/LiveEditing/Modules/SkinModule/ModuleInjector.aspx?macroAlias='+umbModuleToInsertAlias +'&target=' + jQuery(this).parent().attr('id') + "&type=" + jQuery(this).attr('rel'), 'Insert module', true, 550, 550, 50, 0, ['.modalbuton'], null);

    });
}

function umbRemoveModuleContainerSelectors() {
    jQuery(".umbModuleContainerSelector").remove();
}

function umbInsertModule(container,macro,type) {
    umbRemoveModuleContainerSelectors();

    
    if (type == "append") {
        jQuery("#" + container).append("<div class='umbModuleContainerPlaceHolder'>Working...</div>");
    } else {
        jQuery("#" + container).prepend("<div class='umbModuleContainerPlaceHolder'>Working...</div>");
    }

    UmbracoCommunicator.SendClientMessage("injectmodule", container + ";" + macro + ";" + type);

    //need to lose this replace calls + supply current page id;

    jQuery.post("/umbraco/LiveEditing/Modules/SkinModule/ModuleInjectionMacroRenderer.aspx?tag=" + macro.replace('>','').replace('<','').replace('</umbraco:Macro>',''),
     function (data) {
         jQuery(".umbModuleContainerPlaceHolder").html(data);

         UmbSpeechBubble.ShowMessage("Info", "Module", "Module inserted");
    });

}