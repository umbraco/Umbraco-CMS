import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Content tests', () => {
  const contentName = 'TestContent';
  const documentTypeName = 'TestDocumentTypeForContent';
  let documentTypeId = '';

  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  });

  test('can create a content', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.content.clickActionMenuForContent();
    await umbracoUi.content.clickCreateButton();
    await umbracoUi.content.clickLabelWithName(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.dictionary.isSuccessNotificationVisible();

  });

  
});
