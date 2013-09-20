/**
 * @ngdoc service
 * @name umbraco.services.macroService
 *
 *  
 * @description
 * A service to return macro information such as generating syntax to insert a macro into an editor
 */
function macroService() {

    return {
        
        /**
         * @ngdoc function
         * @name generateWebFormsSyntax
         * @methodOf umbraco.services.macroService
         * @function    
         *
         * @description
         * generates the syntax for inserting a macro into a webforms templates
         * 
         * @param {object} args an object containing the macro alias and it's parameter values
         */
        generateWebFormsSyntax: function(args) {
            
            var macroString = '<umbraco:Macro ';

            if (args.macroParams) {
                for (var i = 0; i < args.macroParams.length; i++) {

                    var keyVal = args.macroParams[i].alias + "=\"" + (args.macroParams[i].value ? args.macroParams[i].value : "") + "\" ";
                    macroString += keyVal;
                }
            }

            macroString += "Alias=\"" + args.macroAlias + "\" runat=\"server\"></umbraco:Macro>";

            return macroString;
        },
        
        /**
         * @ngdoc function
         * @name generateMvcSyntax
         * @methodOf umbraco.services.macroService
         * @function    
         *
         * @description
         * generates the syntax for inserting a macro into an mvc template
         * 
         * @param {object} args an object containing the macro alias and it's parameter values
         */
        generateMvcSyntax: function (args) {

            var macroString = "@Umbraco.RenderMacro(\"" + args.macroAlias + "\"";

            if (args.macroParams && args.macroParams.length > 0) {
                macroString += ", new {";
                for (var i = 0; i < args.macroParams.length; i++) {
                    
                    var keyVal = args.macroParams[i].alias + "=\"" + (args.macroParams[i].value ? args.macroParams[i].value : "") + "\"";

                    macroString += keyVal;
                    
                    if (i < args.macroParams.length - 1) {
                        macroString += ", ";
                    }
                }
                macroString += "}";
            }

            macroString += ")";
            return macroString;
        }

    };

}

angular.module('umbraco.services').factory('macroService', macroService);