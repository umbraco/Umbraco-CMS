
//TODO: WE NEED TO CONVERT ALL OF THESE METHODS TO PROXY TO OUR APPLICATION SINCE MANY CUSTOM APPS USE THIS!

Umbraco.Sys.registerNamespace("Umbraco.Application");

(function($) {
    Umbraco.Application.SpeechBubble = function() {

        /**
         * @ngdoc function
         * @name getRootScope
         * @methodOf UmbClientMgr
         * @function
         *
         * @description
         * Returns the root angular scope
         */
        function getRootScope() {
            return angular.element(document.getElementById("umbracoMainPageBody")).scope();
        }
        
        /**
         * @ngdoc function
         * @name getRootInjector
         * @methodOf UmbClientMgr
         * @function
         *
         * @description
         * Returns the root angular injector
         */
        function getRootInjector() {
            return angular.element(document.getElementById("umbracoMainPageBody")).injector();
        }


        return {
            
            /**
             * @ngdoc function
             * @name ShowMessage
             * @methodOf Umbraco.Application.SpeechBubble
             * @function
             *
             * @description
             * Proxies a legacy call to the new notification service
             */               
            ShowMessage: function (icon, header, message) {
                //get our angular navigation service
                var injector = getRootInjector();
                var notifyService = injector.get("notificationsService");

                switch(icon){
                    case "save":
                        notifyService.success(header, message);
                        break;
                    case "success":
                        notifyService.success(header, message);
                        break;    
                    case "warning":
                        notifyService.warning(header, message);
                        break;
                    case "error":
                        notifyService.error(header, message);
                        break;
                    default:
                        notifyService.info(header, message);
                }

                
            }
        };
    };
})(jQuery);

//define alias for use throughout application
var UmbSpeechBubble = new Umbraco.Application.SpeechBubble();
