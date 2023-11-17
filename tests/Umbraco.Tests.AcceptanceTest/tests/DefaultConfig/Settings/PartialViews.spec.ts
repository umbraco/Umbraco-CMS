import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Partial Views tests', () => {
  const partialViewName = 'TestPartialView';
  const partialViewFileName = partialViewName + ".cshtml";
  
  test.beforeEach(async ({page, umbracoApi, umbracoUi}) => {
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
  });

  async function gotoPartialViewDetail(page, partialViewFileName) {
    await page.locator('umb-tree-item').filter({ hasText: 'Partial Views' }).locator('#caret-button').click();
    await page.locator('umb-tree-item').getByLabel(partialViewFileName).click();
  } 

  test('can create an empty partial view', async ({page, umbracoApi}) => {
    //Arrange
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewName);

    // Act
    await page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel('Open actions menu').click({force: true});
    await page.getByLabel('New empty partial view').click();
    await page.getByLabel('template name').fill(partialViewName);
    // TODO: Remove this timeout when frontend validation is implemented
    await page.waitForTimeout(1000);
    await page.getByLabel('Save').click();

    // Assert
    expect(await umbracoApi.partialView.nameExistsAtRoot(partialViewFileName)).toBeTruthy();
    // TODO: when frontend is ready, uncomment this to verify the new partial view is displayed under the Partial Views section 
    //await expect(page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel(partialViewFileName)).toBeVisible();
    // TODO: when frontend is ready, verify the notification displays
    
    // Clean
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
  });

  test('can create a partial view from snippet', async ({page, umbracoApi}) => {
    // Arrange
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
    const expectedTemplateContent = '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n@using Umbraco.Cms.Core.Routing\r\n@using Umbraco.Extensions\r\n\n@inject IPublishedUrlProvider PublishedUrlProvider\r\n@*\r\n    This snippet makes a breadcrumb of parents using an unordered HTML list.\r\n\r\n    How it works:\r\n    - It uses the Ancestors() method to get all parents and then generates links so the visitor can go back\r\n    - Finally it outputs the name of the current page (without a link)\r\n*@\r\n\r\n@{ var selection = Model.Ancestors().ToArray(); }\r\n\r\n@if (selection?.Length > 0)\r\n{\r\n    <ul class=\"breadcrumb\">\r\n        @* For each page in the ancestors collection which have been ordered by Level (so we start with the highest top node first) *@\r\n        @foreach (var item in selection.OrderBy(x => x.Level))\r\n        {\r\n            <li><a href=\"@item.Url(PublishedUrlProvider)\">@item.Name</a> <span class=\"divider\">/</span></li>\r\n        }\r\n\r\n        @* Display the current page as the last item in the list *@\r\n        <li class=\"active\">@Model.Name</li>\r\n    </ul>\r\n}';

    // Act
    await page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel('Open actions menu').click({force: true});
    await page.getByLabel('New partial view from snippet...').click();
    await page.getByLabel('Breadcrumb').click();
    await page.getByLabel('template name').fill(partialViewName);
    // TODO: Remove this timeout when frontend validation is implemented
    await page.waitForTimeout(1000);
    await page.getByLabel('Save').click();

    // Assert
    expect(await umbracoApi.partialView.nameExistsAtRoot(partialViewFileName)).toBeTruthy();
    const partialViewData = await umbracoApi.partialView.getByNameAtRoot(partialViewFileName);
    await expect(partialViewData.content).toBe(expectedTemplateContent);

    // TODO: when frontend is ready, uncomment this to verify the new partial view is displayed under the Partial Views section
    //await expect(page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel(partialViewFileName)).toBeVisible();
    // TODO: when frontend is ready, verify the notification displays
    
    // Clean
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
  });

  
