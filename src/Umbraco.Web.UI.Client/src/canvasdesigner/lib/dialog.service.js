
/*********************************************************************************************************/
/* jQuery UI Slider plugin wrapper */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner").factory('dialogService', function ($rootScope, $q, $http, $timeout, $compile, $templateCache) {

    function closeDialog(dialog, destroyScope) {
        if (dialog.element) {
            dialog.element.removeClass("selected");
            dialog.element.html("");

            if (destroyScope) {
                dialog.scope.$destroy();
            }
        }
    }

    function open() {
    }

    return {

        open: function (options) {

            var defaults = {
                template: "",
                callback: undefined,
                change: undefined,
                cancel: undefined,
                element: undefined,
                dialogItem: undefined,
                dialogData: undefined
            };

            var dialog = angular.extend(defaults, options);
            var destroyScope = true;

            if (options && options.scope) {
                destroyScope = false;
            }
            var scope = (options && options.scope) || $rootScope.$new();

            // Save original value for cancel action
            var originalDialogItem = angular.copy(dialog.dialogItem);

            dialog.element = $(".float-panel");


            /************************************/
            // Close dialog if the user clicks outside the dialog. (Not working well with colorpickers and datepickers)
            $(document).mousedown(function (e) {
                var container = dialog.element;
                if (!container.is(e.target) && container.has(e.target).length === 0) {
                    closeDialog(dialog, destroyScope);
                }
            });
            /************************************/

            
            $q.when($templateCache.get(dialog.template) || $http.get(dialog.template, { cache: true }).then(function (res) { return res.data; }))
            .then(function onSuccess(template) {

                dialog.element.html(template);

                $timeout(function () {
                    $compile(dialog.element)(scope);
                });

                dialog.element.addClass("selected")

                scope.cancel = function () {
                    if (dialog.cancel) {
                        dialog.cancel(originalDialogItem);
                    }
                    closeDialog(dialog, destroyScope);
                }

                scope.change = function (data) {
                    if (dialog.change) {
                        dialog.change(data);
                    }
                }

                scope.submit = function (data) {
                    if (dialog.callback) {
                        dialog.callback(data);
                    }
                    closeDialog(dialog, destroyScope);
                };

                scope.close = function () {
                    closeDialog(dialog, destroyScope);
                }

                scope.dialogData = dialog.dialogData;
                scope.dialogItem = dialog.dialogItem;

                dialog.scope = scope;

            });

            return dialog;

        },

        close: function() {
            var modal = $(".float-panel");
            modal.removeClass("selected")
        }

    }


});