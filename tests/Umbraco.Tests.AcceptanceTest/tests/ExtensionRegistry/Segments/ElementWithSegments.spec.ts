import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// Element
const elementName = 'TestSegmentElement';
// Element Type
const elementTypeName = 'TestSegmentElementType';
let elementTypeId = '';
// DataType
const dataTypeName = 'Textstring';
// Segments
const vipMemberSegment = 'VIP members';
const vipMemberSegmentAlias = 'vip-members';
// Test Values
const defaultSegmentValue = 'Default segment value';
const vipSegmentValue = 'VIP segment value';
const updatedVipSegmentValue = 'Updated VIP segment value';

test.describe('Library elements with segment-only variations', () => {
  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.element.ensureNameNotExists(elementName);
    await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryBySegment(elementTypeName, 'TestGroup', dataTypeName, dataTypeData.id);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.element.ensureNameNotExists(elementName);
    await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  });

  test('can add VIP segment value to a Library element', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, defaultSegmentValue, dataTypeName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.library.goToSection(ConstantHelper.sections.library);
    await umbracoUi.library.goToElementWithName(elementName);

    // Act
    await umbracoUi.library.clickSelectVariantButton();
    await umbracoUi.library.clickSegmentVariantButton(vipMemberSegment);
    await umbracoUi.library.enterTextstring(vipSegmentValue);
    await umbracoUi.library.clickSaveButtonAndWaitForElementToBeUpdated();

    // Assert
    const [defaultValue, vipValue] = await umbracoApi.element.getValuesByCultureAndSegmentForElement(elementName, [
      {culture: null, segment: null},
      {culture: null, segment: vipMemberSegmentAlias},
    ]);
    expect(defaultValue).toBeTruthy();
    expect(defaultValue.value).toBe(defaultSegmentValue);
    expect(vipValue).toBeTruthy();
    expect(vipValue.value).toBe(vipSegmentValue);
  });

  test('can publish updated VIP segment value on a Library element', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const elementId = await umbracoApi.element.createElementWithMultipleSegmentValues(elementName, elementTypeId, dataTypeName, [
      {value: defaultSegmentValue, segment: null},
      {value: vipSegmentValue, segment: vipMemberSegmentAlias},
    ]);
    await umbracoApi.element.publish(elementId);
    expect(await umbracoApi.element.isElementPublished(elementId)).toBeTruthy();
    await umbracoUi.goToBackOffice();
    await umbracoUi.library.goToSection(ConstantHelper.sections.library);
    await umbracoUi.library.goToElementWithName(elementName);
    await umbracoUi.library.clickSelectVariantButton();
    await umbracoUi.library.clickSegmentVariantButton(vipMemberSegment);

    // Act
    await umbracoUi.library.enterTextstring(updatedVipSegmentValue);
    await umbracoUi.library.clickSaveAndPublishButtonAndWaitForElementToBePublished();

    // Assert
    const [publishedDefault, publishedVip] = await umbracoApi.element.getPublishedValuesByCultureAndSegmentForElement(elementId, [
      {culture: null, segment: null},
      {culture: null, segment: vipMemberSegmentAlias},
    ]);
    expect(publishedDefault).toBeTruthy();
    expect(publishedDefault.value).toBe(defaultSegmentValue);
    expect(publishedVip).toBeTruthy();
    expect(publishedVip.value).toBe(updatedVipSegmentValue);
  });
});
