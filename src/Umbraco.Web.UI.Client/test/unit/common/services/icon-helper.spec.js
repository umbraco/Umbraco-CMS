describe('icon helper tests', function () {
    var iconHelper;

    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        iconHelper = $injector.get('iconHelper');
    }));

    describe('converting icon formats to Belle formats', function () {

        it('detects a file based icon', function () {
            var fileBased = "this-is-file-based.jpg";
            var legacyBased = ".legacy-class";
            var belleBased = "normal-class";
            
            expect(iconHelper.isFileBasedIcon(fileBased)).toBe(true);
            expect(iconHelper.isFileBasedIcon(legacyBased)).toBe(false);
            expect(iconHelper.isFileBasedIcon(belleBased)).toBe(false);
        });
        
        it('detects a legacy icon', function () {
            var fileBased = "this-is-file-based.jpg";
            var legacyBased = ".legacy-class";
            var belleBased = "normal-class";

            expect(iconHelper.isLegacyIcon(fileBased)).toBe(false);
            expect(iconHelper.isLegacyIcon(legacyBased)).toBe(true);
            expect(iconHelper.isLegacyIcon(belleBased)).toBe(false);
        });
        
        it('converts from legacy icon', function () {
            var legacyBased = ".sprTreeSettingDomain";

            expect(iconHelper.convertFromLegacyIcon(legacyBased)).toBe("icon-home");
            
        });

    });
});