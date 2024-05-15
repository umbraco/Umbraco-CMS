import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Remove smoke tag before merging
test.describe('Content tests @smoke', () => {
  let documentTypeId = '';
  let contentId = '';
  const contentName = 'TestContent';
  const documentTypeName = 'TestDocumentTypeForContent';
  const dataTypeName = 'Textstring';
  const contentText = 'This is test content text';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName); 
  });

  test('can create an empty content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedState = 'Draft';
    await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);

    // Act
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateButton();
    await umbracoUi.content.clickLabelWithName(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.isSuccessNotificationVisible();
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.variants[0].state).toBe(expectedState);
  });

  test('can save and publish an empty content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedState = 'Published';
    await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);

    // Act
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateButton();
    await umbracoUi.content.clickLabelWithName(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.clickSaveAndPublishButton();

    // Assert
    await umbracoUi.content.isSuccessNotificationVisible();
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.variants[0].state).toBe(expectedState);
  });

  test('can create a content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateButton();
    await umbracoUi.content.clickLabelWithName(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.enterTextstring(contentText);
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.isSuccessNotificationVisible();
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value).toBe(contentText);
  });

  test('can rename an empty content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongContentName = 'Wrong Content Name'
    documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
    contentId = await umbracoApi.document.createDefaultDocument(wrongContentName, documentTypeId);
    expect(await umbracoApi.document.doesNameExist(wrongContentName)).toBeTruthy();

    // Act
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.openContent(wrongContentName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.isSuccessNotificationVisible();
    const updatedContentData = await umbracoApi.document.get(contentId);
    expect(updatedContentData.variants[0].name).toEqual(contentName);
  });

  test('can update content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongContentText = 'This is wrong test content text';
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, wrongContentText, dataTypeName);

    // Act
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.openContent(contentName);
    await umbracoUi.content.enterTextstring(contentText);
    await umbracoUi.content.clickSaveButton();

        // Assert
    await umbracoUi.content.isSuccessNotificationVisible();
    const updatedContentData = await umbracoApi.document.get(contentId);
    expect(updatedContentData.variants[0].name).toEqual(contentName);
    expect(updatedContentData.values[0].value).toBe(contentText);
  });
});
