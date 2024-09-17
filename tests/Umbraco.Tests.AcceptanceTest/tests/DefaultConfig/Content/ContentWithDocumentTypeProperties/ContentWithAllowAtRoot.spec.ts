import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const documentTypeName = 'TestDocumentTypeForContent';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('cannot create any content if allow at root is not enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const noAllowedDocumentTypeAvailableMessage = 'There are no allowed Document Types available for creating content here';
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();

  // Assert
  await umbracoUi.content.isDocumentTypeNameVisible(documentTypeName, false);
  await umbracoUi.content.doesModalHaveText(noAllowedDocumentTypeAvailableMessage);
});