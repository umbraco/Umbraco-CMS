import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {ContentBuilder, DocumentTypeBuilder} from "@umbraco/json-models-builders";

test.describe('Recycle bin', () => {

  test.beforeEach(async ({page, umbracoApi}) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });

  test('Can delete content from recycle bin', async ({page, umbracoApi, umbracoUi}) => {
    const contentToDeleteName = "DeleteMe";
    const contentToNotDeleteName = "DontDelete";
    const testType = "TestType";

    await umbracoApi.documentTypes.ensureNameNotExists(testType);

    const docType = new DocumentTypeBuilder()
      .withName(testType)
      .build();

    await umbracoApi.documentTypes.save(docType).then(async (savedDocType) => {
      const contentToDelete = new ContentBuilder()
        .withContentTypeAlias(savedDocType.alias)
        .withAction("saveNew")
        .addVariant()
        .withName(contentToDeleteName)
        .withSave(true)
        .done()
        .build();

      const contentToNotDelete = new ContentBuilder()
        .withContentTypeAlias(savedDocType.alias)
        .withAction("saveNew")
        .addVariant()
        .withName(contentToNotDeleteName)
        .withSave(true)
        .done()
        .build();

      // Put it in the recycle bin
      await umbracoApi.content.save(contentToDelete).then(async savedToDelete => {
        await umbracoApi.content.deleteById(savedToDelete.id);
      });
      await umbracoApi.content.save(contentToNotDelete).then(async savedNotToDelete => {
        await umbracoApi.content.deleteById(savedNotToDelete.id)
      });
    });

    await umbracoUi.refreshContentTree();
    await umbracoUi.clickElement(umbracoUi.getTreeItem('content', ["Recycle Bin"]));
    await page.locator('.umb-content-grid__content', {hasText: contentToDeleteName}).click();
    // cy.get('.umb-content-grid__content').contains(contentToDeleteName).closest('div').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey("actions_delete"));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('contentTypeEditor_yesDelete'));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();

    await expect(page.locator('.umb-content-grid__content', {hasText: contentToDeleteName})).not.toBeVisible();
    await expect(await umbracoUi.getTreeItem('content', ["Recycle Bin", contentToDeleteName])).not.toBeVisible();
    await expect(page.locator('.umb-content-grid__content', {hasText: contentToNotDeleteName})).toBeVisible();

    // Clean up
    await umbracoApi.documentTypes.ensureNameNotExists(testType);
  });
});
