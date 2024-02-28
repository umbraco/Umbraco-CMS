import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Documents tests', () => {
  let documentTypeId = '';
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
    await expect(umbracoApi.document.doesNameExist(documentName)).toBeTruthy();
  });


});
