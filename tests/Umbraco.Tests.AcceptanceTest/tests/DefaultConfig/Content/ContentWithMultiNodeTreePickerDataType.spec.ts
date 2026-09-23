import {ApiHelpers, ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {BrowserContext, expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'CustomMultiNodeTreePicker';
const allowedTestMemberName = 'Allowed Test Member';
const notAllowedTestMemberName = 'Not Allowed Test Member';
const notAllowedMemberTypeName = 'Not Allowed Member Type';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  await umbracoApi.member.ensureNameNotExists(allowedTestMemberName);
  await umbracoApi.member.ensureNameNotExists(notAllowedTestMemberName);
  await umbracoApi.memberType.ensureNameNotExists(notAllowedMemberTypeName);
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
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeCreated();

  // Assert
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
  const notAllowedContentPickerId = await umbracoApi.document.createDefaultDocument(notAllowedContentPickerName, documentTypeId);
  await umbracoApi.document.waitUntilIndexed('Picker', allowedContentPickerId);
  await umbracoApi.document.waitUntilIndexed('Picker', notAllowedContentPickerId);
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
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
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
  const notAllowedMediaPickerId = await umbracoApi.media.createDefaultMediaWithArticle(notAllowedMediaPickerName);
  await umbracoApi.media.waitUntilIndexed('Test', allowedMediaPickerId);
  await umbracoApi.media.waitUntilIndexed('Test', notAllowedMediaPickerId);
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
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
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
  const notAllowedMemberTypeId = await umbracoApi.memberType.createDefaultMemberType(notAllowedMemberTypeName);
  // Allowed member type
  const allowedMemberTypeData = await umbracoApi.memberType.getByName('Member');
  // Allowed member
  const allowedTestMember = {
    name : allowedTestMemberName,
    username : 'allowedTestMember',
    email : 'allowedTestMember@acceptance.test',
    password : '0123456789',
  };
  const allowedTestMemberId = await umbracoApi.member.createDefaultMember(allowedTestMember.name, allowedMemberTypeData.id, allowedTestMember.email, allowedTestMember.username, allowedTestMember.password);
  // Not allowed member
  const notAllowedTestMember = {
    name : notAllowedTestMemberName,
    username : 'notAllowedTestMember',
    email : 'notAllowedTestMember@acceptance.test',
    password : '0123456789',
  };
  const notAllowedTestMemberId = await umbracoApi.member.createDefaultMember(notAllowedTestMember.name, notAllowedMemberTypeId, notAllowedTestMember.email, notAllowedTestMember.username, notAllowedTestMember.password);
  await umbracoApi.member.waitUntilIndexed(allowedTestMemberName, allowedTestMemberId);
  await umbracoApi.member.waitUntilIndexed(notAllowedTestMemberName, notAllowedTestMemberId);
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
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0]['unique']).toEqual(allowedTestMemberId);
  expect(contentData.values[0].value[0]['type']).toEqual('member');
});

test('can pick multiple items with a multi node tree picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstPickerItemName = 'First Picker Item';
  const secondPickerItemName = 'Second Picker Item';
  const pickerItemDocumentTypeName = 'MultiPickerItemDocumentType';
  const pickerItemDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(pickerItemDocumentTypeName);
  const firstPickerItemId = await umbracoApi.document.createDefaultDocument(firstPickerItemName, pickerItemDocumentTypeId);
  const secondPickerItemId = await umbracoApi.document.createDefaultDocument(secondPickerItemName, pickerItemDocumentTypeId);
  // No min/max configured, so the picker defaults to unlimited selection.
  const customDataTypeId = await umbracoApi.dataType.createDefaultContentPickerSourceDataType(customDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.selectLinkByName(firstPickerItemName);
  await umbracoUi.content.selectLinkByName(secondPickerItemName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.length).toBe(2);
  const pickedIds = contentData.values[0].value.map((item: {unique: string}) => item.unique);
  expect(pickedIds).toContain(firstPickerItemId);
  expect(pickedIds).toContain(secondPickerItemId);

  // Clean
  await umbracoApi.document.ensureNameNotExists(firstPickerItemName);
  await umbracoApi.document.ensureNameNotExists(secondPickerItemName);
  await umbracoApi.documentType.ensureNameNotExists(pickerItemDocumentTypeName);
});

