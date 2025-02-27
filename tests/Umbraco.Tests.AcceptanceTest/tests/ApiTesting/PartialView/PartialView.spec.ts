import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Partial View tests', () => {
  let partialViewPath = '';
  const partialViewName = 'partialViewName.cshtml';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.partialView.ensureNameNotExists(partialViewName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.partialView.ensureNameNotExists(partialViewName);
  });

  test('can create a partial view', async ({umbracoApi}) => {
    // Act
    partialViewPath = await umbracoApi.partialView.create(partialViewName, 'test');

    // Assert
    expect(await umbracoApi.partialView.doesExist(partialViewPath)).toBeTruthy();
  });

  test('can update a partial view', async ({umbracoApi}) => {
    // Arrange
    const newContent = 'Howdy';
    partialViewPath = await umbracoApi.partialView.create(partialViewName, 'test');

    // Act
    await umbracoApi.partialView.updateContent(partialViewPath, newContent);

    // Assert
    const updatedPartialView = await umbracoApi.partialView.get(partialViewPath);
    expect(updatedPartialView.content).toEqual(newContent);
  });

  test('can rename a partial view', async ({umbracoApi}) => {
    // Arrange
    const wrongPartialViewName = 'wrongPartialViewName.cshtml';
    const wrongPartialViewPath = await umbracoApi.partialView.create(wrongPartialViewName, 'test');

    // Act
    const updatedPartialViewPath = await umbracoApi.partialView.updateName(wrongPartialViewPath, partialViewName);

    // Assert
    const updatedPartialView = await umbracoApi.partialView.get(updatedPartialViewPath);
    expect(updatedPartialView.name).toEqual(partialViewName);
  });

  test('can delete a partial view', async ({umbracoApi}) => {
    // Arrange
    partialViewPath = await umbracoApi.partialView.create(partialViewName, 'test');
    expect(await umbracoApi.partialView.doesExist(partialViewPath)).toBeTruthy();

    // Act
    await umbracoApi.partialView.delete(partialViewPath);

    // Assert
    expect(await umbracoApi.partialView.doesExist(partialViewPath)).toBeFalsy();
  });
});