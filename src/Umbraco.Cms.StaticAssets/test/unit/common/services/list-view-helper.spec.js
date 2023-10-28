describe('list view helper tests', function () {
    var $scope, listViewHelper;

    beforeEach(module('LocalStorageModule'));
    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        $scope = $injector.get('$rootScope');
        listViewHelper = $injector.get('listViewHelper');
    }));

    describe('getButtonPermissions', function () {

        it('should update the currentIdsWithPermissions dictionary', function () {

            var currentIdsWithPermissions = {};
            var result = listViewHelper.getButtonPermissions({ "1234": ["A", "B", "C"] }, currentIdsWithPermissions);

            expect(_.has(currentIdsWithPermissions, "1234")).toBe(true);
            expect(_.keys(currentIdsWithPermissions).length).toEqual(1);

        });

        it('returns button permissions', function () {

            var currentIdsWithPermissions = {};
            var result1 = listViewHelper.getButtonPermissions({ "1234": ["O", "C", "D", "M", "U"] }, currentIdsWithPermissions);

            expect(result1["canCopy"]).toBe(true);
            expect(result1["canCreate"]).toBe(true);
            expect(result1["canDelete"]).toBe(true);
            expect(result1["canMove"]).toBe(true);
            expect(result1["canPublish"]).toBe(true);
            expect(result1["canUnpublish"]).toBe(true);

            var result2 = listViewHelper.getButtonPermissions({ "1234": ["A", "B"] }, currentIdsWithPermissions);

            expect(result2["canCopy"]).toBe(false);
            expect(result2["canCreate"]).toBe(false);
            expect(result2["canDelete"]).toBe(false);
            expect(result2["canMove"]).toBe(false);
            expect(result2["canPublish"]).toBe(false);
            expect(result2["canUnpublish"]).toBe(false);

        });

    });
});