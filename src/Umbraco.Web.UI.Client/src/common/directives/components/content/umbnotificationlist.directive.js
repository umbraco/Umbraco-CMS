(function () {
    'use strict';

    function umbNotificationList() {

        var vm = this;
        
    }

    var umbNotificationListComponent = {
        templateUrl: 'views/components/content/umb-notification-list.html',
        bindings: {
            notifications: "<"
        },
        controllerAs: 'vm',
        controller: umbNotificationList
    };

    angular.module("umbraco.directives")
        .component('umbNotificationList', umbNotificationListComponent);

})();
