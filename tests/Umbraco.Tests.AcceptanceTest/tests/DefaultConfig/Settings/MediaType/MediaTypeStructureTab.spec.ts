import {ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

const mediaTypeName = 'TestMediaType';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
});

test('can create a media type with allow at root enabled', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.clickStructureTab();
  await umbracoUi.mediaType.clickAllowAtRootButton();
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  await umbracoUi.mediaType.isSuccessNotificationVisible();
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.allowedAsRoot).toBeTruthy();
});

test('can create a media type with an allowed child node type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.clickStructureTab();
  await umbracoUi.mediaType.clickChooseButton();
  await umbracoUi.mediaType.clickButtonWithName(mediaTypeName);
  await umbracoUi.mediaType.clickAllowedChildNodesButton();
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  await umbracoUi.mediaType.isSuccessNotificationVisible();
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.allowedMediaTypes[0].mediaType.id).toBe(mediaTypeData.id);
});

test('can create a media type with multiple allowed child nodes types', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaTypeId = await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);
  const secondMediaTypeName = 'SecondMediaType';
  await umbracoApi.mediaType.ensureNameNotExists(secondMediaTypeName);
  const secondMediaTypeId = await umbracoApi.mediaType.createDefaultMediaType(secondMediaTypeName);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.clickStructureTab();
  await umbracoUi.mediaType.clickChooseButton();
  await umbracoUi.mediaType.clickButtonWithName(mediaTypeName);
  await umbracoUi.mediaType.clickButtonWithName(secondMediaTypeName);
  await umbracoUi.mediaType.clickAllowedChildNodesButton();
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  await umbracoUi.mediaType.isSuccessNotificationVisible();
  expect(await umbracoApi.mediaType.doesMediaTypeContainAllowedChildNodeIds(mediaTypeName, [mediaTypeId, secondMediaTypeId])).toBeTruthy();

  // Clean
  await umbracoApi.mediaType.ensureNameNotExists(secondMediaTypeName);
});

test('can delete an allowed child note from a media type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childNodeName = 'MediaChildNode';
  await umbracoApi.mediaType.ensureNameNotExists(childNodeName);
  const childNodeId = await umbracoApi.mediaType.createDefaultMediaType(childNodeName);
  await umbracoApi.mediaType.createMediaTypeWithAllowedChildNode(mediaTypeName, childNodeId);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.clickStructureTab();
  await umbracoUi.mediaType.clickRemoveButtonForName(childNodeName);
  await umbracoUi.mediaType.clickConfirmRemoveButton();
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  await umbracoUi.mediaType.isSuccessNotificationVisible();
  const mediaTypeData = await umbracoApi.mediaType.getByName(childNodeName);
  expect(mediaTypeData.allowedMediaTypes.length).toBe(0);

  // Clean
  await umbracoApi.mediaType.ensureNameNotExists(childNodeName);
});

test('can configure a collection for a media type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const collectionDataTypeName = 'TestCollection';
  await umbracoApi.dataType.ensureNameNotExists(collectionDataTypeName);
  const collectionDataTypeId = await umbracoApi.dataType.create(collectionDataTypeName, 'Umbraco.ListView', 'Umb.PropertyEditorUi.Collection', []);
  await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.clickStructureTab();
  await umbracoUi.mediaType.clickAddCollectionButton();
  await umbracoUi.mediaType.clickTextButtonWithName(collectionDataTypeName);
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  await umbracoUi.mediaType.isSuccessNotificationVisible();
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.collection.id).toEqual(collectionDataTypeId);

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(collectionDataTypeName);
});
