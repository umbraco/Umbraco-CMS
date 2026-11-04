import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const contentName = 'TestContentActionGroups';
const documentTypeName = 'TestDocumentTypeForContentActionGroups';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can see the document actions in their action groups', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);

  // Assert
  await umbracoUi.content.doesOpenActionMenuHaveActionGroups([
    'Umb.EntityActionGroup.Create',
    'Umb.EntityActionGroup.Structure',
    'Umb.EntityActionGroup.Publishing',
    'Umb.EntityActionGroup.Routing',
    'Umb.EntityActionGroup.Notifications',
  ]);
  await umbracoUi.content.isEntityActionInActionGroup('Create', 'Create');
  await umbracoUi.content.isEntityActionInActionGroup('Trash', 'Structure');
  await umbracoUi.content.isEntityActionInActionGroup('MoveTo', 'Structure');
  await umbracoUi.content.isEntityActionInActionGroup('Publish', 'Publishing');
  await umbracoUi.content.isEntityActionInActionGroup('CultureAndHostnames', 'Routing');
  await umbracoUi.content.isEntityActionInActionGroup('Notifications', 'Notifications');
  await umbracoUi.content.isEntityActionInActionGroup('ReloadChildrenOf', 'Other');
});
