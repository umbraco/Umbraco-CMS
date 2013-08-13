describe('icon helper tests', function () {
    var umbImageHelper;

    beforeEach(module('umbraco.services'));
    beforeEach(module('umbraco.mocks'));

    beforeEach(inject(function ($injector) {
        umbImageHelper = $injector.get('umbImageHelper');
    }));

    describe('basic utility methods return correct values', function () {

        it('detects an image based file', function () {

            var image1 = "a-jpeg.jpg";
            var image2 = "a-png.png";
            var image3 = "thisisagif.blah.gif";
            var doc1 = "anormaldocument.doc";
            
            expect(umbImageHelper.detectIfImageByExtension(image1)).toBe(true);
            expect(umbImageHelper.detectIfImageByExtension(image2)).toBe(true);
            expect(umbImageHelper.detectIfImageByExtension(image3)).toBe(true);
            expect(umbImageHelper.detectIfImageByExtension(doc1)).toBe(false);
        });
        
        it('gets a thumbnail path', function () {
            
            var image1 = "a-jpeg.jpg";
            var image2 = "a-png.png";
            var image3 = "thisisagif.blah.gif";

            expect(umbImageHelper.getThumbnailFromPath(image1)).toBe("a-jpeg_thumb.jpg");
            expect(umbImageHelper.getThumbnailFromPath(image2)).toBe("a-png_thumb.jpg");
            expect(umbImageHelper.getThumbnailFromPath(image3)).toBe("thisisagif.blah_thumb.jpg");
        });
        
    });
});