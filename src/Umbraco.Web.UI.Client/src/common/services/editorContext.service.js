angular.module('umbraco.services')
.service('editorContextService', function () {
        var context;
        return {
            getContext:function () {
                return context;
            },
            setContext:function (value) {
                context = value;
            }
        };
    });