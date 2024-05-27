import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Remove smoke tag before merging
test.describe('Content with property editors tests', {tag: '@smoke'}, () => {
  const contentName = 'TestContent';
  const documentTypeName = 'TestDocumentTypeForContent';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.document.ensureNameNotExists(contentName); 
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName); 
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName); 
  });

  test('can create a content with Rich Text Editor', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeName = 'Richtext editor';
    const contentText = 'This is Rich Text Editor content!';
    const expectedContentValue = {
      blocks: {
        contentData: [],
        layout: {},
        propertyEditorAlias: 'Umbraco.TinyMCE',
        settingsData: [],
      },
      markup: '<p>' + contentText + '</p>',
    };
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateButton();
    await umbracoUi.content.clickLabelWithName(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.enterRichTextArea(contentText);
    await umbracoUi.content.clickSaveAndPublishButton();

    // Assert
    await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value).toEqual(expectedContentValue);
  });

  test('can create a content with text area', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeName = 'Textarea';
    const contentText = 'This is Textarea content!';
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateButton();
    await umbracoUi.content.clickLabelWithName(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.enterTextArea(contentText);
    await umbracoUi.content.clickSaveAndPublishButton();

    // Assert
    await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value).toEqual(contentText);
  });

  // TODO: Remove skip when the front-end is ready. Currently it returns error when publishing a content
  test.skip('can create a content with upload file', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeName = 'Upload File';
    const uploadFilePath = 'Umbraco.png';
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateButton();
    await umbracoUi.content.clickLabelWithName(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.changeFileTypeWithFileChooser('./fixtures/mediaLibrary/' + uploadFilePath);
    await umbracoUi.content.clickSaveAndPublishButton();

    // Assert
    await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value.src).toContainEqual(uploadFilePath);
  });

  test('can create a content with tags', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeName = 'Tags';
    const tagName = 'test';
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateButton();
    await umbracoUi.content.clickLabelWithName(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.addTags(tagName);
    await umbracoUi.content.clickSaveAndPublishButton();

    // Assert
    await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value).toEqual([tagName]);
  });

  test('can create a content with content picker', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeName = 'Content Picker';
    const contentPickerName = 'TestContentPicker';
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    const contentPickerId = await umbracoApi.document.createDefaultDocument(contentPickerName, documentTypeId);

    // Act
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateButton();
    await umbracoUi.content.clickLabelWithName(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.addContentPicker(contentPickerName);
    await umbracoUi.content.clickSaveAndPublishButton();

    // Assert
    await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value).toEqual(contentPickerId);

    // Clean
    await umbracoApi.document.delete(contentPickerId);
  });

  // TODO: Remove skip and update the test when the front-end is ready. Currently the list of content is not displayed.
  test.skip('can create a content with list view - content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeName = 'List View - Content';
    const contentListViewName = 'TestListViewContent';
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    const contentPickerId = await umbracoApi.document.createDefaultDocument(contentListViewName, documentTypeId);

    // Act
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateButton();
    await umbracoUi.content.clickLabelWithName(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    // TODO: add step to interact with the list
    await umbracoUi.content.clickSaveAndPublishButton();

    // Assert
    await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();

    // Clean
    await umbracoApi.document.delete(contentPickerId);
  });
});
