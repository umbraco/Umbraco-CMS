/**
 * @ngdoc service
 * @name umbraco.services.windowResizeListener
 * @function
 *
 * @description
 * A single window resize listener... we don't want to have more than one in theory to ensure that
 * there aren't too many events raised. This will debounce the event with 100 ms intervals and force
 * a $rootScope.$apply when changed and notify all listeners
 *
 */
function windowResizeListener($rootScope) {

    var WinResizer = (function () {
        var registered = [];
        var inited = false;        
        var resize = _.debounce(function(ev) {
            notify();
        }, 100);
        var notify = function () {
            var h = $(window).height();
            var w = $(window).width();
            //execute all registrations inside of a digest
            $rootScope.$apply(function() {
                for (var i = 0, cnt = registered.length; i < cnt; i++) {
                    registered[i].apply($(window), [{ width: w, height: h }]);
                }
            });
        };
        return {
            register: function (fn) {
                registered.push(fn);
                if (inited === false) {
                    $(window).on('resize', resize);
                    inited = true;
                }
            },
            unregister: function (fn) {
                var index = registered.indexOf(fn);
                if (index > -1) {
                    registered.splice(index, 1);
                }
            }
        };
    }());

    return {

        /**
         * Register a callback for resizing
         * @param {Function} cb 
         */
        register: function (cb) {
            WinResizer.register(cb);
        },

        /**
         * Removes a registered callback
         * @param {Function} cb 
         */
        unregister: function(cb) {
            WinResizer.unregister(cb);
        }

    };
}
angular.module('umbraco.services').factory('windowResizeListener', windowResizeListener);
