import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const documentTypeName = 'LockedDocumentType';
const documentTypeFolderName = 'LockedDocumentTypeFolder';

test('blocks every change to a document type in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
  const settingsSectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.documentType.isInlineEntityActionVisibleForName('Document Types', 'ReloadChildrenOf');
    await umbracoUi.documentType.isSidebarActionsMenuVisibleForName('Document Types', false);
    await umbracoUi.documentType.clickRootFolderCaretButton();
    await umbracoUi.documentType.isInlineEntityActionVisibleForName(documentTypeFolderName, 'ReloadChildrenOf');
    await umbracoUi.documentType.isSidebarActionsMenuVisibleForName(documentTypeFolderName, false);
    await umbracoUi.documentType.clickActionsMenuForDocumentType(documentTypeName);
    await umbracoUi.documentType.isEntityActionWithNameVisible('Export');
    await umbracoUi.documentType.isEntityActionWithNameVisible('Create', false);
    await umbracoUi.documentType.isEntityActionWithNameVisible('DuplicateTo', false);
    await umbracoUi.documentType.isEntityActionWithNameVisible('MoveTo', false);
    await umbracoUi.documentType.isEntityActionWithNameVisible('Delete', false);
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
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${settingsSectionUrl}/workspace/document-type/create/parent/document-type-root/null/null`);
    await umbracoUi.documentType.isEntityDetailForbiddenVisible();
  });
});

test('blocks every change to a document type through the Management API', async ({umbracoApi}) => {
  // Arrange
  const documentType = await umbracoApi.documentType.getByName(documentTypeName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.documentType}`;

  // Act
  const createResponse = await umbracoApi.post(url, {});
  const updateResponse = await umbracoApi.put(`${url}/${documentType.id}`, documentType);
  const deleteResponse = await umbracoApi.delete(`${url}/${documentType.id}`);

  // Assert
  expect(createResponse.status()).toBe(403);
  expect(createResponse.headers()['umb-schema-lockdown']).toBe('document-type:create');
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('document-type:update');
  expect(deleteResponse.status()).toBe(403);
  expect(deleteResponse.headers()['umb-schema-lockdown']).toBe('document-type:delete');
});
