import {expect} from '@playwright/test';
import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

test('can get content from delivery api', async ({umbracoApi}) => {
  // Arrange
  const documentTypeName = 'TestDocumentType';
  const contentName = 'TestContent';
  const dataTypeName = 'Textstring';
  const textStringValue = 'This is a test text string value';
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataType.id, 'TestGroup');
  const documentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, textStringValue, dataTypeName);
  const propertyValue = {
    dataTypeName: AliasHelper.toAlias(dataTypeName),
    dataTypeValue: textStringValue
  }

  // Act
  await umbracoApi.document.publish(documentId);

  // Assert
  expect(await umbracoApi.deliveryApi.doesContentItemWithIdContainValues(documentId, contentName, AliasHelper.toAlias(documentTypeName), [propertyValue])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can get media image from delivery api', async ({umbracoApi}) => {
  // Arrange
  const mediaName = 'TestMedia';
  const mediaTypeName = 'File';
  await umbracoApi.media.ensureNameNotExists(mediaName);
  const mediaId = await umbracoApi.media.createDefaultMediaFile(mediaName);
  const mediaUrl = await umbracoApi.media.getMediaUrlById(mediaId);

  // Assert
  expect(await umbracoApi.deliveryApi.doesMediaItemWithIdContainValues(mediaId, mediaName, mediaTypeName, mediaUrl)).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});
