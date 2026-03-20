import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const memberTypeName = 'TestMemberType';
const compositionMemberTypeName = 'CompositionMemberType';
const dataTypeName = 'Textstring';
const groupName = 'TestGroup';
let compositionMemberTypeId = null;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  await umbracoApi.memberType.ensureNameNotExists(compositionMemberTypeName);
  await umbracoUi.goToBackOffice();
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  compositionMemberTypeId = await umbracoApi.memberType.createMemberTypeWithPropertyEditor(compositionMemberTypeName, dataTypeName, dataTypeData.id, groupName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  await umbracoApi.memberType.ensureNameNotExists(compositionMemberTypeName);
});

test('can add a composition to a member type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
  await umbracoUi.memberType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.memberType.goToMemberType(memberTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.memberType.clickCompositionsButton();
  await umbracoUi.memberType.clickModalMenuItemWithName(compositionMemberTypeName);
  await umbracoUi.memberType.clickSubmitButton();
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  expect(memberTypeData.compositions.length).toBe(1);
  expect(memberTypeData.compositions[0].memberType.id).toBe(compositionMemberTypeId);
});

test('can remove a composition from a member type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.memberType.createMemberTypeWithAComposition(memberTypeName, compositionMemberTypeId);
  await umbracoUi.memberType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.memberType.goToMemberType(memberTypeName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  await umbracoUi.memberType.clickCompositionsButton();
  await umbracoUi.memberType.clickModalMenuItemWithName(compositionMemberTypeName);
  await umbracoUi.memberType.clickSubmitButton();
  await umbracoUi.memberType.clickConfirmToSubmitButton();
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  expect(memberTypeData.compositions).toEqual([]);
});
