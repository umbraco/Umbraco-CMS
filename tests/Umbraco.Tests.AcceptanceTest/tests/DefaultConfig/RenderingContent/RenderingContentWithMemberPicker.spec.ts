import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Member Picker';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Member Picker';
let dataTypeData = null;
const memberName = 'Test Member';
const memberTypeName = 'Test Member Type';
const username = 'testmember';
const email = 'testmember@acceptance.test';
const password = '0123456789';

test.beforeEach(async ({umbracoApi}) => {
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('can render content with member picker value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create member
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  const memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
  const memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeId, email, username, password);
  const templateId = await umbracoApi.template.createTemplateWithDisplayingMemberPickerValue(templateName, AliasHelper.toAlias(propertyName));
  await umbracoApi.document.createPublishedDocumentWithValue(contentName, memberId, dataTypeData.id, templateId, propertyName, documentTypeName);
  const contentData = await umbracoApi.document.getByName(contentName);
  const contentURL = contentData.urls[0].url;

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(memberName);
});