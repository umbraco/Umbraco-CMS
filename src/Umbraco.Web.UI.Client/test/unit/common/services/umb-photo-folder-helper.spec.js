describe('umbPhotoFolderHelper tests', function () {
    var umbPhotoFolderHelper;

    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        umbPhotoFolderHelper = $injector.get('umbPhotoFolderHelper');
    }));

    describe('Calculate row', function () {

        it('Builds a row by scaling the height to fit the width', function () {

            var images = [
                { "properties": [{ "id": 8737, "value": "/media/2173/Save-The-Date.jpg", "alias": "umbracoFile" }, { "id": 8738, "value": "443", "alias": "umbracoWidth" }, { "id": 8739, "value": "500", "alias": "umbracoHeight" }, { "id": 8740, "value": "30830", "alias": "umbracoBytes" }, { "id": 8741, "value": "jpg", "alias": "umbracoExtension" }], "updateDate": "2013-12-10 16:57:56", "createDate": "2013-12-10 14:21:26", "published": false, "owner": { "id": 0, "name": "admin" }, "updater": null, "contentTypeAlias": "Image", "sortOrder": 5, "name": "Save-The-Date.jpg", "id": 1349, "icon": "mediaPhoto.gif", "key": "8eb67ae3-49da-4a25-ab39-185667a9b412", "parentId": 1160, "alias": null, "path": "-1,1142,1160,1349", "metaData": {}, "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1349", "originalWidth": 443, "originalHeight": 500 },
                { "properties": [{ "id": 8742, "value": "/media/2174/IMG_2980.JPG", "alias": "umbracoFile" }, { "id": 8743, "value": "640", "alias": "umbracoWidth" }, { "id": 8744, "value": "480", "alias": "umbracoHeight" }, { "id": 8745, "value": "113311", "alias": "umbracoBytes" }, { "id": 8746, "value": "jpg", "alias": "umbracoExtension" }], "updateDate": "2013-12-10 16:57:51", "createDate": "2013-12-10 14:22:33", "published": false, "owner": { "id": 0, "name": "admin" }, "updater": null, "contentTypeAlias": "Image", "sortOrder": 6, "name": "IMG_2980.JPG", "id": 1350, "icon": "mediaPhoto.gif", "key": "0a9618ea-9b4a-4d34-bf53-e76a0d252048", "parentId": 1160, "alias": null, "path": "-1,1142,1160,1350", "metaData": {}, "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1350", "originalWidth": 640, "originalHeight": 480 },
                { "properties": [{ "id": 8747, "value": "/media/2175/IMG_3023.JPG", "alias": "umbracoFile" }, { "id": 8748, "value": "360", "alias": "umbracoWidth" }, { "id": 8749, "value": "480", "alias": "umbracoHeight" }, { "id": 8750, "value": "106365", "alias": "umbracoBytes" }, { "id": 8751, "value": "jpg", "alias": "umbracoExtension" }], "updateDate": "2013-12-10 16:57:46", "createDate": "2013-12-10 14:39:28", "published": false, "owner": { "id": 0, "name": "admin" }, "updater": null, "contentTypeAlias": "Image", "sortOrder": 7, "name": "IMG_3023.JPG", "id": 1351, "icon": "mediaPhoto.gif", "key": "44cb1ee0-e3d7-40f7-b27c-ae05fb1a8e0c", "parentId": 1160, "alias": null, "path": "-1,1142,1160,1351", "metaData": {}, "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1351", "originalWidth": 360, "originalHeight": 480 },
                { "properties": [{ "id": 8752, "value": "/media/2176/IMG_2055.JPG", "alias": "umbracoFile" }, { "id": 8753, "value": "1024", "alias": "umbracoWidth" }, { "id": 8754, "value": "630", "alias": "umbracoHeight" }, { "id": 8755, "value": "57046", "alias": "umbracoBytes" }, { "id": 8756, "value": "jpg", "alias": "umbracoExtension" }], "updateDate": "2013-12-10 16:57:41", "createDate": "2013-12-10 15:09:47", "published": false, "owner": { "id": 0, "name": "admin" }, "updater": null, "contentTypeAlias": "Image", "sortOrder": 8, "name": "IMG_2055.JPG", "id": 1352, "icon": "mediaPhoto.gif", "key": "8a45465c-251e-44d4-88c5-d86606377105", "parentId": 1160, "alias": null, "path": "-1,1142,1160,1352", "metaData": {}, "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1352", "originalWidth": 1024, "originalHeight": 630 },
                { "properties": [{ "id": 8757, "value": "/media/2177/Signature1.png", "alias": "umbracoFile" }, { "id": 8758, "value": "873", "alias": "umbracoWidth" }, { "id": 8759, "value": "269", "alias": "umbracoHeight" }, { "id": 8760, "value": "105616", "alias": "umbracoBytes" }, { "id": 8761, "value": "png", "alias": "umbracoExtension" }], "updateDate": "2013-12-10 16:57:36", "createDate": "2013-12-10 15:11:53", "published": false, "owner": { "id": 0, "name": "admin" }, "updater": null, "contentTypeAlias": "Image", "sortOrder": 9, "name": "Signature1.png", "id": 1353, "icon": "mediaPhoto.gif", "key": "e12d382a-56f8-4b85-b507-b82aa466cd6f", "parentId": 1160, "alias": null, "path": "-1,1142,1160,1353", "metaData": {}, "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1353", "originalWidth": 873, "originalHeight": 269 }
            ];
            var maxRowHeight = 330;
            var minDisplayHeight = 100;
            var maxRowWidth = 851;
            var idealImgPerRow = 5;
            var margin = 5;

            var result = umbPhotoFolderHelper.buildRow(images, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow, margin);


            expect(result.images.length).toBe(5);

        });

        it('Builds a row by removing an item to scale up to fit', function () {

            var images = [
                { "properties": [{ "id": 8737, "value": "/media/2173/Save-The-Date.jpg", "alias": "umbracoFile" }, { "id": 8738, "value": "443", "alias": "umbracoWidth" }, { "id": 8739, "value": "500", "alias": "umbracoHeight" }, { "id": 8740, "value": "30830", "alias": "umbracoBytes" }, { "id": 8741, "value": "jpg", "alias": "umbracoExtension" }], "updateDate": "2013-12-10 16:57:56", "createDate": "2013-12-10 14:21:26", "published": false, "owner": { "id": 0, "name": "admin" }, "updater": null, "contentTypeAlias": "Image", "sortOrder": 5, "name": "Save-The-Date.jpg", "id": 1349, "icon": "mediaPhoto.gif", "key": "8eb67ae3-49da-4a25-ab39-185667a9b412", "parentId": 1160, "alias": null, "path": "-1,1142,1160,1349", "metaData": {}, "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1349", "originalWidth": 443, "originalHeight": 500 },
                { "properties": [{ "id": 8742, "value": "/media/2174/IMG_2980.JPG", "alias": "umbracoFile" }, { "id": 8743, "value": "640", "alias": "umbracoWidth" }, { "id": 8744, "value": "480", "alias": "umbracoHeight" }, { "id": 8745, "value": "113311", "alias": "umbracoBytes" }, { "id": 8746, "value": "jpg", "alias": "umbracoExtension" }], "updateDate": "2013-12-10 16:57:51", "createDate": "2013-12-10 14:22:33", "published": false, "owner": { "id": 0, "name": "admin" }, "updater": null, "contentTypeAlias": "Image", "sortOrder": 6, "name": "IMG_2980.JPG", "id": 1350, "icon": "mediaPhoto.gif", "key": "0a9618ea-9b4a-4d34-bf53-e76a0d252048", "parentId": 1160, "alias": null, "path": "-1,1142,1160,1350", "metaData": {}, "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1350", "originalWidth": 640, "originalHeight": 480 },
                { "properties": [{ "id": 8747, "value": "/media/2175/IMG_3023.JPG", "alias": "umbracoFile" }, { "id": 8748, "value": "360", "alias": "umbracoWidth" }, { "id": 8749, "value": "480", "alias": "umbracoHeight" }, { "id": 8750, "value": "106365", "alias": "umbracoBytes" }, { "id": 8751, "value": "jpg", "alias": "umbracoExtension" }], "updateDate": "2013-12-10 16:57:46", "createDate": "2013-12-10 14:39:28", "published": false, "owner": { "id": 0, "name": "admin" }, "updater": null, "contentTypeAlias": "Image", "sortOrder": 7, "name": "IMG_3023.JPG", "id": 1351, "icon": "mediaPhoto.gif", "key": "44cb1ee0-e3d7-40f7-b27c-ae05fb1a8e0c", "parentId": 1160, "alias": null, "path": "-1,1142,1160,1351", "metaData": {}, "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1351", "originalWidth": 360, "originalHeight": 480 },
                { "properties": [{ "id": 8752, "value": "/media/2176/IMG_2055.JPG", "alias": "umbracoFile" }, { "id": 8753, "value": "1024", "alias": "umbracoWidth" }, { "id": 8754, "value": "630", "alias": "umbracoHeight" }, { "id": 8755, "value": "57046", "alias": "umbracoBytes" }, { "id": 8756, "value": "jpg", "alias": "umbracoExtension" }], "updateDate": "2013-12-10 16:57:41", "createDate": "2013-12-10 15:09:47", "published": false, "owner": { "id": 0, "name": "admin" }, "updater": null, "contentTypeAlias": "Image", "sortOrder": 8, "name": "IMG_2055.JPG", "id": 1352, "icon": "mediaPhoto.gif", "key": "8a45465c-251e-44d4-88c5-d86606377105", "parentId": 1160, "alias": null, "path": "-1,1142,1160,1352", "metaData": {}, "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1352", "originalWidth": 1024, "originalHeight": 630 },
                { "properties": [{ "id": 8757, "value": "/media/2177/Signature1.png", "alias": "umbracoFile" }, { "id": 8758, "value": "873", "alias": "umbracoWidth" }, { "id": 8759, "value": "269", "alias": "umbracoHeight" }, { "id": 8760, "value": "105616", "alias": "umbracoBytes" }, { "id": 8761, "value": "png", "alias": "umbracoExtension" }], "updateDate": "2013-12-10 16:57:36", "createDate": "2013-12-10 15:11:53", "published": false, "owner": { "id": 0, "name": "admin" }, "updater": null, "contentTypeAlias": "Image", "sortOrder": 9, "name": "Signature1.png", "id": 1353, "icon": "mediaPhoto.gif", "key": "e12d382a-56f8-4b85-b507-b82aa466cd6f", "parentId": 1160, "alias": null, "path": "-1,1142,1160,1353", "metaData": {}, "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1353", "originalWidth": 873, "originalHeight": 269 }                
            ];
            var maxRowHeight = 330;
            var minDisplayHeight = 100;
            var maxRowWidth = 802;
            var idealImgPerRow = 5;
            var margin = 5;

            var result = umbPhotoFolderHelper.buildRow(images, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow, margin);


            expect(result.images.length).toBe(4);
            
        });

        it('Builds a row by removing an item to scale up to fit, then attempts to upscale remaining 2 images, but that doesnt fit so drops another and we end up with one', function () {

            var images = [
                { "properties": [{ "id": 8737, "value": "/media/2173/Save-The-Date.jpg", "alias": "umbracoFile" }, { "id": 8738, "value": "198", "alias": "umbracoWidth" }, { "id": 8739, "value": "220", "alias": "umbracoHeight" }, { "id": 8740, "value": "30830", "alias": "umbracoBytes" }, { "id": 8741, "value": "jpg", "alias": "umbracoExtension" }], "updateDate": "2013-12-10 16:57:56", "createDate": "2013-12-10 14:21:26", "published": false, "owner": { "id": 0, "name": "admin" }, "updater": null, "contentTypeAlias": "Image", "sortOrder": 5, "name": "Save-The-Date.jpg", "id": 1349, "icon": "mediaPhoto.gif", "key": "8eb67ae3-49da-4a25-ab39-185667a9b412", "parentId": 1160, "alias": null, "path": "-1,1142,1160,1349", "metaData": {}, "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1349", "originalWidth": 198, "originalHeight": 220 },
                { "properties": [{ "id": 8742, "value": "/media/2174/IMG_2980.JPG", "alias": "umbracoFile" }, { "id": 8743, "value": "211", "alias": "umbracoWidth" }, { "id": 8744, "value": "500", "alias": "umbracoHeight" }, { "id": 8745, "value": "113311", "alias": "umbracoBytes" }, { "id": 8746, "value": "jpg", "alias": "umbracoExtension" }], "updateDate": "2013-12-10 16:57:51", "createDate": "2013-12-10 14:22:33", "published": false, "owner": { "id": 0, "name": "admin" }, "updater": null, "contentTypeAlias": "Image", "sortOrder": 6, "name": "IMG_2980.JPG", "id": 1350, "icon": "mediaPhoto.gif", "key": "0a9618ea-9b4a-4d34-bf53-e76a0d252048", "parentId": 1160, "alias": null, "path": "-1,1142,1160,1350", "metaData": {}, "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1350", "originalWidth": 211, "originalHeight": 500 },
                { "properties": [{ "id": 8747, "value": "/media/2175/IMG_3023.JPG", "alias": "umbracoFile" }, { "id": 8748, "value": "940", "alias": "umbracoWidth" }, { "id": 8749, "value": "317", "alias": "umbracoHeight" }, { "id": 8750, "value": "106365", "alias": "umbracoBytes" }, { "id": 8751, "value": "jpg", "alias": "umbracoExtension" }], "updateDate": "2013-12-10 16:57:46", "createDate": "2013-12-10 14:39:28", "published": false, "owner": { "id": 0, "name": "admin" }, "updater": null, "contentTypeAlias": "Image", "sortOrder": 7, "name": "IMG_3023.JPG", "id": 1351, "icon": "mediaPhoto.gif", "key": "44cb1ee0-e3d7-40f7-b27c-ae05fb1a8e0c", "parentId": 1160, "alias": null, "path": "-1,1142,1160,1351", "metaData": {}, "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1351", "originalWidth": 940, "originalHeight": 317 }
            ];
            var maxRowHeight = 250;
            var minDisplayHeight = 105;
            var maxRowWidth = 400;
            var idealImgPerRow = 3;
            var margin = 5;

            var result = umbPhotoFolderHelper.buildRow(images, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow, margin);


            expect(result.images.length).toBe(1);

        });

        //SEE: http://issues.umbraco.org/issue/U4-5304
        it('When a row fits with width but its too short, we remove one and scale up, but that comes up too narrow, so we just render what we have', function () {

            var images = [
{ "properties": [{ "value": "/test35.jpg", "alias": "umbracoFile" }, { "value": "1000", "alias": "umbracoWidth" }, { "value": "1041", "alias": "umbracoHeight" }], "contentTypeAlias": "Image", "name": "Test.jpg", "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1349", "originalWidth": 1000, "originalHeight": 1041 },
{ "properties": [{ "value": "/test36.jpg", "alias": "umbracoFile" }, { "value": "1000", "alias": "umbracoWidth" }, { "value": "2013", "alias": "umbracoHeight" }], "contentTypeAlias": "Image", "name": "Test.jpg", "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1349", "originalWidth": 1000, "originalHeight": 2013 },
{ "properties": [{ "value": "/test37.jpg", "alias": "umbracoFile" }, { "value": "840", "alias": "umbracoWidth" }, { "value": "360", "alias": "umbracoHeight" }], "contentTypeAlias": "Image", "name": "Test.jpg", "thumbnail": "/umbraco/UmbracoApi/Images/GetBigThumbnail?mediaId=1349", "originalWidth": 840, "originalHeight": 360 }
            ];
            var maxRowHeight = 250;
            var minDisplayHeight = 105; 
            var maxRowWidth = 400;
            var idealImgPerRow = 3;
            var margin = 5;

            var result = umbPhotoFolderHelper.buildRow(images, maxRowHeight, minDisplayHeight, maxRowWidth, idealImgPerRow, margin);

            expect(result.images.length).toBe(2);

        });
        
        
    });

});