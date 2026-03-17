import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// Contents
const parentDocumentName = 'ParentDocument';
const childDocumentName = 'ChildDocument';
let parentDocumentId = '';
let childDocumentId = '';
// Document Types
const documentTypeName = 'DocumentTypeForPublicAccess';
const childDocumentTypeName = 'ChildDocumentTypeForPublicAccess';
// Member Groups
const memberGroupName = 'TestMemberGroupForPublicAccess';
const secondMemberGroupName = 'SecondMemberGroupForPublicAccess';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.memberGroup.createDefaultMemberGroup(memberGroupName);
  await umbracoApi.memberGroup.createDefaultMemberGroup(secondMemberGroupName);
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(childDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  parentDocumentId = await umbracoApi.document.createDefaultDocument(parentDocumentName, documentTypeId);
  await umbracoApi.document.publish(parentDocumentId);
  childDocumentId = await umbracoApi.document.createDefaultDocumentWithParent(childDocumentName, childDocumentTypeId, parentDocumentId);
  await umbracoApi.document.publish(childDocumentId);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(childDocumentName);
  await umbracoApi.document.ensureNameNotExists(parentDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.memberGroup.ensureNameNotExists(memberGroupName);
  await umbracoApi.memberGroup.ensureNameNotExists(secondMemberGroupName);
});

test('can setup public access protection on a content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(parentDocumentName);
  await umbracoUi.content.clickPublicAccessActionMenuOption();
  await umbracoUi.content.addGroupBasedPublicAccess(memberGroupName, parentDocumentName);

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.publicAccessSettingCreated);
});

test('can setup public access protection on the child content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.openContentCaretButtonForName(parentDocumentName);
  await umbracoUi.content.clickActionsMenuForContent(childDocumentName);
  await umbracoUi.content.clickPublicAccessActionMenuOption();
  await umbracoUi.content.addGroupBasedPublicAccess(memberGroupName, parentDocumentName);

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.publicAccessSettingCreated);
});

test('can update public access protection on the child content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.setupPublicAccessForDocument(childDocumentId, [memberGroupName], parentDocumentId, parentDocumentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.openContentCaretButtonForName(parentDocumentName);
  await umbracoUi.content.clickActionsMenuForContent(childDocumentName);
  await umbracoUi.content.clickPublicAccessActionMenuOption();
  await umbracoUi.content.updateGroupBasedPublicAccess(secondMemberGroupName);

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.publicAccessSettingUpdated);
});

test('can delete public access protection from a content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.setupPublicAccessForDocument(parentDocumentId, [memberGroupName], parentDocumentId, parentDocumentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(parentDocumentName);
  await umbracoUi.content.clickPublicAccessActionMenuOption();
  await umbracoUi.content.clickRemoveProtectionButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.publicAccessSettingDeleted);
});

test('can delete public access protection from a child content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.setupPublicAccessForDocument(childDocumentId, [memberGroupName], parentDocumentId, parentDocumentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.openContentCaretButtonForName(parentDocumentName);
  await umbracoUi.content.clickActionsMenuForContent(childDocumentName);
  await umbracoUi.content.clickPublicAccessActionMenuOption();
  await umbracoUi.content.clickRemoveProtectionButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.publicAccessSettingDeleted);
});

test('can display inherited public access protection on child content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.setupPublicAccessForDocument(parentDocumentId, [memberGroupName], parentDocumentId, parentDocumentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.openContentCaretButtonForName(parentDocumentName);
  await umbracoUi.content.clickActionsMenuForContent(childDocumentName);
  await umbracoUi.content.clickPublicAccessActionMenuOption();

  // Assert
  // The setup wizard should appear (not an error) with the inherited protection pre-filled
  await umbracoUi.content.isPublicAccessErrorVisible(false);
  await umbracoUi.content.isPublicAccessHeadlineVisible('Setup Public Access');
  await umbracoUi.content.isPublicAccessSetupWizardVisible();
  await umbracoUi.content.isPublicAccessGroupBasedProtectionVisible();
});
