import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Documents tests', () => {
  let documentTypeId = '';
  let documentId = '';
  const documentName = 'TestDocument';
  const documentTypeName = 'TestDocumentType';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(documentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  });

  test('can create a document', async ({umbracoApi}) => {
    // Act
    await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);

    // Assert
    expect(await umbracoApi.document.doesNameExist(documentName)).toBeTruthy();
  });

  test('can delete a document', async ({umbracoApi}) => {
    // Arrange
    documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
    expect(umbracoApi.document.doesExist(documentId)).toBeTruthy();

    // Act
    await umbracoApi.document.delete(documentId);

    // Assert
    expect(await umbracoApi.document.doesNameExist(documentName)).toBeFalsy();
  });

  test('can update a document', async ({umbracoApi}) => {
    // Arrange
    const wrongName = 'WrongDocument';
    documentId = await umbracoApi.document.createDefaultDocument(wrongName, documentTypeId);
    expect(await umbracoApi.document.doesNameExist(wrongName)).toBeTruthy();
    let documentData = await umbracoApi.document.get(documentId);
    documentData.variants[0].name = documentName;

    // Act
    await umbracoApi.document.update(documentData.id, documentData);

    // Assert
    const updatedDocumentData = await umbracoApi.document.get(documentId);
    expect(updatedDocumentData.variants[0].name).toEqual(documentName);
  });
});
