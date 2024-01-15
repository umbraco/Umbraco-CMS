import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Partial View tests', () => {
  let partialViewPath = "";
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
    const partialView = await umbracoApi.partialView.get(partialViewPath);
    partialView.content = newContent;

    // Act
    await umbracoApi.partialView.update(partialView);

    // Assert
    const updatedPartialView = await umbracoApi.partialView.get(partialViewPath);
    expect(updatedPartialView.content).toEqual(newContent);
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