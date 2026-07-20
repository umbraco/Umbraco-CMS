import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const mediaTypeName = 'TestMediaType';
const compositionMediaTypeName = 'CompositionMediaType';
const secondCompositionMediaTypeName = 'SecondCompositionMediaType';
const dataTypeName = 'Textstring';
const secondDataTypeName = 'Numeric';
const groupName = 'TestGroup';
const secondGroupName = 'SecondGroup';
let dataTypeData = null;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
  await umbracoApi.mediaType.ensureNameNotExists(compositionMediaTypeName);
  await umbracoApi.mediaType.ensureNameNotExists(secondCompositionMediaTypeName);
  await umbracoUi.goToBackOffice();
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
  await umbracoApi.mediaType.ensureNameNotExists(compositionMediaTypeName);
  await umbracoApi.mediaType.ensureNameNotExists(secondCompositionMediaTypeName);
});

test('can add a composition to a media type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const compositionMediaTypeId = await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(compositionMediaTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);
  await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  await umbracoUi.mediaType.clickCompositionsButton();
  await umbracoUi.mediaType.clickModalMenuItemWithName(compositionMediaTypeName);
  await umbracoUi.mediaType.clickSubmitButton();
  await umbracoUi.mediaType.clickSaveButtonAndWaitForMediaTypeToBeUpdated();

  // Assert
  await umbracoUi.mediaType.isInheritedGroupVisible(groupName, compositionMediaTypeName);
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.compositions.length).toBe(1);
  expect(mediaTypeData.compositions[0].mediaType.id).toBe(compositionMediaTypeId);
});

test('can remove a composition from a media type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const compositionMediaTypeId = await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(compositionMediaTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoApi.mediaType.createMediaTypeWithAComposition(mediaTypeName, compositionMediaTypeId);
  await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  await umbracoUi.mediaType.clickCompositionsButton();
  await umbracoUi.mediaType.clickModalMenuItemWithName(compositionMediaTypeName);
  await umbracoUi.mediaType.clickSubmitButton();
  await umbracoUi.mediaType.clickConfirmToSubmitButton();
  await umbracoUi.mediaType.clickSaveButtonAndWaitForMediaTypeToBeUpdated();

  // Assert
  await umbracoUi.mediaType.isGroupVisible(groupName, false);
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.compositions).toEqual([]);
});

test('can add multiple compositions to a media type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondDataTypeData = await umbracoApi.dataType.getByName(secondDataTypeName);
  const compositionMediaTypeId = await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(compositionMediaTypeName, dataTypeName, dataTypeData.id, groupName);
  const secondCompositionMediaTypeId = await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(secondCompositionMediaTypeName, secondDataTypeName, secondDataTypeData.id, secondGroupName);
  await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);
  await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  await umbracoUi.mediaType.clickCompositionsButton();
  await umbracoUi.mediaType.clickModalMenuItemWithName(compositionMediaTypeName);
  await umbracoUi.mediaType.clickModalMenuItemWithName(secondCompositionMediaTypeName);
  await umbracoUi.mediaType.clickSubmitButton();
  await umbracoUi.mediaType.clickSaveButtonAndWaitForMediaTypeToBeUpdated();

  // Assert
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.compositions.length).toBe(2);
  const compositionIds = mediaTypeData.compositions.map(c => c.mediaType.id);
  expect(compositionIds).toContain(compositionMediaTypeId);
  expect(compositionIds).toContain(secondCompositionMediaTypeId);
});
