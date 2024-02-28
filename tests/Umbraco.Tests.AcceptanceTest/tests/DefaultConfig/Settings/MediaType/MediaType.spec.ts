import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.describe('Media Type tests', () => {

  const mediaTypeName = 'TestMediaType';
  const dataTypeName = 'Approved Color';
  const groupName = 'TestGroup';
  const tabName = 'TestTab';



  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoApi.dataType.ensureNameNotExists(mediaTypeName)
    await umbracoUi.goToBackOffice();
    await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);

  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName)


  });

  // Design
  // Name and alias is removed when saving
  test.skip('can create an empty media type', async ({page,umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.mediaType.clickActionsMenuForName('Media Types');
    await umbracoUi.mediaType.clickCreateThreeDotsButton();
    await umbracoUi.mediaType.clickNewMediaTypeButton();

    await umbracoUi.mediaType.enterMediaTypeName(mediaTypeName);

    await umbracoUi.mediaType.clickSaveButton();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
  });

  test('can create a media type with a single property', async ({page,umbracoApi, umbracoUi}) => {
    await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);


    await umbracoUi.mediaType.goToMediaType(mediaTypeName);
    await page.pause();
    await umbracoUi.mediaType.clickAddGroupButton();
    await umbracoUi.mediaType.addPropertyEditor(dataTypeName);
    await umbracoUi.mediaType.enterMediaTypeGroupName(groupName);
    await umbracoUi.mediaType.clickSaveButton();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
    const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
    const dataType = await umbracoApi.dataType.getByName(dataTypeName);
    // Checks if the correct property was added to the document type
    expect(mediaTypeData.properties[0].dataType.id).toBe(dataType.id);
  });

  test('can rename a media type', async ({page,umbracoApi, umbracoUi}) => {
  });

  test('can update a property in a media type', async ({page,umbracoApi, umbracoUi}) => {
  });

  test('can delete a media type', async ({page,umbracoApi, umbracoUi}) => {
  });

  test('can delete a property in a media type', async ({page,umbracoApi, umbracoUi}) => {
  });

  test('can delete a group in a media type', async ({page,umbracoApi, umbracoUi}) => {
  });

  test('can create a media type with multiple properties', async ({page,umbracoApi, umbracoUi}) => {
  });

  test('can create a media type with a single tab', async ({page,umbracoApi, umbracoUi}) => {
  });

  test('can create a media type with multiple tabs', async ({page,umbracoApi, umbracoUi}) => {
  });

  test('can create a media type with multiple groups', async ({page,umbracoApi, umbracoUi}) => {
  });

  test('can add a composition to a media type', async ({page,umbracoApi, umbracoUi}) => {
  });

  test('can reorder properties in a media type', async ({page,umbracoApi, umbracoUi}) => {
  });

  test('can reorder groups in a media type', async ({page,umbracoApi, umbracoUi}) => {
  });



// Structure

});
