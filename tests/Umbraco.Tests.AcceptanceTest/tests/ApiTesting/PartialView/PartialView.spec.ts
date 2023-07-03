import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Partial View tests', () => {
  let partialViewPath = "";
  const partialViewName = 'partialViewName.cshtml';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.partialView.delete(partialViewPath);
  });

  test('can create a partial view', async ({page, umbracoApi, umbracoUi}) => {
    partialViewPath = await umbracoApi.partialView.create(partialViewName, 'test');

    // Assert
    await expect(await umbracoApi.partialView.exists(partialViewPath)).toBeTruthy();
  });

  test('can update a partial view', async ({page, umbracoApi, umbracoUi}) => {
    const newContent = 'Howdy';

    partialViewPath = await umbracoApi.partialView.create(partialViewName, 'test');

    const partialView = await umbracoApi.partialView.get(partialViewPath);

    partialView.content = newContent;

    await umbracoApi.partialView.update(partialView);

    // Assert
    const updatedPartialView = await umbracoApi.partialView.get(partialViewPath);
    await expect(updatedPartialView.content).toEqual(newContent);
  });

  test('can delete a partial view', async ({page, umbracoApi, umbracoUi}) => {
    partialViewPath = await umbracoApi.partialView.create(partialViewName, 'test');

    await expect(await umbracoApi.partialView.exists(partialViewPath)).toBeTruthy();

    await umbracoApi.partialView.delete(partialViewPath);

    // Assert
    await expect(await umbracoApi.partialView.exists(partialViewPath)).toBeFalsy();
  });
});
