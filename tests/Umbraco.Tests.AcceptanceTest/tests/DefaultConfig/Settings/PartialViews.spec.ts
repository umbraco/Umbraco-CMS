import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Partial Views tests', () => {
  
  test.beforeEach(async ({page, umbracoApi, umbracoUi}) => {
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
  });

  async function gotoPartialViewDetail(page, partialViewFileName) {
    await page.locator('umb-tree-item').filter({ hasText: 'Partial Views' }).locator('#caret-button').click();
    await page.locator('umb-tree-item').getByLabel(partialViewFileName).click();
  }
  

  test('can create an empty partial view', async ({page, umbracoApi}) => {
    const partialViewName = 'TestPartialView';
    const partialViewFileName = partialViewName + ".cshtml";
    // Act
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewName);

    await page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel('Open actions menu').click({force: true});
    await page.getByLabel('New empty partial view').click();
    await page.getByLabel('template name').fill(partialViewName);
    await page.getByLabel('Save').click();

    // Assert
    expect(await umbracoApi.partialView.nameExistsAtRoot(partialViewFileName)).toBeTruthy;
    // TODO: when frontend is ready, uncomment this to verify the new partial view is displayed under the Partial Views section 
    //await expect(page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel(partialViewFileName)).toBeVisible();
    // TODO: when frontend is ready, verify the notification displays
    
    // Clean
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
  });

  test('can create a partial view from snippet', async ({page, umbracoApi}) => {
    const partialViewName = 'TestPartialViewFromSnippet';
    const partialViewFileName = partialViewName + ".cshtml";
    // Act
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);

    await page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel('Open actions menu').click({force: true});
    await page.getByLabel('New partial view from snippet...').click();
    await page.getByLabel('Breadcrumb').click();
    await page.getByLabel('template name').fill(partialViewName);
    await page.getByLabel('Save').click();
    

    // Assert
    expect(await umbracoApi.partialView.nameExistsAtRoot(partialViewFileName)).toBeTruthy;
    // TODO: when frontend is ready, uncomment this to verify the new partial view is displayed under the Partial Views section
    //await expect(page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel(partialViewFileName)).toBeVisible();
    // TODO: when frontend is ready, verify the notification displays
    
    // Clean
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
  });

  
test('can update a partial view name', async ({page, umbracoApi}) => {
    const partialViewName = 'TestPartialView';
    const partialViewFileName = partialViewName + ".cshtml";
    const updatedPartialViewName = 'TestPartialViewUpdated';
    const updatedPartialViewFileName = updatedPartialViewName + ".cshtml";
    // Arrange
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");

    //Act
    gotoPartialViewDetail(page, partialViewFileName);
    await page.getByLabel('template name').fill(updatedPartialViewName);
    await page.getByLabel('Save').click();

    // Assert
    expect(await umbracoApi.partialView.nameExistsAtRoot(updatedPartialViewFileName)).toBeTruthy;
    // TODO: when frontend is ready, uncomment this to verify the updated partial view name is displayed under the Partial Views section
    //await expect(page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel(updatedPartialViewFileName)).toBeVisible();
    expect(await umbracoApi.partialView.nameExistsAtRoot(partialViewFileName)).toBeFalsy;
    // TODO: when frontend is ready, uncomment this to verify the old partial view name is NOT displayed under the Partial Views section
    //await expect(page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel(partialViewFileName)).toHaveCount(0);
    
    // Clean
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(updatedPartialViewFileName);
  });

  test('can update a partial view content', async ({page, umbracoApi}) => {
    const partialViewName = 'TestPartialView';
    const partialViewFileName = partialViewName + ".cshtml";
    const updatedPartialViewContent = '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n' +
      '@{\r\n' +
      '\tLayout = null;\r\n' +
      '}\r\n' +
      '<p>AcceptanceTests</p>';

    // Arrange
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");

    //Act
    gotoPartialViewDetail(page, partialViewFileName);
    await page.locator('textarea.inputarea').clear();
    await page.locator('textarea.inputarea').fill(updatedPartialViewContent);
    await page.getByLabel('Save').click();

    // Assert
    const updatedPartialView = await umbracoApi.partialView.getByNameAtRoot(partialViewFileName);
    expect(updatedPartialView.content).toBe(updatedPartialViewContent);
    
    // Clean
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
  });

  test('can use query builder for a partial view', async ({page, umbracoApi, umbracoUi}) => {
    const partialViewName = 'TestPartialView';
    const partialViewFileName = partialViewName + ".cshtml";
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


    // Arrange
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");

    // Act
    gotoPartialViewDetail(page, partialViewFileName);
    await page.locator('#query-builder-button').getByLabel('Query builder').click();
    await page.waitForTimeout(1000);
    await page.locator('#property-alias-dropdown').getByLabel('Property alias').click({force: true});
    await page.waitForTimeout(1000);
    await page.locator('#property-alias-dropdown').getByText('CreateDate').click({force:true});
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
    const partialViewName = 'TestPartialView';
    const partialViewFileName = partialViewName + ".cshtml";
    const dictionaryName = 'TestDictionary';
    // Arrange
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(partialViewFileName);
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");

    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
    await umbracoApi.dictionary.create(dictionaryName);

    const partialViewContent = '@Umbraco.GetDictionaryValue("TestDictionary")@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n';

    // Act
    gotoPartialViewDetail(page, partialViewFileName);
    await page.getByLabel('Choose value to insert').click();
    await page.getByLabel('Insert Dictionary item').click({force: true});
    // We need to wait for the modal to load before clicking the button
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
    const partialViewName = 'TestPartialView';
    const partialViewFileName = partialViewName + ".cshtml";
    // Arrange
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
    const folderName = 'TestFolder';
    // Act
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(folderName);
    await page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel('Open actions menu').click({force: true});
    await page.getByLabel('Create folder').click();
    await page.getByRole('textbox', { name: 'Enter folder name...' }).fill('TestFolder');
    await page.getByLabel('Create Folder', { exact: true }).click();

    // Assert
    expect(await umbracoApi.partialView.nameExistsAtRoot(folderName)).toBeTruthy();
    // TODO: when frontend is ready, uncomment this to verify the new folder is  displayed under the Partial Views section 
    //await expect(page.locator('umb-tree-item', {hasText: 'Partial Views'}).getByLabel(folderName)).toBeVisible();
    // TODO: when frontend is ready, verify the notification displays

    //Clean
    await umbracoApi.partialView.ensureNameNotExistsAtRoot(folderName);
  });

  test('can place a partial view into folder', async ({page, umbracoApi}) => {
    // TODO: implement this later as the frontend is missing now
  });
  
});
