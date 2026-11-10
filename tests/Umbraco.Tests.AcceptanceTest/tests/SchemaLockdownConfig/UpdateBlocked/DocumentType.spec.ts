import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const documentTypeName = 'LockedDocumentType';
const documentTypeFolderName = 'LockedDocumentTypeFolder';

test('blocks updating a document type in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  await test.step('Tree', async () => {
    await umbracoUi.documentType.clickActionsMenuAtRoot();
    await umbracoUi.documentType.isEntityActionWithNameVisible('Create');
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.documentType.clickRootFolderCaretButton();
    await umbracoUi.documentType.clickActionsMenuForDocumentType(documentTypeFolderName);
    await umbracoUi.documentType.isEntityActionWithNameVisible('Delete');
    await umbracoUi.documentType.isEntityActionWithNameVisible('Rename', false);
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.documentType.clickActionsMenuForDocumentType(documentTypeName);
    await umbracoUi.documentType.isEntityActionWithNameVisible('Delete');
    await umbracoUi.documentType.isEntityActionWithNameVisible('MoveTo', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.documentType.isSaveButtonVisible(false);
    await umbracoUi.documentType.isWorkspaceNameEditable(false);
    await umbracoUi.documentType.isPropertyTypeEditable('LockedProperty', false);
    await umbracoUi.documentType.isAddPropertyButtonVisible(false);
    await umbracoUi.schemaLockdown.clickLockedWorkspaceActionButton();
    await umbracoUi.documentType.doesModalHaveText('Changes to this item are locked in this environment.');
  });
});

test('blocks updating a document type through the Management API', async ({umbracoApi}) => {
  // Arrange
  const documentType = await umbracoApi.documentType.getByName(documentTypeName);

  // Act
  const response = await umbracoApi.put(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.documentType}/${documentType.id}`, documentType);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('document-type:update');
});
