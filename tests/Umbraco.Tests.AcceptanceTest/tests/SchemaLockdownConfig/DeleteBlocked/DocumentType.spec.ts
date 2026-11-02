import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const documentTypeName = 'LockedDocumentType';
const documentTypeFolderName = 'LockedDocumentTypeFolder';

test('blocks deleting a document type in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  await test.step('Tree', async () => {
    await umbracoUi.documentType.clickActionsMenuAtRoot();
    await umbracoUi.documentType.isEntityActionWithNameVisible('Create');
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.documentType.clickRootFolderCaretButton();
    await umbracoUi.documentType.clickActionsMenuForDocumentType(documentTypeFolderName);
    await umbracoUi.documentType.isEntityActionWithNameVisible('Rename');
    await umbracoUi.documentType.isEntityActionWithNameVisible('Delete', false);
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.documentType.clickActionsMenuForDocumentType(documentTypeName);
    await umbracoUi.documentType.isEntityActionWithNameVisible('MoveTo');
    await umbracoUi.documentType.isEntityActionWithNameVisible('Delete', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.isSaveButtonVisible();
    await umbracoUi.documentType.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.documentType.isWorkspaceNameEditable();
    await umbracoUi.documentType.isPropertyTypeEditable('LockedProperty');
  });
});

test('blocks deleting a document type through the Management API', async ({umbracoApi}) => {
  // Arrange
  const documentType = await umbracoApi.documentType.getByName(documentTypeName);

  // Act
  const response = await umbracoApi.delete(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.documentType}/${documentType.id}`);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('document-type:delete');
});
