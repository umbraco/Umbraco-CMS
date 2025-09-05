import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'CustomMultiNodeTreePicker';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can create content with content picker with allowed types', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const allowedContentPickerDocumentTypeName = 'ContentPickerDocumentType';
  const allowedContentPickerName = 'Test Content Picker';
  const notAllowedContentPickerName = 'Not Allowed Test Content Picker';
  const allowedContentPickerDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(allowedContentPickerDocumentTypeName);
  const allowedContentPickerId = await umbracoApi.document.createDefaultDocument(allowedContentPickerName, allowedContentPickerDocumentTypeId);
  // Create a custom content picker with predefined allowed types
  const customDataTypeId = await umbracoApi.dataType.createMultiNodeTreePickerDataTypeWithAllowedTypes(customDataTypeName, allowedContentPickerDocumentTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(notAllowedContentPickerName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.isModalMenuItemWithNameDisabled(notAllowedContentPickerName);
  await umbracoUi.content.selectLinkByName(allowedContentPickerName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0]['unique']).toEqual(allowedContentPickerId);
  expect(contentData.values[0].value[0]['type']).toEqual('document');

  // Clean
  await umbracoApi.document.ensureNameNotExists(allowedContentPickerName);
  await umbracoApi.document.ensureNameNotExists(notAllowedContentPickerName);
});

test('can search and see only allowed content types', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const allowedContentPickerDocumentTypeName = 'ContentPickerDocumentType';
  const allowedContentPickerName = 'Test Content Picker';
  const notAllowedContentPickerName = 'Not Allowed Test Content Picker';
  const allowedContentPickerDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(allowedContentPickerDocumentTypeName);
  const allowedContentPickerId = await umbracoApi.document.createDefaultDocument(allowedContentPickerName, allowedContentPickerDocumentTypeId);
  // Create a content with custom content picker with predefined allowed types
  const customDataTypeId = await umbracoApi.dataType.createMultiNodeTreePickerDataTypeWithAllowedTypes(customDataTypeName, allowedContentPickerDocumentTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocument(notAllowedContentPickerName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();

  // Assert
  await umbracoUi.content.isModalMenuItemWithNameVisible(allowedContentPickerName);
  await umbracoUi.content.isModalMenuItemWithNameDisabled(notAllowedContentPickerName);
  await umbracoUi.content.enterSearchKeywordInTreePickerModal('Picker');
  await umbracoUi.content.isModalMenuItemWithNameVisible(notAllowedContentPickerName, false);
  await umbracoUi.content.clickEntityItemByName(allowedContentPickerName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessStateIconVisible();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0]['unique']).toEqual(allowedContentPickerId);
  expect(contentData.values[0].value[0]['type']).toEqual('document');

  // Clean
  await umbracoApi.document.ensureNameNotExists(allowedContentPickerName);
  await umbracoApi.document.ensureNameNotExists(notAllowedContentPickerName);
});

test('can search and see only allowed media types', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const allowedMediaPickerName = 'Test Image';
  const notAllowedMediaPickerName = 'Test Article';
  const allowedMediaPickerId = await umbracoApi.media.createDefaultMediaWithImage(allowedMediaPickerName);
  await umbracoApi.media.createDefaultMediaWithArticle(notAllowedMediaPickerName);
  const imageMediaTypeData = await umbracoApi.mediaType.getByName('Image');
  // Create a content with custom tree picker with predefined allowed media types
  const customDataTypeId = await umbracoApi.dataType.createMultiNodeTreePickerDataTypeWithAllowedTypes(customDataTypeName, imageMediaTypeData.id, 'media');
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();

  // Assert
  await umbracoUi.content.isMediaCardItemWithNameDisabled(notAllowedMediaPickerName);
  await umbracoUi.content.isMediaCardItemWithNameVisible(allowedMediaPickerName);
  await umbracoUi.content.enterSearchKeywordInMediaPickerModal('Test');
  await umbracoUi.content.isMediaCardItemWithNameVisible(notAllowedMediaPickerName, false);
  await umbracoUi.content.clickMediaWithName(allowedMediaPickerName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessStateIconVisible();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0]['unique']).toEqual(allowedMediaPickerId);
  expect(contentData.values[0].value[0]['type']).toEqual('media');

  // Clean
  await umbracoApi.media.ensureNameNotExists(allowedMediaPickerName);
  await umbracoApi.media.ensureNameNotExists(notAllowedMediaPickerName);
});

test('can search and see only allowed member types', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Not allowed member type
  const notAllowedMemberTypeName = 'Not Allowed Member Type';
  const notAllowedMemberTypeId = await umbracoApi.memberType.createDefaultMemberType(notAllowedMemberTypeName);
  // Allowed member type
  const allowedMemberTypeData = await umbracoApi.memberType.getByName('Member');
  // Allowed member
  const allowedTestMember = {
    name : 'Allowed Test Member',
    username : 'allowedTestMember',
    email : 'allowedTestMember@acceptance.test',
    password : '0123456789',
  };
  const allowedTestMemberId = await umbracoApi.member.createDefaultMember(allowedTestMember.name, allowedMemberTypeData.id, allowedTestMember.email, allowedTestMember.username, allowedTestMember.password);
  // Not allowed member
  const notAllowedTestMember = {
    name : 'Not Allowed Test Member',
    username : 'notAllowedTestMember',
    email : 'notAllowedTestMember@acceptance.test',
    password : '0123456789',
  };
  await umbracoApi.member.createDefaultMember(notAllowedTestMember.name, notAllowedMemberTypeId, notAllowedTestMember.email, notAllowedTestMember.username, notAllowedTestMember.password);
  // Create a content with custom tree picker with predefined allowed member types
  const customDataTypeId = await umbracoApi.dataType.createMultiNodeTreePickerDataTypeWithAllowedTypes(customDataTypeName, allowedMemberTypeData.id, 'member');
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();

  // Assert
  await umbracoUi.content.isModalMenuItemWithNameVisible(allowedTestMember.name);
  await umbracoUi.content.isModalMenuItemWithNameDisabled(notAllowedTestMember.name);
  await umbracoUi.content.enterSearchKeywordInMemberPickerModal('Allowed Test Member');
  await umbracoUi.content.isModalMenuItemWithNameVisible(notAllowedTestMember.name, false);
  await umbracoUi.content.clickEntityItemByName(allowedTestMember.name);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButton();
  await umbracoUi.content.isSuccessStateIconVisible();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0]['unique']).toEqual(allowedTestMemberId);
  expect(contentData.values[0].value[0]['type']).toEqual('member');

  // Clean
  await umbracoApi.member.ensureNameNotExists(allowedTestMember.name);
  await umbracoApi.member.ensureNameNotExists(notAllowedTestMember.name);
  await umbracoApi.memberType.ensureNameNotExists(notAllowedMemberTypeName);
});