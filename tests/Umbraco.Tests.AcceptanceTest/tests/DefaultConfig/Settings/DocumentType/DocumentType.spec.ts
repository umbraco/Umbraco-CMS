import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

test.describe('Document Type tests', () => {
  const documentTypeName = 'TestDocumentType';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoUi.goToBackOffice();


  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  });

  // DESIGN

  test('can create a empty document type', async ({page, umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.clickActionsMenuForName('Document Types');
    await umbracoUi.documentType.clickCreateThreeDotsButton();
    await umbracoUi.documentType.clickNewDocumentTypeButton();
    await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);

    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  });

  test('can rename a empty document type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongName = 'NotADocumentTypeName';
    await umbracoApi.documentType.ensureNameNotExists(wrongName);
    await umbracoApi.documentType.createDefaultDocumentType(wrongName);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(wrongName);
    await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  });

  test('can add a property to a empty document type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeName = 'Approved Color';
    await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.clickAddGroupButton();
    await umbracoUi.documentType.addPropertyEditor(dataTypeName);
    await umbracoUi.documentType.enterGroupName('TestGroup');
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    const dataType = await umbracoApi.dataType.getByName(dataTypeName);
    // Checks if the correct property was added to the document type
    expect(documentTypeData.properties[0].dataType.id).toBe(dataType.id);
  });

  test('can update a property in a document type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeName = 'Approved Color';
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);

    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName,dataTypeName, dataTypeData.id);

    await page.pause();

    await page.getByLabel('Editor settings').click();
    await page.getByLabel('Change').click();

    await page.getByLabel('Type to filter icons').fill('Media');
    await page.locator('li:nth-child(3) > uui-button > .item-content > .icon > svg').click();
    await page.getByRole('textbox', { name: 'Enter a name...' }).fill('Media Picker');
    await page.getByLabel('Update').click();
    await page.getByLabel('Save').click();



  });

  // TODO:   test('can update group name to a document type', async ({page, umbracoApi, umbracoUi}) => { });
  // TODO:   test('can update a property to a document type', async ({page, umbracoApi, umbracoUi}) => { });

  // TODO:   test('can create a document type with a property in a group', async ({page, umbracoApi, umbracoUi}) => { });
  // TODO:   test('can create a document type with a property in a tab', async ({page, umbracoApi, umbracoUi}) => { });
  // TODO:   test('can create a document type with properties in a group and a tab', async ({page, umbracoApi, umbracoUi}) => { });
  // TODO:   test('can create a document type with multiple groups', async ({page, umbracoApi, umbracoUi}) => { });
  // TODO:   test('can create a document type with multiple tabs', async ({page, umbracoApi, umbracoUi}) => { });
  // TODO:   test('can create a document type with a composition', async ({page, umbracoApi, umbracoUi}) => { });
  // TODO:   test('can reorder a group in a document type', async ({page, umbracoApi, umbracoUi}) => { });
  // TODO:   test('can reorder a property in a document type', async ({page, umbracoApi, umbracoUi}) => { });

  // TODO:   test('can create a document type ', async ({page, umbracoApi, umbracoUi}) => { });
  // TODO:   test('can create a document type', async ({page, umbracoApi, umbracoUi}) => { });
  // TODO:   test('can create a document type', async ({page, umbracoApi, umbracoUi}) => { });
  // TODO:   test('can create a document type', async ({page, umbracoApi, umbracoUi}) => { });

});
