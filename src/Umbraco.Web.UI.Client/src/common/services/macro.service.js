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

        /** parses the special macro syntax like <?UMBRACO_MACRO macroAlias="Map" /> and returns an object with the macro alias and it's parameters */
        parseMacroSyntax: function (syntax) {

            //This regex will match an alias of anything except characters that are quotes or new lines (for legacy reasons, when new macros are created
            // their aliases are cleaned an invalid chars are stripped)
            var expression = /(<\?UMBRACO_MACRO (?:.+?)?macroAlias=["']([^\"\'\n\r]+?)["'][\s\S]+?)(\/>|>.*?<\/\?UMBRACO_MACRO>)/i;
            var match = expression.exec(syntax);
            if (!match || match.length < 3) {
                return null;
            }
            var alias = match[2];

            //this will leave us with just the parameters
            var paramsChunk = match[1].trim().replace(new RegExp("UMBRACO_MACRO macroAlias=[\"']" + alias + "[\"']"), "").trim();

            var paramExpression = /(\w+?)=['\"]([\s\S]*?)['\"]/g;

            var paramMatch;
            var returnVal = {
                macroAlias: alias,
                macroParamsDictionary: {}
            };
            while (paramMatch = paramExpression.exec(paramsChunk)) {
                returnVal.macroParamsDictionary[paramMatch[1]] = paramMatch[2];
            }
            return returnVal;
        },

        /**
         * @ngdoc function
         * @name umbraco.services.macroService#generateMacroSyntax
         * @methodOf umbraco.services.macroService
         * @function
         *
         * @description
         * generates the syntax for inserting a macro into a rich text editor - this is the very old umbraco style syntax
         *
         * @param {object} args an object containing the macro alias and it's parameter values
         */
        generateMacroSyntax: function (args) {

            // <?UMBRACO_MACRO macroAlias="BlogListPosts" />

            var macroString = '<?UMBRACO_MACRO macroAlias=\"' + args.macroAlias + "\" ";

            if (args.macroParamsDictionary) {

                _.each(args.macroParamsDictionary, function (val, key) {
                    //check for null
                    val = val ? val : "";
                    //need to detect if the val is a string or an object
                    var keyVal;
                    if (Utilities.isString(val)) {
                        keyVal = key + "=\"" + (val ? val : "") + "\" ";
                    }
                    else {
                        //if it's not a string we'll send it through the json serializer
                        var json = Utilities.toJson(val);
                        //then we need to url encode it so that it's safe
                        var encoded = encodeURIComponent(json);
                        keyVal = key + "=\"" + encoded + "\" ";
                    }

                    macroString += keyVal;
                });

            }

            macroString += "/>";

            return macroString;
        },

        /**
         * @ngdoc function
         * @name umbraco.services.macroService#generateMvcSyntax
         * @methodOf umbraco.services.macroService
         * @function
         *
         * @description
         * generates the syntax for inserting a macro into an mvc template
         *
         * @param {object} args an object containing the macro alias and it's parameter values
         */
        generateMvcSyntax: function (args) {

            var macroString = "@await Umbraco.RenderMacroAsync(\"" + args.macroAlias + "\"";

            var hasParams = false;
            var paramString;
            if (args.macroParamsDictionary) {

                paramString = ", new {";

                _.each(args.macroParamsDictionary, function(val, key) {

                    hasParams = true;

                    var keyVal = key + "=\"" + (val ? val : "") + "\", ";

                    paramString += keyVal;
                });

                //remove the last ,
                paramString = paramString.trimEnd(", ");

                paramString += "}";
            }
            if (hasParams) {
                macroString += paramString;
            }

            macroString += ")";
            return macroString;
        },

        collectValueData: function(macro, macroParams, renderingEngine) {

            var paramDictionary = {};
            var macroAlias = macro.alias;
            if (!macroAlias) {
                throw "The macro object does not contain an alias";
            }

            var syntax;

            _.each(macroParams, function (item) {

                var val = item.value;

                if (item.value !== null && item.value !== undefined && !_.isString(item.value)) {
                    try {
                        val = Utilities.toJson(val);
                    }
                    catch (e) {
                        // not json
                    }
                }

                //each value needs to be xml escaped!! since the value get's stored as an xml attribute
                paramDictionary[item.alias] = _.escape(val);

            });

            //get the syntax based on the rendering engine
            if (renderingEngine && renderingEngine === "Mvc") {
                syntax = this.generateMvcSyntax({ macroAlias: macroAlias, macroParamsDictionary: paramDictionary });
            }
            else {
                syntax = this.generateMacroSyntax({ macroAlias: macroAlias, macroParamsDictionary: paramDictionary });
            }

            var macroObject = {
                "macroParamsDictionary": paramDictionary,
                "macroAlias": macroAlias,
                "syntax": syntax
            };

            return macroObject;

        }

    };

}

angular.module('umbraco.services').factory('macroService', macroService);
