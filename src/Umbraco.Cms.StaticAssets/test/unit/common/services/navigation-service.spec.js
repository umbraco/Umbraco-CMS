describe('navigation services tests', function () {
    var navigationService;

    beforeEach(module('umbraco.services'));
    beforeEach(module('ngRoute'));

    beforeEach(inject(function ($injector) {
        navigationService = $injector.get('navigationService');
    }));

    describe('determine if route change causes navigation', function () {

        it('navigates when parameters added', function () {
            var currParams = { section: "content", id: 123 };
            var nextParams = { section: "content", id: 123, create: true };
            var result = navigationService.isRouteChangingNavigation(currParams, nextParams);
            expect(result).toBe(true);
        });

        it('navigates when parameters removed', function () {
            var currParams = { section: "content", id: 123, create: true };
            var nextParams = { section: "content", id: 123 };
            var result = navigationService.isRouteChangingNavigation(currParams, nextParams);
            expect(result).toBe(true);
        });

        it('does not navigate when non routing parameters added', function () {
            var currParams = { section: "content", id: 123 };
            var nextParams = { section: "content", id: 123, mculture: "abc", cculture: "xyz" };
            var result = navigationService.isRouteChangingNavigation(currParams, nextParams);
            expect(result).toBe(false);
        });

        it('does not navigate when non routing parameters changed', function () {
            var currParams = { section: "content", id: 123, mculture: "abc" };
            var nextParams = { section: "content", id: 123, mculture: "ooo", cculture: "xyz" };
            var result = navigationService.isRouteChangingNavigation(currParams, nextParams);
            expect(result).toBe(false);
        });

        it('does not navigate when non routing parameters removed', function () {
            var currParams = { section: "content", id: 123, mculture: "abc", cculture: "xyz" };
            var nextParams = { section: "content", id: 123, cculture: "xyz" };
            var result = navigationService.isRouteChangingNavigation(currParams, nextParams);
            expect(result).toBe(false);
        });

    });
    
});
