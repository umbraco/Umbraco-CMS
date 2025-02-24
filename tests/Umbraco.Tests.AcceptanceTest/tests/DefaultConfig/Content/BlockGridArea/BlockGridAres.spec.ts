import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

let documentTypeId = '';
let contentId = '';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';

const blockGridDataTypeName = 'BlockGridTester';
let blockGridDataTypeId = null;

const firstElementTypeName = 'FirstBlockGridElement';
let firstElementTypeId = null;
const firstElementGroupName = 'FirstGroup';
const textStringDataTypeName = 'Textstring';

const secondElementTypeName = 'SecondBlockGridElement';

let textStringDataType = null;


const firstAreaName = 'FirstArea';
const areaCreateLabel = 'CreateLabel';
const toAllowInAreas = true;


test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(firstElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondElementTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);

  textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(firstElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondElementTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
});

test('can create content with a block grid with an empty block in a area', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  firstElementTypeId = await umbracoApi.documentType.createEmptyElementType(firstElementTypeName);
  blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithAnAreaInABlockWithAllowInAreas(blockGridDataTypeName, firstElementTypeId, firstAreaName, toAllowInAreas, areaCreateLabel);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);


  // Act
  await umbracoUi.content.clickAddBlockGridElementWithName(firstElementTypeName);
  await umbracoUi.content.clickSelectBlockElementWithName(firstElementTypeName);

  await umbracoUi.content.clickLinkWithName(areaCreateLabel)
  await umbracoUi.content.clickSelectBlockElementInAreaWithName(firstElementTypeName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await page.pause()

  await umbracoUi.content.doesBlockContainBlockInAreaWithName(firstElementTypeName, firstAreaName, firstElementTypeName);



});

test('can create content with block grid area alias', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
});

test('can create content with block grid area with a create label', async ({page, umbracoApi, umbracoUi}) => {
});

test('can create content with block grid area with column span', async ({page, umbracoApi, umbracoUi}) => {
});

test('can create content with block grid area with row span', async ({page, umbracoApi, umbracoUi}) => {
});

test('can create content with block grid area with min allowed', async ({page, umbracoApi, umbracoUi}) => {
});

test('can create content with block grid area with max allowed', async ({page, umbracoApi, umbracoUi}) => {
});
