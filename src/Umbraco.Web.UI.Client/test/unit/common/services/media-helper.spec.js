describe('mediaHelper service tests', function () {
    var mediaHelper;

    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        mediaHelper = $injector.get('mediaHelper');
    }));

    describe('formatFileTypes', function () {

        it('will ignore any empty strings passed as arguments', function () {
            
            var result = mediaHelper.formatFileTypes("");
            expect(result.length).toBe(0);

            result = mediaHelper.formatFileTypes(" ");
            expect(result.length).toBe(0);

            result = mediaHelper.formatFileTypes("  ");
            expect(result.length).toBe(0);

            result = mediaHelper.formatFileTypes("  , ");
            expect(result.length).toBe(0);

            result = mediaHelper.formatFileTypes("  , ,,");
            expect(result.length).toBe(0);

        });

        it('includes prefixed dot when formatting', function () {

            var result = mediaHelper.formatFileTypes(".jpg, .png, gif");
            expect(result).toBe(".jpg,.png,.gif");

        });

    });
});