test('can see validation error clear when minimum number of items is met', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minNumberDataTypeName = 'MinNumberMultiNodeTreePicker';
  const minNumberDocumentTypeName = 'MinNumberDocumentType';
  const firstPickerItemName = 'First Picker Item';
  const secondPickerItemName = 'Second Picker Item';
  const pickerItemDocumentTypeName = 'MultiPickerItemDocumentType';
  const pickerItemDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(pickerItemDocumentTypeName);
  await umbracoApi.document.createDefaultDocument(firstPickerItemName, pickerItemDocumentTypeId);
  await umbracoApi.document.createDefaultDocument(secondPickerItemName, pickerItemDocumentTypeId);
  const minNumberDataTypeId = await umbracoApi.dataType.createMultiNodeTreePickerDataTypeWithMinNumberOfItems(minNumberDataTypeName, 2);
  const minNumberDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(minNumberDocumentTypeName, minNumberDataTypeName, minNumberDataTypeId, 'TestGroup', false, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, minNumberDocumentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.selectLinkByName(firstPickerItemName);
  await umbracoUi.content.clickChooseModalButton();
  // Try to save with only 1 item picked (min is 2)
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.minimumTwoEntriesRequiresOneMore);
  // Add a second item
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.selectLinkByName(secondPickerItemName);
  await umbracoUi.content.clickChooseModalButton();
  // Validation should clear
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.minimumTwoEntriesRequiresOneMore, false);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.length).toBe(2);

  // Clean
  await umbracoApi.document.ensureNameNotExists(firstPickerItemName);
  await umbracoApi.document.ensureNameNotExists(secondPickerItemName);
  await umbracoApi.documentType.ensureNameNotExists(pickerItemDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(minNumberDocumentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(minNumberDataTypeName);
});

// The picker opens at an unrestricted root (no start node configured), so this exercises browsing/drilling
// into a node that has a collection from a higher tree level, then picking an item from that collection -
// not a picker configured to land on a collection directly.
test('can browse into a collection and pick an item from it', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const collectionContentName = 'Collection Root Content';
  const collectionDocumentTypeName = 'CollectionDocumentType';
  const collectionChildDocumentTypeName = 'CollectionChildDocumentType';
  const firstCollectionItemName = 'First Collection Item';
  const secondCollectionItemName = 'Second Collection Item';
  const listViewDataTypeName = 'List View - Content';

  const collectionChildDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(collectionChildDocumentTypeName);
  const listViewDataTypeData = await umbracoApi.dataType.getByName(listViewDataTypeName);
  const collectionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(collectionDocumentTypeName, collectionChildDocumentTypeId, listViewDataTypeData.id);
  const collectionContentId = await umbracoApi.document.createDefaultDocument(collectionContentName, collectionDocumentTypeId);
  const firstCollectionItemId = await umbracoApi.document.createDefaultDocumentWithParent(firstCollectionItemName, collectionChildDocumentTypeId, collectionContentId);
  await umbracoApi.document.createDefaultDocumentWithParent(secondCollectionItemName, collectionChildDocumentTypeId, collectionContentId);
  // No start node restriction, so the picker opens at the tree root, where the collection node above
  // appears as a normal (root-allowed) item to browse into.
  const customDataTypeId = await umbracoApi.dataType.createDefaultContentPickerSourceDataType(customDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  // The caret drills into the node (its name would select it instead).
  await umbracoUi.content.openCaretButtonForName(collectionContentName, true);
  await umbracoUi.content.clickCollectionCardInPickerModal(firstCollectionItemName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0]['unique']).toEqual(firstCollectionItemId);
  expect(contentData.values[0].value[0]['type']).toEqual('document');

  // Clean
  await umbracoApi.document.ensureNameNotExists(collectionContentName);
  await umbracoApi.document.ensureNameNotExists(firstCollectionItemName);
  await umbracoApi.document.ensureNameNotExists(secondCollectionItemName);
  await umbracoApi.documentType.ensureNameNotExists(collectionDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(collectionChildDocumentTypeName);
});

