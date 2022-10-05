import {expect, request} from "@playwright/test";
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.describe('Media', () => {

    test.beforeEach(async ({page, umbracoApi, umbracoUi}) => {
        // TODO: REMOVE THIS WHEN SQLITE IS FIXED
        // Wait so we don't bombard the API
        await page.waitForTimeout(1000);
        await umbracoApi.login();
        await umbracoUi.goToSection(ConstantHelper.sections.media);
        await umbracoApi.media.deleteAllMedia()
    });
    
    test('move one of each Files into a Folder', async ({page, umbracoApi, umbracoUi}) => {
        const articleName = 'ArticleToMove';
        const audioName = 'AudioToMove';
        const fileName = 'FileToMove';
        const folderName = 'FolderToMove';
        const imageName = 'ImageToMove';
        const vectorGraphicsName = 'VectorGraphicsToMove';
        const videoName = 'VideoToMove';
        const folderToMoveTooName = 'MoveHere';
        
        const mediaFileTypes = [
            {fileTypeNames: articleName},
            {fileTypeNames: audioName},
            {fileTypeNames: fileName},
            {fileTypeNames: imageName},
            {fileTypeNames: vectorGraphicsName},
            {fileTypeNames: videoName}
        ]
        
        await umbracoApi.media.deleteAllFiles(articleName,audioName,fileName,folderName,imageName,vectorGraphicsName,videoName);
        await umbracoApi.media.ensureNameNotExists(folderToMoveTooName);
        
        // Action
        await umbracoApi.media.createAllFileTypes(articleName, audioName, fileName, folderName, imageName, vectorGraphicsName, videoName);
        await umbracoApi.media.createDefaultFolder(folderToMoveTooName);
        await page.reload();
        // Needs to close tours when page has reloaded
        await page.click('.umb-tour-step__close');
        const files = await page.locator('[data-element="media-grid"]').locator("xpath=/*");
        await umbracoUi.clickMultiple(files);
        // I set an position on the click event so it does not click on the folder name and open it.
        await page.locator(".umb-folder-grid").locator("xpath=/*", {hasText: folderName}).click({
            position: {
                x: 5,
                y: 0
            }
        });
        await page.locator('[label-key="actions_move"]').click();
        await page.locator('[data-element="editor-container"] >> "' + folderToMoveTooName + '"').click();
        await page.locator('[label-key="general_submit"]').click();
  
        // Assert
        await page.waitForTimeout(500);
        await umbracoUi.refreshMediaTree();
        await page.locator('[data-element="tree-item-' + folderToMoveTooName + '"]').click();
        for (const names of mediaFileTypes) {
            await expect(page.locator('[data-element="media-grid"]', {hasText: names.fileTypeNames})).toBeVisible();
        }
        await expect(page.locator(".umb-folder-grid", {hasText: folderName})).toBeVisible();

        // Clean
        await umbracoApi.media.deleteAllFiles(articleName, audioName, fileName, folderName, imageName, vectorGraphicsName, videoName);
        await umbracoApi.media.ensureNameNotExists(folderToMoveTooName);
    });
    
    test('sort by Name', async ({page, umbracoApi, umbracoUi}) => {
        const FolderNameA = 'A';
        const FolderNameB = 'B';
        const FolderNameC = 'C';
        await umbracoApi.media.ensureNameNotExists(FolderNameA);
        await umbracoApi.media.ensureNameNotExists(FolderNameB);
        await umbracoApi.media.ensureNameNotExists(FolderNameC);

        // Action
        await umbracoApi.media.createDefaultFolder(FolderNameC);
        await umbracoApi.media.createDefaultFolder(FolderNameB);
        await umbracoApi.media.createDefaultFolder(FolderNameA);

        await umbracoUi.refreshMediaTree();
        await page.locator('[element="tree-item-options"]', {hasText: "Media"}).click({button: "right", force: true});
        await page.locator('[data-element="action-sort"]').click();
        await page.locator('.table-sortable >> [key="general_name"]').click();
        await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

        // Assert
        const item = await page.locator('[ui-sortable="vm.sortableOptions"]').locator("xpath=/*[1]")
        await expect(item).toContainText(FolderNameA);

        // Clean
        await umbracoApi.media.ensureNameNotExists(FolderNameA);
        await umbracoApi.media.ensureNameNotExists(FolderNameB);
        await umbracoApi.media.ensureNameNotExists(FolderNameC);
    });

    test('search after a specific Folder', async ({page, umbracoApi, umbracoUi}) => {
        const FolderSearchName = 'SearchMe';
        await umbracoApi.media.ensureNameNotExists(FolderSearchName);

        // Action
        await umbracoApi.media.createDefaultFolder(FolderSearchName)
        await page.locator('[model="options.filter"]').click();
        await page.locator('[placeholder="Type to search..."]').type(FolderSearchName);

        // Assert
        await expect(page.locator(".umb-folder-grid__folder-description", {hasText: FolderSearchName})).toBeVisible();

        // Clean
        await umbracoApi.media.ensureNameNotExists(FolderSearchName);
    });

    test('change Grid to List', async ({page, umbracoApi, umbracoUi}) => {
        const FolderOneName = 'FolderOne';
        const FolderTwoName = 'FolderTwo';
        await umbracoApi.media.ensureNameNotExists(FolderOneName);
        await umbracoApi.media.ensureNameNotExists(FolderTwoName);

        // Action
        await umbracoApi.media.createDefaultFolder(FolderOneName);
        await umbracoApi.media.createDefaultFolder(FolderTwoName);
        await umbracoUi.refreshMediaTree();
        await page.locator('[ng-click="vm.toggleLayoutDropdown()"]').click({force: true});
        await page.locator('[title="List"]').click();

        // Assert
        await expect(page.locator('[icon="icon-list"]')).toBeVisible();

        // Clean
        await umbracoApi.media.ensureNameNotExists(FolderOneName);
        await umbracoApi.media.ensureNameNotExists(FolderTwoName);
    });

    test('change List to Grid', async ({page, umbracoApi, umbracoUi}) => {
        const FolderOneName = 'FolderOne';
        const FolderTwoName = 'FolderTwo';
        await umbracoApi.media.ensureNameNotExists(FolderOneName);
        await umbracoApi.media.ensureNameNotExists(FolderTwoName);

        // Action
        await umbracoApi.media.createDefaultFolder(FolderOneName);
        await umbracoApi.media.createDefaultFolder(FolderTwoName);
        await umbracoUi.refreshMediaTree();
        await page.locator('[ng-click="vm.toggleLayoutDropdown()"]').click({force: true});
        await page.locator('[title="List"]').click();
        await umbracoUi.refreshMediaTree();
        await page.locator('[ng-click="vm.toggleLayoutDropdown()"]').click({force: true});
        await page.locator('[title="Grid"]').click();

        // Assert
        await expect(page.locator('[icon="icon-thumbnails-small"]')).toBeVisible();

        // Clean
        await umbracoApi.media.ensureNameNotExists(FolderOneName);
        await umbracoApi.media.ensureNameNotExists(FolderTwoName);
    });
});