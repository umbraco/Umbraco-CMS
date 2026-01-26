import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const documentTypeName = 'TestDocumentType';
const contentName = 'TestContent';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.relationType.goToSettingsTreeItem('Relations');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

const relationTypes = [
  {name: 'Relate Document On Copy', parentType: 'Document', childType: 'Document', biDirectional: 'true', dependency: 'false'},
  {name: 'Relate Parent Document On Delete', parentType: 'Document', childType: 'Document', biDirectional: 'false', dependency: 'false'},
  {name: 'Relate Parent Media Folder On Delete', parentType: 'Media', childType: 'Media', biDirectional: 'false', dependency: 'false'},
  {name: 'Related Document', parentType: '', childType: '', biDirectional: 'false', dependency: 'true'},
  {name: 'Related Media', parentType: '', childType: '', biDirectional: 'false', dependency: 'true'},
  {name: 'Related Member', parentType: '', childType: '', biDirectional: 'false', dependency: 'true'}
];

for (const relationType of relationTypes) {
  test(`can see relation type ${relationType.name}`, async ({umbracoUi}) => {
    // Act
    await umbracoUi.waitForTimeout(ConstantHelper.wait.long);
    await umbracoUi.relationType.goToRelationTypeWithName(relationType.name);

    // Assert
    await umbracoUi.relationType.doesParentTypeContainValue(relationType.parentType);
    await umbracoUi.relationType.doesChildTypeContainValue(relationType.childType);
    await umbracoUi.relationType.doesBidirectionalContainValue(relationType.biDirectional);
    await umbracoUi.relationType.doesDependencyContainValue(relationType.dependency);
  });
}

test('can see related document in relation type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Content Picker
  const contentPickerName = 'Content Picker';
  const contentPickerData = await umbracoApi.dataType.getByName(contentPickerName);
  // Document Type
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, contentPickerName, contentPickerData.id);
  // Content
  const contentToBePickedName = 'ContentToBePicked';
  const contentToBePickedId = await umbracoApi.document.createDefaultDocument(contentToBePickedName, documentTypeId);
  await umbracoApi.document.createDocumentWithContentPicker(contentName, documentTypeId, contentToBePickedId);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.long);

  // Act
  await umbracoUi.relationType.goToRelationTypeWithName('Related Document');

  // Assert
  await umbracoUi.relationType.isRelationWithParentAndChildVisible(contentName, contentToBePickedName);
});

test('can see related media in relation type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Media Picker
  const mediaPickerName = 'Media Picker';
  const mediaPickerData = await umbracoApi.dataType.getByName(mediaPickerName);
  // Media
  const mediaName = 'TestMedia';
  const mediaFileId = await umbracoApi.media.createDefaultMediaFile(mediaName);
  // Document Type
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, mediaPickerName, mediaPickerData.id);
  // Content
  await umbracoApi.document.createDocumentWithOneMediaPicker(contentName, documentTypeId, mediaFileId);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.long);

  // Act
  await umbracoUi.relationType.goToRelationTypeWithName('Related Media');

  // Assert
  await umbracoUi.relationType.isRelationWithParentAndChildVisible(contentName, mediaName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can see related member in relation type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // MemberPicker
  const memberPickerName = 'Member Picker';
  const memberPickerData = await umbracoApi.dataType.getByName(memberPickerName);
  // Member
  const memberTypeData = await umbracoApi.memberType.getByName('Member');
  const memberName = 'TestMember';
  const memberEmail = 'TestMemberEmail@test.com';
  const memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeData.id, memberEmail, memberEmail, memberEmail);
  // DocumentType
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, memberPickerName, memberPickerData.id);
  // Content
  await umbracoApi.document.createDocumentWithMemberPicker(contentName, documentTypeId, memberId);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.long);

  // Act
  await umbracoUi.relationType.goToRelationTypeWithName('Related Member');

  // Assert
  await umbracoUi.relationType.isRelationWithParentAndChildVisible(contentName, memberName);

  // Clean
  await umbracoApi.member.ensureNameNotExists(memberName);
});

test('can not see relation after content with relation is deleted', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Content Picker
  const contentPickerName = 'Content Picker';
  const contentPickerData = await umbracoApi.dataType.getByName(contentPickerName);
  // Document Type
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, contentPickerName, contentPickerData.id);
  // Content
  const contentToBePickedName = 'ContentToBePicked';
  const contentToBePickedId = await umbracoApi.document.createDefaultDocument(contentToBePickedName, documentTypeId);
  await umbracoApi.document.createDocumentWithContentPicker(contentName, documentTypeId, contentToBePickedId);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.long);

  await umbracoUi.relationType.goToRelationTypeWithName('Related Document');
  await umbracoUi.relationType.isRelationWithParentAndChildVisible(contentName, contentToBePickedName);

  // Act
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);

  // Assert
  await umbracoUi.relationType.goToSettingsTreeItem('Relations');
  await umbracoUi.relationType.goToRelationTypeWithName('Related Document');
  await umbracoUi.relationType.isRelationWithParentAndChildVisible(contentName, contentToBePickedName, false);
});

test('can not see relation after media with relation is deleted', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Media Picker
  const mediaPickerName = 'Media Picker';
  const mediaPickerData = await umbracoApi.dataType.getByName(mediaPickerName);
  // Media
  const mediaName = 'TestMedia';
  const mediaFileId = await umbracoApi.media.createDefaultMediaFile(mediaName);
  // Document Type
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, mediaPickerName, mediaPickerData.id);
  // Content
  await umbracoApi.document.createDocumentWithOneMediaPicker(contentName, documentTypeId, mediaFileId);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.long);

  await umbracoUi.relationType.goToRelationTypeWithName('Related Media');
  await umbracoUi.relationType.isRelationWithParentAndChildVisible(contentName, mediaName);

  // Act
  await umbracoApi.media.ensureNameNotExists(mediaName);

  // Assert
  await umbracoUi.relationType.goToSettingsTreeItem('Relations');
  await umbracoUi.relationType.goToRelationTypeWithName('Related Media');
  await umbracoUi.relationType.isRelationWithParentAndChildVisible(contentName, mediaName, false);
});

test('can not see relation after member with relation is deleted', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // MemberPicker
  const memberPickerName = 'Member Picker';
  const memberPickerData = await umbracoApi.dataType.getByName(memberPickerName);
  // Member
  const memberTypeData = await umbracoApi.memberType.getByName('Member');
  const memberName = 'TestMember';
  const memberEmail = 'TestMemberEmail@test.com';
  const memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeData.id, memberEmail, memberEmail, memberEmail);
  // DocumentType
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, memberPickerName, memberPickerData.id);
  // Content
  await umbracoApi.document.createDocumentWithMemberPicker(contentName, documentTypeId, memberId);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.long);

  await umbracoUi.relationType.goToRelationTypeWithName('Related Member');
  await umbracoUi.relationType.isRelationWithParentAndChildVisible(contentName, memberName);

  // Act
  await umbracoApi.member.ensureNameNotExists(memberName);

  // Assert
  await umbracoUi.relationType.goToSettingsTreeItem('Relations');
  await umbracoUi.relationType.goToRelationTypeWithName('Related Member');
  await umbracoUi.relationType.isRelationWithParentAndChildVisible(contentName, memberName, false);
});