test.describe('can pick multiple items from a collection within the picker', () => {
  // The collection (document type + root + 2 children) is read-only for these tests - only browsed, never
  // mutated - so it's safe to create once in beforeAll and share across both tests. The host content that
  // actually receives the picked value is still created fresh per test, so one test's save can't leave
  // picked items in place for the other to trivially pass against.
  const collectionContentName = 'Collection Root Content';
  const collectionDocumentTypeName = 'CollectionDocumentType';
  const collectionChildDocumentTypeName = 'CollectionChildDocumentType';
  const firstCollectionItemName = 'First Collection Item';
  const secondCollectionItemName = 'Second Collection Item';

  let sharedContext: BrowserContext;
  let sharedApi: ApiHelpers;
  let firstCollectionItemId: string;
  let secondCollectionItemId: string;

  test.beforeAll(async ({browser}, testInfo) => {
    sharedContext = await browser.newContext(testInfo.project.use);
    sharedApi = new ApiHelpers(await sharedContext.newPage());
    await sharedApi.isLoginStateValid();

    const listViewDataTypeName = 'List View - Content';
    const collectionChildDocumentTypeId = await sharedApi.documentType.createDefaultDocumentType(collectionChildDocumentTypeName);
    const listViewDataTypeData = await sharedApi.dataType.getByName(listViewDataTypeName);
    const collectionDocumentTypeId = await sharedApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(collectionDocumentTypeName, collectionChildDocumentTypeId, listViewDataTypeData.id);
    const collectionContentId = await sharedApi.document.createDefaultDocument(collectionContentName, collectionDocumentTypeId);
    firstCollectionItemId = await sharedApi.document.createDefaultDocumentWithParent(firstCollectionItemName, collectionChildDocumentTypeId, collectionContentId);
    secondCollectionItemId = await sharedApi.document.createDefaultDocumentWithParent(secondCollectionItemName, collectionChildDocumentTypeId, collectionContentId);
  });

  test.afterAll(async () => {
    await sharedApi.document.ensureNameNotExists(collectionContentName);
    await sharedApi.document.ensureNameNotExists(firstCollectionItemName);
    await sharedApi.document.ensureNameNotExists(secondCollectionItemName);
    await sharedApi.documentType.ensureNameNotExists(collectionDocumentTypeName);
    await sharedApi.documentType.ensureNameNotExists(collectionChildDocumentTypeName);
    await sharedContext.close();
  });

  test('can pick multiple items from a collection browsed via the picker', async ({umbracoApi, umbracoUi}) => {
    // Arrange - No min/max configured, so the picker defaults to unlimited selection.
    const customDataTypeId = await umbracoApi.dataType.createDefaultContentPickerSourceDataType(customDataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickChooseButton();
    await umbracoUi.content.openCaretButtonForName(collectionContentName, true);
    await umbracoUi.content.clickCollectionCardInPickerModal(firstCollectionItemName);
    await umbracoUi.content.clickCollectionCardInPickerModal(secondCollectionItemName);
    await umbracoUi.content.clickChooseModalButton();
    await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

    // Assert
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value.length).toBe(2);
    const pickedIds = contentData.values[0].value.map((item: {unique: string}) => item.unique);
    expect(pickedIds).toContain(firstCollectionItemId);
    expect(pickedIds).toContain(secondCollectionItemId);
  });

  test('can pick multiple items from a collection table view within the picker', async ({umbracoApi, umbracoUi}) => {
    // Arrange - No min/max configured, so the picker defaults to unlimited selection.
    const customDataTypeId = await umbracoApi.dataType.createDefaultContentPickerSourceDataType(customDataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickChooseButton();
    await umbracoUi.content.openCaretButtonForName(collectionContentName, true);
    await umbracoUi.content.changeToListView();
    await umbracoUi.content.selectContentWithNameInListView(firstCollectionItemName);
    await umbracoUi.content.selectContentWithNameInListView(secondCollectionItemName);
    await umbracoUi.content.clickChooseModalButton();
    await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

    // Assert
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value.length).toBe(2);
    const pickedIds = contentData.values[0].value.map((item: {unique: string}) => item.unique);
    expect(pickedIds).toContain(firstCollectionItemId);
    expect(pickedIds).toContain(secondCollectionItemId);
  });
});

