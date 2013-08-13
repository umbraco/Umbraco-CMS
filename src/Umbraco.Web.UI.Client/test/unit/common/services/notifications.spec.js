describe('notification tests', function () {
    var $scope, notifications;

    beforeEach(module('umbraco.services'));

    beforeEach(inject(function($injector) {
      $scope = $injector.get('$rootScope');
      notifications = $injector.get('notificationsService');
    }));

    describe('global notifications crud', function () {

      it('should allow to add, get and remove notifications', function () {
        var not1 = notifications.success("success", "something great happened");
        var not2 = notifications.error("error", "something great happened");
        var not3 = notifications.warning("warning", "something great happened");

        expect(notifications.getCurrent().length).toBe(3);

        //remove at index 0
        notifications.remove(0);

        expect(notifications.getCurrent().length).toEqual(2);
        expect(notifications.getCurrent()[0].headline).toBe("error");

        notifications.removeAll();
        expect(notifications.getCurrent().length).toEqual(0);
      });

    });
});