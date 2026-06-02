import {AliasHelper, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'Test Content';
const documentTypeName = 'TestDocumentTypeForContent';
const templateName = 'TestTemplateForContent';
const customDataTypeName = 'Custom Approved Color';
const propertyName = 'Test Approved Color';
const colorValue = '9c2121';
const oldLabel = 'first';
const newLabel = 'third';
let contentId = null;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  const dataTypeId = await umbracoApi.dataType.createApprovedColorDataTypeWithOneItem(customDataTypeName, oldLabel, colorValue);
  const templateId = await umbracoApi.template.createTemplateWithDisplayingApprovedColorValue(templateName, AliasHelper.toAlias(propertyName));
  contentId = await umbracoApi.document.createPublishedDocumentWithValue(contentName, {label: oldLabel, value: '#' + colorValue}, dataTypeId, templateId, propertyName, documentTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can update the contents color label when the data type label changes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.updateApprovedColorItemLabel(customDataTypeName, colorValue, newLabel);
  const oldContentData = await umbracoApi.document.getByName(contentName);
  expect(oldContentData.values[0].value.label).toBe(oldLabel);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.label).toBe(newLabel);
  expect(contentData.values[0].value.value).toBe('#' + colorValue);
});

test('can render the updated color label after republishing', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.updateApprovedColorItemLabel(customDataTypeName, colorValue, newLabel);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeUpdated();
  const contentURL = await umbracoApi.document.getDocumentUrl(contentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(newLabel, true);
});