test('can switch a collection to table view within the picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const collectionContentName = 'Collection Root Content';
  const collectionDocumentTypeName = 'CollectionDocumentType';
  const collectionChildDocumentTypeName = 'CollectionChildDocumentType';
  const collectionItemName = 'Collection Item';
  const listViewDataTypeName = 'List View - Content';

  const collectionChildDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(collectionChildDocumentTypeName);
  const listViewDataTypeData = await umbracoApi.dataType.getByName(listViewDataTypeName);
  const collectionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(collectionDocumentTypeName, collectionChildDocumentTypeId, listViewDataTypeData.id);
  const collectionContentId = await umbracoApi.document.createDefaultDocument(collectionContentName, collectionDocumentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(collectionItemName, collectionChildDocumentTypeId, collectionContentId);
  const customDataTypeId = await umbracoApi.dataType.createDefaultContentPickerSourceDataType(customDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.openCaretButtonForName(collectionContentName, true);
  await umbracoUi.content.changeToListView();

  // Assert
  await umbracoUi.content.isDocumentListViewVisible();

  // Clean
  await umbracoApi.document.ensureNameNotExists(collectionContentName);
  await umbracoApi.document.ensureNameNotExists(collectionItemName);
  await umbracoApi.documentType.ensureNameNotExists(collectionDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(collectionChildDocumentTypeName);
});

test('can sort a collection by name within the picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const collectionContentName = 'Collection Root Content';
  const collectionDocumentTypeName = 'CollectionDocumentType';
  const collectionChildDocumentTypeName = 'CollectionChildDocumentType';
  const firstCollectionItemName = 'Middle Collection Item';
  const secondCollectionItemName = 'Apple Collection Item';
  const thirdCollectionItemName = 'Zebra Collection Item';
  const listViewDataTypeName = 'List View - Content';

  const collectionChildDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(collectionChildDocumentTypeName);
  const listViewDataTypeData = await umbracoApi.dataType.getByName(listViewDataTypeName);
  const collectionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(collectionDocumentTypeName, collectionChildDocumentTypeId, listViewDataTypeData.id);
  const collectionContentId = await umbracoApi.document.createDefaultDocument(collectionContentName, collectionDocumentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(firstCollectionItemName, collectionChildDocumentTypeId, collectionContentId);
  await umbracoApi.document.createDefaultDocumentWithParent(secondCollectionItemName, collectionChildDocumentTypeId, collectionContentId);
  await umbracoApi.document.createDefaultDocumentWithParent(thirdCollectionItemName, collectionChildDocumentTypeId, collectionContentId);
  const customDataTypeId = await umbracoApi.dataType.createDefaultContentPickerSourceDataType(customDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.openCaretButtonForName(collectionContentName, true);
  await umbracoUi.content.changeToListView();
  await umbracoUi.content.clickNameButtonInListView();

  // Assert
  await umbracoUi.content.doesFirstItemInListViewHaveName(secondCollectionItemName);

  // Clean
  await umbracoApi.document.ensureNameNotExists(collectionContentName);
  await umbracoApi.document.ensureNameNotExists(firstCollectionItemName);
  await umbracoApi.document.ensureNameNotExists(secondCollectionItemName);
  await umbracoApi.document.ensureNameNotExists(thirdCollectionItemName);
  await umbracoApi.documentType.ensureNameNotExists(collectionDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(collectionChildDocumentTypeName);
});

test('can filter a collection within the picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const collectionContentName = 'Collection Root Content';
  const collectionDocumentTypeName = 'CollectionDocumentType';
  const collectionChildDocumentTypeName = 'CollectionChildDocumentType';
  const firstCollectionItemName = 'First Collection Item';
  const secondCollectionItemName = 'Second Collection Item';
  const listViewDataTypeName = 'List View - Content';

  const collectionChildDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(collectionChildDocumentTypeName);
  const listViewDataTypeData = await umbracoApi.dataType.getByName(listViewDataTypeName);
  const collectionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(collectionDocumentTypeName, collectionChildDocumentTypeId, listViewDataTypeData.id);
  const collectionContentId = await umbracoApi.document.createDefaultDocument(collectionContentName, collectionDocumentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(firstCollectionItemName, collectionChildDocumentTypeId, collectionContentId);
  await umbracoApi.document.createDefaultDocumentWithParent(secondCollectionItemName, collectionChildDocumentTypeId, collectionContentId);
  const customDataTypeId = await umbracoApi.dataType.createDefaultContentPickerSourceDataType(customDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.openCaretButtonForName(collectionContentName, true);
  await umbracoUi.content.changeToListView();
  await umbracoUi.content.searchByKeywordInCollection(firstCollectionItemName);

  // Assert
  await umbracoUi.content.doesListViewContainCount(1);
  await umbracoUi.content.doesFirstItemInListViewHaveName(firstCollectionItemName);

  // Clean
  await umbracoApi.document.ensureNameNotExists(collectionContentName);
  await umbracoApi.document.ensureNameNotExists(firstCollectionItemName);
  await umbracoApi.document.ensureNameNotExists(secondCollectionItemName);
  await umbracoApi.documentType.ensureNameNotExists(collectionDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(collectionChildDocumentTypeName);
});

test('can select an item from a collection table view within the picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const collectionContentName = 'Collection Root Content';
  const collectionDocumentTypeName = 'CollectionDocumentType';
  const collectionChildDocumentTypeName = 'CollectionChildDocumentType';
  const firstCollectionItemName = 'First Collection Item';
  const secondCollectionItemName = 'Second Collection Item';
  const listViewDataTypeName = 'List View - Content';

  const collectionChildDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(collectionChildDocumentTypeName);
  const listViewDataTypeData = await umbracoApi.dataType.getByName(listViewDataTypeName);
  const collectionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(collectionDocumentTypeName, collectionChildDocumentTypeId, listViewDataTypeData.id);
  const collectionContentId = await umbracoApi.document.createDefaultDocument(collectionContentName, collectionDocumentTypeId);
  const firstCollectionItemId = await umbracoApi.document.createDefaultDocumentWithParent(firstCollectionItemName, collectionChildDocumentTypeId, collectionContentId);
  await umbracoApi.document.createDefaultDocumentWithParent(secondCollectionItemName, collectionChildDocumentTypeId, collectionContentId);
  const customDataTypeId = await umbracoApi.dataType.createDefaultContentPickerSourceDataType(customDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.openCaretButtonForName(collectionContentName, true);
  await umbracoUi.content.changeToListView();
  await umbracoUi.content.selectContentWithNameInListView(firstCollectionItemName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0]['unique']).toEqual(firstCollectionItemId);
  expect(contentData.values[0].value[0]['type']).toEqual('document');

  // Clean
  await umbracoApi.document.ensureNameNotExists(collectionContentName);
  await umbracoApi.document.ensureNameNotExists(firstCollectionItemName);
  await umbracoApi.document.ensureNameNotExists(secondCollectionItemName);
  await umbracoApi.documentType.ensureNameNotExists(collectionDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(collectionChildDocumentTypeName);
});

test('can only pick allowed types from a collection grid view within the picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const collectionContentName = 'Collection Root Content';
  const collectionDocumentTypeName = 'CollectionDocumentType';
  const allowedCollectionChildDocumentTypeName = 'AllowedCollectionChildDocumentType';
  const notAllowedCollectionChildDocumentTypeName = 'NotAllowedCollectionChildDocumentType';
  const allowedCollectionItemName = 'Permitted Collection Item';
  const notAllowedCollectionItemName = 'Restricted Collection Item';
  const listViewDataTypeName = 'List View - Content';

  const allowedCollectionChildDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(allowedCollectionChildDocumentTypeName);
  const notAllowedCollectionChildDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(notAllowedCollectionChildDocumentTypeName);
  const listViewDataTypeData = await umbracoApi.dataType.getByName(listViewDataTypeName);
  const collectionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodesAndCollectionId(collectionDocumentTypeName, [allowedCollectionChildDocumentTypeId, notAllowedCollectionChildDocumentTypeId], listViewDataTypeData.id);
  const collectionContentId = await umbracoApi.document.createDefaultDocument(collectionContentName, collectionDocumentTypeId);
  const allowedCollectionItemId = await umbracoApi.document.createDefaultDocumentWithParent(allowedCollectionItemName, allowedCollectionChildDocumentTypeId, collectionContentId);
  await umbracoApi.document.createDefaultDocumentWithParent(notAllowedCollectionItemName, notAllowedCollectionChildDocumentTypeId, collectionContentId);
  // The picker is restricted to the allowed child type only, so the other type's item appears in the collection but cannot be picked.
  const customDataTypeId = await umbracoApi.dataType.createMultiNodeTreePickerDataTypeWithAllowedTypes(customDataTypeName, allowedCollectionChildDocumentTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.openCaretButtonForName(collectionContentName, true);

  // Assert
  await umbracoUi.content.isContentCardSelectableForName(notAllowedCollectionItemName, false);
  await umbracoUi.content.isContentCardSelectableForName(allowedCollectionItemName);

  // Act
  await umbracoUi.content.clickCollectionCardInPickerModal(allowedCollectionItemName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0]['unique']).toEqual(allowedCollectionItemId);
  expect(contentData.values[0].value[0]['type']).toEqual('document');

  // Clean
  await umbracoApi.document.ensureNameNotExists(collectionContentName);
  await umbracoApi.document.ensureNameNotExists(allowedCollectionItemName);
  await umbracoApi.document.ensureNameNotExists(notAllowedCollectionItemName);
  await umbracoApi.documentType.ensureNameNotExists(collectionDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(allowedCollectionChildDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(notAllowedCollectionChildDocumentTypeName);
});

test('can only pick allowed types from a collection table view within the picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const collectionContentName = 'Collection Root Content';
  const collectionDocumentTypeName = 'CollectionDocumentType';
  const allowedCollectionChildDocumentTypeName = 'AllowedCollectionChildDocumentType';
  const notAllowedCollectionChildDocumentTypeName = 'NotAllowedCollectionChildDocumentType';
  const allowedCollectionItemName = 'Permitted Collection Item';
  const notAllowedCollectionItemName = 'Restricted Collection Item';
  const listViewDataTypeName = 'List View - Content';

  const allowedCollectionChildDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(allowedCollectionChildDocumentTypeName);
  const notAllowedCollectionChildDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(notAllowedCollectionChildDocumentTypeName);
  const listViewDataTypeData = await umbracoApi.dataType.getByName(listViewDataTypeName);
  const collectionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodesAndCollectionId(collectionDocumentTypeName, [allowedCollectionChildDocumentTypeId, notAllowedCollectionChildDocumentTypeId], listViewDataTypeData.id);
  const collectionContentId = await umbracoApi.document.createDefaultDocument(collectionContentName, collectionDocumentTypeId);
  const allowedCollectionItemId = await umbracoApi.document.createDefaultDocumentWithParent(allowedCollectionItemName, allowedCollectionChildDocumentTypeId, collectionContentId);
  await umbracoApi.document.createDefaultDocumentWithParent(notAllowedCollectionItemName, notAllowedCollectionChildDocumentTypeId, collectionContentId);
  // The picker is restricted to the allowed child type only, so the other type's item appears in the collection but cannot be picked.
  const customDataTypeId = await umbracoApi.dataType.createMultiNodeTreePickerDataTypeWithAllowedTypes(customDataTypeName, allowedCollectionChildDocumentTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.openCaretButtonForName(collectionContentName, true);
  await umbracoUi.content.changeToListView();

  // Assert
  await umbracoUi.content.isListViewTableRowSelectableForName(notAllowedCollectionItemName, false);
  await umbracoUi.content.isListViewTableRowSelectableForName(allowedCollectionItemName);

  // Act
  await umbracoUi.content.selectContentWithNameInListView(allowedCollectionItemName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0]['unique']).toEqual(allowedCollectionItemId);
  expect(contentData.values[0].value[0]['type']).toEqual('document');

  // Clean
  await umbracoApi.document.ensureNameNotExists(collectionContentName);
  await umbracoApi.document.ensureNameNotExists(allowedCollectionItemName);
  await umbracoApi.document.ensureNameNotExists(notAllowedCollectionItemName);
  await umbracoApi.documentType.ensureNameNotExists(collectionDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(allowedCollectionChildDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(notAllowedCollectionChildDocumentTypeName);
});

test('can switch the tree to table view within the picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const itemName = 'Tree Table View Item';
  const itemDocumentTypeName = 'TreeTableViewItemDocumentType';

  const itemDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(itemDocumentTypeName);
  await umbracoApi.document.createDefaultDocument(itemName, itemDocumentTypeId);
  const customDataTypeId = await umbracoApi.dataType.createDefaultContentPickerSourceDataType(customDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.changeTreeToTableView();

  // Assert
  await umbracoUi.content.isTreeTableViewVisible();

  // Clean
  await umbracoApi.document.ensureNameNotExists(itemName);
  await umbracoApi.documentType.ensureNameNotExists(itemDocumentTypeName);
});

test('can drill into an item with children by clicking it in the table view', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const parentContentName = 'Table Drillable Parent';
  const childContentName = 'Table Drillable Child';
  const parentDocumentTypeName = 'TableDrillableParentDocumentType';
  const childDocumentTypeName = 'TableDrillableChildDocumentType';

  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const parentDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(parentDocumentTypeName, childDocumentTypeId);
  const parentContentId = await umbracoApi.document.createDefaultDocument(parentContentName, parentDocumentTypeId);
  const childContentId = await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, parentContentId);
  const customDataTypeId = await umbracoApi.dataType.createDefaultContentPickerSourceDataType(customDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.changeTreeToTableView();
  await umbracoUi.content.clickOpenButtonInTreeTableRowWithName(parentContentName);
  await umbracoUi.content.selectTreeTableRowWithName(childContentName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0]['unique']).toEqual(childContentId);
  expect(contentData.values[0].value[0]['type']).toEqual('document');

  // Clean
  await umbracoApi.document.ensureNameNotExists(parentContentName);
  await umbracoApi.document.ensureNameNotExists(childContentName);
  await umbracoApi.documentType.ensureNameNotExists(parentDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('can select an item in the tree table view within the picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const itemName = 'Tree Table Select Item';
  const itemDocumentTypeName = 'TreeTableSelectItemDocumentType';

  const itemDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(itemDocumentTypeName);
  const itemId = await umbracoApi.document.createDefaultDocument(itemName, itemDocumentTypeId);
  const customDataTypeId = await umbracoApi.dataType.createDefaultContentPickerSourceDataType(customDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.changeTreeToTableView();
  await umbracoUi.content.selectTreeTableRowWithName(itemName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0]['unique']).toEqual(itemId);
  expect(contentData.values[0].value[0]['type']).toEqual('document');

  // Clean
  await umbracoApi.document.ensureNameNotExists(itemName);
  await umbracoApi.documentType.ensureNameNotExists(itemDocumentTypeName);
});

test('can select multiple items in the tree table view within the picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstItemName = 'First Tree Table Item';
  const secondItemName = 'Second Tree Table Item';
  const itemDocumentTypeName = 'MultiSelectTreeTableItemDocumentType';

  const itemDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(itemDocumentTypeName);
  const firstItemId = await umbracoApi.document.createDefaultDocument(firstItemName, itemDocumentTypeId);
  const secondItemId = await umbracoApi.document.createDefaultDocument(secondItemName, itemDocumentTypeId);
  // No min/max configured, so the picker defaults to unlimited selection.
  const customDataTypeId = await umbracoApi.dataType.createDefaultContentPickerSourceDataType(customDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.changeTreeToTableView();
  await umbracoUi.content.selectTreeTableRowWithName(firstItemName);
  await umbracoUi.content.selectTreeTableRowWithName(secondItemName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.length).toBe(2);
  const pickedIds = contentData.values[0].value.map((item: {unique: string}) => item.unique);
  expect(pickedIds).toContain(firstItemId);
  expect(pickedIds).toContain(secondItemId);

  // Clean
  await umbracoApi.document.ensureNameNotExists(firstItemName);
  await umbracoApi.document.ensureNameNotExists(secondItemName);
  await umbracoApi.documentType.ensureNameNotExists(itemDocumentTypeName);
});

test('can only select allowed types in the tree table view within the picker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const allowedItemName = 'Permitted Tree Table Item';
  const notAllowedItemName = 'Restricted Tree Table Item';
  const allowedItemDocumentTypeName = 'PermittedTreeTableItemDocumentType';
  const notAllowedItemDocumentTypeName = 'RestrictedTreeTableItemDocumentType';

  const allowedItemDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(allowedItemDocumentTypeName);
  const notAllowedItemDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(notAllowedItemDocumentTypeName);
  const allowedItemId = await umbracoApi.document.createDefaultDocument(allowedItemName, allowedItemDocumentTypeId);
  await umbracoApi.document.createDefaultDocument(notAllowedItemName, notAllowedItemDocumentTypeId);
  const customDataTypeId = await umbracoApi.dataType.createMultiNodeTreePickerDataTypeWithAllowedTypes(customDataTypeName, allowedItemDocumentTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickChooseButton();
  await umbracoUi.content.changeTreeToTableView();

  // Assert
  await umbracoUi.content.isTreeTableRowSelectableForName(notAllowedItemName, false);
  await umbracoUi.content.isTreeTableRowSelectableForName(allowedItemName);

  // Act
  await umbracoUi.content.selectTreeTableRowWithName(allowedItemName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0]['unique']).toEqual(allowedItemId);
  expect(contentData.values[0].value[0]['type']).toEqual('document');

  // Clean
  await umbracoApi.document.ensureNameNotExists(allowedItemName);
  await umbracoApi.document.ensureNameNotExists(notAllowedItemName);
  await umbracoApi.documentType.ensureNameNotExists(allowedItemDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(notAllowedItemDocumentTypeName);
});
