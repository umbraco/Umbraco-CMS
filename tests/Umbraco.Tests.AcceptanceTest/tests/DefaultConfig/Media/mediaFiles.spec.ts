import {expect} from "@playwright/test";
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.describe('media File Types', () => {

    test.beforeEach(async ({page, umbracoApi, umbracoUi}) => {
        // TODO: REMOVE THIS WHEN SQLITE IS FIXED
        // Wait so we don't bombard the API
        await page.waitForTimeout(1000);
        await umbracoApi.login();
        await umbracoUi.goToSection(ConstantHelper.sections.media);
        await umbracoApi.media.deleteAllMedia();
    });
    
    test.describe('create each File Types', () => {
        test('create Article', async ({page, umbracoApi, umbracoUi}) => {
            const articleName = "Article";
            const fileName = "Article.pdf";
            const path = fileName;
            const mimeType = "application/pdf";
            await umbracoApi.media.ensureNameNotExists(articleName);

            // Action
            await umbracoApi.media.createArticleWithFile(articleName, fileName, path, mimeType);
            await umbracoUi.refreshMediaTree();

            // Assert
            await expect(page.locator(".umb-tree-item__inner", {hasText: articleName})).toBeVisible();

            // Clean
            await umbracoApi.media.ensureNameNotExists(articleName);
        });

        test('create Audio', async ({page, umbracoApi, umbracoUi}) => {
            const audioName = "Audio";
            const fileName = "Audio.mp3";
            const path = fileName;
            const mimeType = "audio/mp3"
            await umbracoApi.media.ensureNameNotExists(audioName);

            // Action
            await umbracoApi.media.createAudioWithFile(audioName, fileName, path, mimeType);
            await umbracoUi.refreshMediaTree();

            // Assert
            await expect(page.locator(".umb-tree-item__inner", {hasText: audioName})).toBeVisible();

            // Clean
            await umbracoApi.media.ensureNameNotExists(audioName);
        });

        test('create File', async ({page, umbracoApi, umbracoUi}) => {
            const fileItemName = "File";
            const fileName = "File.txt";
            const path = fileName;
            const mimeType = "*/*";
            await umbracoApi.media.ensureNameNotExists(fileItemName);
            
            // Action
            await umbracoApi.media.createFileWithFile(fileItemName, fileName, path, mimeType);
            await umbracoUi.refreshMediaTree();

            // Assert
            await expect(page.locator(".umb-tree-item__inner", {hasText: fileItemName})).toBeVisible();

            // Clean
            await umbracoApi.media.ensureNameNotExists(fileItemName);
        });

        test('create Folder', async ({page, umbracoApi, umbracoUi}) => {
            const folderName = "Folder";
            await umbracoApi.media.ensureNameNotExists(folderName);

            // Action
            await umbracoApi.media.createDefaultFolder(folderName);
            await umbracoUi.refreshMediaTree();

            // Assert
            await expect(page.locator(".umb-tree-item__inner", {hasText: folderName})).toBeVisible();

            // Clean
            await umbracoApi.media.ensureNameNotExists(folderName);
        });

        test('create Image', async ({page, umbracoApi, umbracoUi}) => {
            const imageName = "Umbraco";
            const umbracoFileValue = {"src": "Umbraco.png"};
            const fileName = "Umbraco.png"
            const path = fileName;
            const mimeType = "image/png";
            await umbracoApi.media.ensureNameNotExists(imageName);

            // Action
            await umbracoApi.media.createImageWithFile(imageName, umbracoFileValue, fileName, path, mimeType);
            await umbracoUi.refreshMediaTree();

            // Assert
            await expect(page.locator(".umb-tree-item__inner", {hasText: imageName})).toBeVisible();

            // Clean
            await umbracoApi.media.ensureNameNotExists(imageName);
        });

        test('create VectorGraphics(SVG)', async ({page, umbracoApi, umbracoUi}) => {
            const vectorGraphicsName = 'VectorGraphics';
            const fileName = "VectorGraphics.svg";
            const path = fileName;
            const mimeType = "image/svg+xml";
            await umbracoApi.media.ensureNameNotExists(vectorGraphicsName);

            // Action
            await umbracoApi.media.createVectorGraphicsWithFile(vectorGraphicsName, fileName, path, mimeType);
            await umbracoUi.refreshMediaTree();

            // Assert
            await expect(page.locator(".umb-tree-item__inner", {hasText: vectorGraphicsName})).toBeVisible();

            // Clean
            await umbracoApi.media.ensureNameNotExists(vectorGraphicsName);
        });

        test('create Video', async ({page, umbracoApi, umbracoUi}) => {
            const videoName = "Video";
            const fileName = "Video.mp4";
            const path = fileName;
            const mimeType = "video/mp4";
            await umbracoApi.media.ensureNameNotExists(videoName);

            // Action
            await umbracoApi.media.createVideoWithFile(videoName, fileName, path, mimeType);
            await umbracoUi.refreshMediaTree();

            // Assert
            await expect(page.locator(".umb-tree-item__inner", {hasText: videoName})).toBeVisible();

            // Clean
            await umbracoApi.media.ensureNameNotExists(videoName);
        });
    });

    test.describe('create each File Types in a Folder', () => {
        test('create Article in a Folder', async ({page, umbracoApi, umbracoUi}) => {
            const parentName = 'ParentArticleFolder';
            const childName = 'ChildArticle';
            await umbracoApi.media.ensureNameNotExists(parentName);
            await umbracoApi.media.ensureNameNotExists(childName);
            const articlePath = "./fixtures/mediaLibrary/Article.pdf"

            // Action
            await umbracoApi.media.createDefaultFolder(parentName);
            await umbracoUi.refreshMediaTree();
            await umbracoUi.clickElement(umbracoUi.getTreeItem('media', [parentName]), {
                button: "right",
                force: true
            });
            await page.locator('[data-element="action-create"]').click();
            await page.locator('[data-element="action-umbracoMediaArticle"]').click();
            await page.locator('[data-element="editor-name-field"]').type(childName);
            await umbracoUi.fileUploader(articlePath);

            // Assert
            await expect(page.locator('.alert-success')).toBeVisible();

            // Clean
            await umbracoApi.media.ensureNameNotExists(parentName);
            await umbracoApi.media.ensureNameNotExists(childName);
        });
        test('create Audio in a Folder', async ({page, umbracoApi, umbracoUi}) => {
            const parentName = 'ParentAudioFolder';
            const childName = 'ChildAudio';
            await umbracoApi.media.ensureNameNotExists(parentName);
            await umbracoApi.media.ensureNameNotExists(childName);
            const childPath = "./fixtures/mediaLibrary/Audio.mp3"

            // Action
            await umbracoApi.media.createDefaultFolder(parentName);
            await umbracoUi.refreshMediaTree();
            await umbracoUi.clickElement(umbracoUi.getTreeItem('media', [parentName]), {
                button: "right",
                force: true
            });
            await page.locator('[data-element="action-create"]').click();
            await page.locator('[data-element="action-umbracoMediaAudio"]').click();
            await page.locator('[data-element="editor-name-field"]').type(childName);
            await umbracoUi.fileUploader(childPath);

            // Assert
            await expect(page.locator('.alert-success')).toBeVisible();

            // Clean
            await umbracoApi.media.ensureNameNotExists(parentName);
            await umbracoApi.media.ensureNameNotExists(childName);
        });

        test('create File in a Folder', async ({page, umbracoApi, umbracoUi}) => {
            const parentName = 'ParentFileFolder';
            const childName = 'ChildFile';
            await umbracoApi.media.ensureNameNotExists(parentName);
            await umbracoApi.media.ensureNameNotExists(childName);
            const childPath = "./fixtures/mediaLibrary/File.txt"

            // Action
            await umbracoApi.media.createDefaultFolder(parentName);
            await umbracoUi.refreshMediaTree();
            await umbracoUi.clickElement(umbracoUi.getTreeItem('media', [parentName]), {
                button: "right",
                force: true
            });
            await page.locator('[data-element="action-create"]').click();
            await page.locator('[data-element="action-File"]').click();
            await page.locator('[data-element="editor-name-field"]').type(childName);
            await umbracoUi.fileUploader(childPath);

            // Assert
            await expect(page.locator('.alert-success')).toBeVisible();

            // Clean
            await umbracoApi.media.ensureNameNotExists(parentName);
            await umbracoApi.media.ensureNameNotExists(childName);
        });

        test('create Folder in a Folder', async ({page, umbracoApi, umbracoUi}) => {
            const parentName = 'ParentFolderFolder';
            const childName = 'ChildFolder';
            await umbracoApi.media.ensureNameNotExists(parentName);
            await umbracoApi.media.ensureNameNotExists(childName);
            
            // Action
            await umbracoApi.media.createDefaultFolder(parentName);
            await umbracoUi.refreshMediaTree();

            await umbracoUi.clickElement(umbracoUi.getTreeItem('media', [parentName]), {
                button: "right",
                force: true
            });
            await page.locator('[data-element="action-create"]').click();
            await page.locator('[data-element="action-Folder"]').click();
            await page.locator('[data-element="editor-name-field"]').type(childName);
            await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

            // Assert
            await expect(page.locator('.alert-success')).toBeVisible();

            // Clean
            await umbracoApi.media.ensureNameNotExists(parentName);
            await umbracoApi.media.ensureNameNotExists(childName);
        });

        test('create Image in a Folder', async ({page, umbracoApi, umbracoUi}) => {
            const parentName = 'ParentImageFolder';
            const childName = 'ChildImage';
            await umbracoApi.media.ensureNameNotExists(parentName);
            await umbracoApi.media.ensureNameNotExists(childName);
            const childPath = "./fixtures/mediaLibrary/Umbraco.png"

            // Action
            await umbracoApi.media.createDefaultFolder(parentName);
            await umbracoUi.refreshMediaTree();
            await umbracoUi.clickElement(umbracoUi.getTreeItem('media', [parentName]), {
                button: "right",
                force: true
            });
            await page.locator('[data-element="action-create"]').click();
            await page.locator('[data-element="action-Image"]').click();
            await page.locator('[data-element="editor-name-field"]').type(childName);
            await umbracoUi.fileUploader(childPath);

            // Assert
            await expect(page.locator('.alert-success')).toBeVisible();

            // Clean
            await umbracoApi.media.ensureNameNotExists(parentName);
            await umbracoApi.media.ensureNameNotExists(childName);
        });

        test('create VectorGraphics(SVG) in a Folder', async ({page, umbracoApi, umbracoUi}) => {
            const parentName = 'ParentVectorGraphicsFolder';
            const childName = 'ChildVectorGraphics';
            await umbracoApi.media.ensureNameNotExists(parentName);
            await umbracoApi.media.ensureNameNotExists(childName);
            const childPath = "./fixtures/mediaLibrary/VectorGraphics.svg"

            // Action
            await umbracoApi.media.createDefaultFolder(parentName);
            await umbracoUi.refreshMediaTree();
            await umbracoUi.clickElement(umbracoUi.getTreeItem('media', [parentName]), {
                button: "right",
                force: true
            });
            await page.locator('[data-element="action-create"]').click();
            await page.locator('[data-element="action-umbracoMediaVectorGraphics"]').click();
            await page.locator('[data-element="editor-name-field"]').type(childName);
            await umbracoUi.fileUploader(childPath);

            // Assert
            await expect(page.locator('.alert-success')).toBeVisible();

            // Clean
            await umbracoApi.media.ensureNameNotExists(parentName);
            await umbracoApi.media.ensureNameNotExists(childName);
        });

        test('create Video in a Folder', async ({page, umbracoApi, umbracoUi}) => {
            const parentName = 'ParentVideoFolder';
            const childName = 'ChildVideo';
            await umbracoApi.media.ensureNameNotExists(parentName);
            await umbracoApi.media.ensureNameNotExists(childName);
            const childPath = "./fixtures/mediaLibrary/Video.mp4"

            // Action
            await umbracoApi.media.createDefaultFolder(parentName);
            await umbracoUi.refreshMediaTree();
            await umbracoUi.clickElement(umbracoUi.getTreeItem('media', [parentName]), {
                button: "right",
                force: true
            });
            await page.locator('[data-element="action-create"]').click();
            await page.locator('[data-element="action-umbracoMediaVideo"]').click();
            await page.locator('[data-element="editor-name-field"]').type(childName);
            await umbracoUi.fileUploader(childPath);

            // Assert
            await expect(page.locator('.alert-success')).toBeVisible();

            // Clean
            await umbracoApi.media.ensureNameNotExists(parentName);
            await umbracoApi.media.ensureNameNotExists(childName);
        });
    });
    
    test('Delete one of each Files in media', async ({page, umbracoApi, umbracoUi}) => {
        const articleName = 'ArticleToDelete';
        const audioName = 'AudioToDelete';
        const fileName = 'FileToDelete';
        const folderName = 'FolderToDelete';
        const imageName = 'ImageToDelete';
        const vectorGraphicsName = 'VectorGraphicsToDelete';
        const videoName = 'VideoToDelete';
        await umbracoApi.media.deleteAllFiles(articleName, audioName, fileName, folderName, imageName, vectorGraphicsName, videoName);

        // Action
        await umbracoApi.media.createAllFileTypes(articleName, audioName, fileName, folderName, imageName, vectorGraphicsName, videoName);
        await page.reload();
        // Needs to close tours when page has reloaded
        await page.click('.umb-tour-step__close');
        
        // Takes all the child elements in folder-grid.
        await page.locator(".umb-folder-grid").locator("xpath=/*", {hasText: folderName}).click({
            position: {
                x: 5,
                y: 0
            }
        });
        const files = await page.locator('[data-element="media-grid"]').locator("xpath=/*");
        await umbracoUi.clickMultiple(files);
        await page.locator('[label-key="actions_delete"]').click();
        await page.locator('[alias="overlaySubmit"]').click();

        // Assert
        await expect(page.locator('.alert-success')).toBeVisible();
        await expect(page.locator(".umb-folder-grid")).toBeHidden();
        await expect(page.locator('[data-element="media-grid"]')).toBeHidden();

        // Clean
        await umbracoApi.media.clearRecycleBin();
    });

    test('Update existing File with new name', async ({page, umbracoApi, umbracoUi}) => {
        const fileItemNameOld = "File";
        const fileItemNameNew = "UpdatedFile";
        const fileName = "File.txt";
        const path = fileName;
        const mimeType = "*/*";
        await umbracoApi.media.ensureNameNotExists(fileItemNameOld);

        // Action
        await umbracoApi.media.createFileWithFile(fileItemNameOld, fileName, path, mimeType);
        await umbracoUi.refreshMediaTree();
        await page.locator('[data-element="tree-item-' + fileItemNameOld + '"]').click();
        await page.locator('[data-element="editor-name-field"]').fill(fileItemNameNew);
        await page.locator('[label-key="buttons_save"]').click();
        await umbracoUi.refreshMediaTree();

        // Assert
        await expect(page.locator(".umb-tree-item__inner", {hasText: fileItemNameNew})).toBeVisible();

        // Clean
        await umbracoApi.media.ensureNameNotExists(fileItemNameNew);
    });

    test('Update existing File with new File', async ({page, umbracoApi, umbracoUi}) => {
        const fileItemName = "File";
        const fileName = "File.txt";
        const path = fileName;
        const fileNameNew = "UpdatedFile.txt"
        const pathNew = "./fixtures/mediaLibrary/" + fileNameNew;
        const mimeType = "*/*";
        await umbracoApi.media.ensureNameNotExists(fileItemName);

        // Action
        await umbracoApi.media.createFileWithFile(fileItemName, fileName, path, mimeType);
        await umbracoUi.refreshMediaTree();
        await page.locator('[data-element="tree-item-' + fileItemName + '"]').click();
        await page.locator('[key="content_uploadClear"]').click();
        await umbracoUi.fileUploader(pathNew);
        await page.locator('[label-key="buttons_save"]').click();

        // Assert
        await expect(page.locator('.alert-success')).toBeVisible();

        // Clean
        await umbracoApi.media.ensureNameNotExists(fileItemName);
    });
});
