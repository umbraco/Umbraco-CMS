import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'Test RTE Tiptap Style Select';
const inputText = 'This is Tiptap test';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  const customDataTypeId = await umbracoApi.dataType.createTiptapDataTypeWithStyleSelect(customDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
})

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can apply page header format', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.typeIntoRTETipTapEditor(inputText);
  await umbracoUi.content.selectAllRTETipTapEditorText();

  // Act
  await umbracoUi.content.clickStyleSelectButton();
  await umbracoUi.content.hoverCascadingMenuItemWithName('Headers');
  await umbracoUi.content.clickCascadingMenuItemWithName('Page header');
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.markup).toEqual('<h2>' + inputText + '</h2><p></p>');
});

test('can apply section header format', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.typeIntoRTETipTapEditor(inputText);
  await umbracoUi.content.selectAllRTETipTapEditorText();

  // Act
  await umbracoUi.content.clickStyleSelectButton();
  await umbracoUi.content.hoverCascadingMenuItemWithName('Headers');
  await umbracoUi.content.clickCascadingMenuItemWithName('Section header');
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.markup).toEqual('<h3>' + inputText + '</h3><p></p>');
});

test('can apply paragraph header format', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.typeIntoRTETipTapEditor(inputText);
  await umbracoUi.content.selectAllRTETipTapEditorText();

  // Act
  await umbracoUi.content.clickStyleSelectButton();
  await umbracoUi.content.hoverCascadingMenuItemWithName('Headers');
  await umbracoUi.content.clickCascadingMenuItemWithName('Paragraph header');
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.markup).toEqual('<h4>' + inputText + '</h4><p></p>');
});

test('can apply paragraph blocks format', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.typeIntoRTETipTapEditor(inputText);
  await umbracoUi.content.selectAllRTETipTapEditorText();

  // Act
  await umbracoUi.content.clickStyleSelectButton();
  await umbracoUi.content.hoverCascadingMenuItemWithName('Blocks');
  await umbracoUi.content.clickCascadingMenuItemWithName('Paragraph');
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.markup).toEqual('<p>' + inputText + '</p>');
});

test('can apply block quote format', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.typeIntoRTETipTapEditor(inputText);
  await umbracoUi.content.selectAllRTETipTapEditorText();

  // Act
  await umbracoUi.content.clickStyleSelectButton();
  await umbracoUi.content.hoverCascadingMenuItemWithName('Containers');
  await umbracoUi.content.clickCascadingMenuItemWithName('Block quote');
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.markup).toEqual('<blockquote><p>' + inputText + '</p></blockquote><p></p>');
});

test('can apply code block format', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.typeIntoRTETipTapEditor(inputText);
  await umbracoUi.content.selectAllRTETipTapEditorText();

  // Act
  await umbracoUi.content.clickStyleSelectButton();
  await umbracoUi.content.hoverCascadingMenuItemWithName('Containers');
  await umbracoUi.content.clickCascadingMenuItemWithName('Code block');
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.markup).toEqual('<pre><code>' + inputText + '</pre></code><p></p>');
});