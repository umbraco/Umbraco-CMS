describe('file manager tests', function () {
    var fileManager;

    beforeEach(module('umbraco.services'));

    beforeEach(inject(function ($injector) {
        fileManager = $injector.get('fileManager');
    }));

    describe('file management', function () {

        it('adding a file adds to the collection', function () {
            fileManager.setFiles({
                propertyAlias: 'testProp',
                files: ["testFile"]
            });
            expect(fileManager.getFiles().length).toBe(1);
        });

        it('adding a file with the same property id replaces the existing one', function () {
            fileManager.setFiles({
                propertyAlias: 'testProp',
                files: ["testFile"]
            });
            fileManager.setFiles({
                propertyAlias: 'testProp',
                files: ["testFile2"]
            });
            expect(fileManager.getFiles().length).toBe(1);
            expect(fileManager.getFiles()[0].file).toBe("testFile2");
        });
        
        it('clears all files', function () {
            fileManager.setFiles({
                propertyAlias: 'testProp',
                files: ["testFile"]
            });
            fileManager.setFiles({
                propertyAlias: 'testProp2',
                files: ["testFile"]
            });
            expect(fileManager.getFiles().length).toBe(2);
            fileManager.clearFiles();
            expect(fileManager.getFiles().length).toBe(0);
        });

    });
});
