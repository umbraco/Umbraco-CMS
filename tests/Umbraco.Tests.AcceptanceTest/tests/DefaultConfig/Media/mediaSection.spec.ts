import {expect, request} from "@playwright/test";
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.describe('Media', () => {

    test.beforeEach(async ({page, umbracoApi, umbracoUi}, testInfo) => {
        await umbracoApi.report.report(testInfo);
        await umbracoApi.login();
        await umbracoUi.goToSection(ConstantHelper.sections.media);
        await umbracoApi.media.deleteAllMedia()
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
});