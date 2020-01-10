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
          var not4 = notifications.warning("warning", "");
          var not5 = notifications.info("info", "something informative happened");

        expect(notifications.getCurrent().length).toBe(5);

        //remove at index 0
        notifications.remove(0);

        expect(notifications.getCurrent().length).toEqual(4);
        expect(notifications.getCurrent()[0].headline).toBe("error: ");
        expect(notifications.getCurrent()[1].headline).toBe("warning: ");
          expect(notifications.getCurrent()[2].headline).toBe("warning");
          expect(notifications.getCurrent()[3].headline).toBe("info: ");

        notifications.removeAll();
        expect(notifications.getCurrent().length).toEqual(0);
      });

    });
});
