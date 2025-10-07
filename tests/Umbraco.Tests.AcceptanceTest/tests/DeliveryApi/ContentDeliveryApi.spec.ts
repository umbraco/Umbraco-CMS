import {expect} from '@playwright/test';
import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

// Fixed it later in other PR
test.fixme('can get content from delivery api', async ({umbracoApi}) => {
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
  expect(await umbracoApi.contentDeliveryApi.doesContentItemWithIdContainValues(documentId, contentName, AliasHelper.toAlias(documentTypeName), [propertyValue])).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});
