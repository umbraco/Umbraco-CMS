import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Partial View tests', () => {
  const partialViewName = 'partialViewName.cshtml';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.partialView.ensurePartialViewNameNotExistsAtRoot(partialViewName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.partialView.ensurePartialViewNameNotExistsAtRoot(partialViewName);
  });

  test('can create partial view', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.partialView.createPartialView(partialViewName, 'test');

    // Assert
    await expect(await umbracoApi.partialView.doesPartialViewWithNameExistAtRoot(partialViewName)).toBeTruthy();
  });

  test('can update partial view', async ({page, umbracoApi, umbracoUi}) => {
    const newContent = 'Howdy';

    await umbracoApi.partialView.createPartialView(partialViewName, 'test');

    const partialView = await umbracoApi.partialView.getPartialViewByNameAtRoot(partialViewName);

    partialView.content = newContent;

    await umbracoApi.partialView.updatePartialView(partialView);

    // Assert
    const updatedPartialView = await umbracoApi.partialView.getPartialViewByPath(partialView.path);
    await expect(updatedPartialView.content === newContent).toBeTruthy();
  });

  test('can delete partial view', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.partialView.createPartialView(partialViewName, 'test');

    await expect(await umbracoApi.partialView.doesPartialViewWithNameExistAtRoot(partialViewName)).toBeTruthy();

    const partialView = await umbracoApi.partialView.getPartialViewByNameAtRoot(partialViewName);

    await umbracoApi.partialView.deletePartialViewByPath(partialView.path);

    // Assert
    await expect(await umbracoApi.partialView.doesPartialViewWithNameExistAtRoot(partialViewName)).toBeFalsy();
  });
});