test('can update a partial view name', async ({page, umbracoApi}) => {
    // Arrange
    const updatedPartialViewName = 'TestPartialViewUpdated';
    const updatedPartialViewFileName = updatedPartialViewName + ".cshtml";

    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");

    //Act
    await gotoPartialViewDetail(page, partialViewFileName);
    await page.getByLabel('template name').fill(updatedPartialViewName);
    // TODO: Remove this timeout when frontend validation is implemented
    await page.waitForTimeout(1000);
    await page.getByLabel('Save').click();

    // Assert
    expect(await umbracoApi.partialView.nameExistsAtRoot(updatedPartialViewFileName)).toBeTruthy();
    // TODO: when frontend is ready, uncomment this to verify the updated partial view name is displayed under the Partial Views section
    //await expect(page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel(updatedPartialViewFileName)).toBeVisible();
    expect(await umbracoApi.partialView.nameExistsAtRoot(partialViewFileName)).toBeFalsy();
    // TODO: when frontend is ready, uncomment this to verify the old partial view name is NOT displayed under the Partial Views section
    //await expect(page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel(partialViewFileName)).toHaveCount(0);
    
    // Clean
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(updatedPartialViewFileName);
  });

  test('can update a partial view content', async ({page, umbracoApi}) => {
    // Arrange
    const updatedPartialViewContent = '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n' +
      '@{\r\n' +
      '\tLayout = null;\r\n' +
      '}\r\n' +
      '<p>AcceptanceTests</p>';

    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");

    //Act
    await gotoPartialViewDetail(page, partialViewFileName);
    await page.locator('textarea.inputarea').clear();
    await page.locator('textarea.inputarea').fill(updatedPartialViewContent);
    // TODO: Remove this timeout when frontend validation is implemented
    await page.waitForTimeout(1000);
    await page.getByLabel('Save').click();

    // Assert
    const updatedPartialView = await umbracoApi.partialView.getByNameAtRoot(partialViewFileName);
    await expect(updatedPartialView.content).toBe(updatedPartialViewContent);
    
    // Clean
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
  });

  test('can use query builder for a partial view', async ({page, umbracoApi, umbracoUi}) => {
    //Arrange
    const expectedTemplateContent = '\r\n' +
    '@{\r\n' +
    '\tvar selection = Umbraco.ContentAtRoot().FirstOrDefault()\r\n' +
    '    .Children()\r\n' +
    '    .Where(x => x.IsVisible());\r\n' +
    '}\r\n' +
    '<ul>\r\n' +
    '\t@foreach (var item in selection)\r\n' +
    '\t{\r\n' +
    '\t\t<li>\r\n' +
    '\t\t\t<a href="@item.Url()">@item.Name()</a>\r\n' +
    '\t\t</li>\r\n' +
    '\t}\r\n' +
    '</ul>\r\n' +
    '\r\n' +
    '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n';

    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");

    // Act
    await gotoPartialViewDetail(page, partialViewFileName);
    await page.locator('#query-builder-button').getByLabel('Query builder').click({force:true});
    // TODO: Remove this timeout when frontend validation is implemented
    await page.waitForTimeout(1000);
    await expect(page.locator('uui-modal-container[backdrop]')).toBeTruthy();
    await page.locator('#property-alias-dropdown').getByLabel('Property alias').click({force:true});
    await expect(page.locator('uui-popover[open]')).toBeTruthy();
    await page.locator('#property-alias-dropdown').getByText('CreateDate').click();
    await page.getByLabel('Submit').click();
    await page.getByLabel('Save').click();

    // Assert
    const updatedPartialView = await umbracoApi.partialView.getByNameAtRoot(partialViewFileName);
    expect(updatedPartialView.content).toBe(expectedTemplateContent);

    // Clean
    await umbracoApi.template.ensureNameNotExists(partialViewFileName);
  });

  // Skip this test now as the function delete dictionary is missing
  test.skip('can insert dictionaryItem into a partial view', async ({page, umbracoApi}) => {
    // Arrange
    const dictionaryName = 'TestDictionary';
    
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");

    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
    await umbracoApi.dictionary.create(dictionaryName);

    const partialViewContent = '@Umbraco.GetDictionaryValue("TestDictionary")@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n';

    // Act
    await gotoPartialViewDetail(page, partialViewFileName);
    await page.getByLabel('Choose value to insert').click();
    await page.getByLabel('Insert Dictionary item').click({force: true});
    // TODO: Remove this timeout when frontend validation is implemented
    await page.waitForTimeout(1000);
    await page.locator('umb-tree-picker-modal').locator('#caret-button').click({force: true});
    await page.getByLabel(dictionaryName).click();
    await page.getByLabel('Submit').click();
    await page.getByLabel('Save').click();

    // Assert
    const partialViewData = await umbracoApi.partialView.getByNameAtRoot(partialViewFileName);
    expect(partialViewData.content).toBe(partialViewContent);

    // Clean
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
    await umbracoApi.template.ensureNameNotExists(partialViewFileName);
  });

  test('can delete a partial view', async ({page, umbracoApi, umbracoUi}) => {
    //Arrange
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");

    //Act
    await page.locator('umb-tree-item').filter({ hasText: 'Partial Views' }).locator('#caret-button').click();
    await page.locator('umb-tree-item').locator('[label="' + partialViewFileName + '"] >> [label="Open actions menu"]').click();
    await page.getByLabel('Delete').click();
    await page.locator('#confirm').getByLabel('Delete').click();

    // Assert
    expect(await umbracoApi.partialView.nameExistsAtRoot(partialViewFileName)).toBeFalsy();
    // TODO: when frontend is ready, uncomment this to verify the partial view is NOT displayed under the Partial Views section 
    //expect(page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel(partialViewFileName)).toHaveCount(0);
    // TODO: when frontend is ready, verify the notification displays

  });

  test('can create a folder', async ({page, umbracoApi}) => {
    //Arrange
    const folderName = 'TestFolder';
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(folderName);

    // Act
    await page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel('Open actions menu').click({force: true});
    await page.getByLabel('Create folder').click();
    await page.getByRole('textbox', { name: 'Enter folder name...' }).fill(folderName);
    await page.getByLabel('Create Folder', { exact: true }).click();

    // Assert
    expect(await umbracoApi.partialView.nameExistsAtRoot(folderName)).toBeTruthy();
    // TODO: when frontend is ready, uncomment this to verify the new folder is  displayed under the Partial Views section 
    //await expect(page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel(folderName)).toBeVisible();
    // TODO: when frontend is ready, verify the notification displays

    //Clean
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(folderName);
  });

  test.skip('can place a partial view into folder', async ({page, umbracoApi}) => {
    // TODO: implement this later as the frontend is missing now
  });
  
});
