import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const blockListEditorName = 'TestBlockListEditor';
const blockListLocatorName = 'Block List';
const blockListEditorAlias = 'Umbraco.BlockList';
const blockListEditorUiAlias = 'Umb.PropertyEditorUi.BlockList';

const elementTypeName = 'BlockListElement';
const dataTypeName = 'Textstring';
const groupName = 'testGroup';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(blockListEditorName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(blockListEditorName);
});

test('can add a label to a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const labelText = 'ThisIsALabel';
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListEditorName, elementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.enterBlockLabelText(labelText);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  const block = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(await umbracoApi.dataType.doesBlockListBlockContainLabel(blockListEditorName, elementTypeId, labelText)).toBeTruthy();
});

test('can update a label for a block', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const labelText = 'ThisIsALabel';
  const newLabelText = 'ThisIsANewLabel';
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockListWithBlockWithEditorAppearance(blockListEditorName, elementTypeId, labelText);
  expect(await umbracoApi.dataType.doesBlockListBlockContainLabel(blockListEditorName, elementTypeId, labelText)).toBeTruthy();


  // Act
  await umbracoUi.dataType.goToDataType(blockListEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.enterBlockLabelText(newLabelText);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.isSuccessNotificationVisible();
  const block = await umbracoApi.dataType.getByName(blockListEditorName);
  expect(await umbracoApi.dataType.doesBlockListBlockContainLabel(blockListEditorName, elementTypeId, newLabelText)).toBeTruthy();
});

test('can remove a label from a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can update overlay size for a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can open content model in a block', async ({umbracoApi, umbracoUi}) => {

});

test('can remove a content model from a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can add a settings model to a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can remove a settings model from a block', async ({umbracoApi, umbracoUi}) => {

});

test('can add a background color to a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can update a background color for a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can delete a background color from a block', async ({page, umbracoApi, umbracoUi}) => {
});

test('can add a icon color to a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can update a icon color for a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can delete a icon color from a block', async ({page, umbracoApi, umbracoUi}) => {
});

test('can add a custom stylesheet to a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can update a custom stylesheet for a block', async ({page, umbracoApi, umbracoUi}) => {

});

test('can delete a custom stylesheet from a block', async ({page, umbracoApi, umbracoUi}) => {
});


test('can enable hide content editor in a block', async ({page, umbracoApi, umbracoUi}) => {
});

test('can disable hide content editor in a block', async ({page, umbracoApi, umbracoUi}) => {
});



