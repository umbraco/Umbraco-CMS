import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Template tests', () => {
  const templateName = 'TestTemplate';

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.template.ensureNameNotExists(templateName);
  });

  test('can create a template', async ({page, umbracoApi, umbracoUi}) => {
    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('umb-tree-item', {hasText: 'Templates'}).getByLabel('Open actions menu').click({force: true});
    await page.getByLabel('Create').click();
    await page.getByLabel('template name').fill(templateName);
    // TODO: Remove this timeout when frontend validation is implemented
    await page.waitForTimeout(1000);
    await page.getByLabel('Save').click();

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    expect(await umbracoApi.template.doesNameExist(templateName)).toBeTruthy();

    // Clean
    await umbracoApi.template.ensureNameNotExists(templateName);
  });

  test('can update a template', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const templateAlias = AliasHelper.toAlias(templateName);
    const updatedTemplateContent = '@using Umbraco.Cms.Web.Common.PublishedModels;\r\n' +
      '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n' +
      '@{\r\n' +
      '\tLayout = null;\r\n' +
      '}\r\n' +
      '<p>AcceptanceTests</p>';

    await umbracoApi.template.create(templateName, templateAlias, '');

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToTemplate(templateName)
    await page.locator('textarea.inputarea').clear();
    await page.locator('textarea.inputarea').fill(updatedTemplateContent);
    await page.getByLabel('Save').click();

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the template was updated
    const updatedTemplate = await umbracoApi.template.getByName(templateName);
    expect(updatedTemplate.content).toBe(updatedTemplateContent);

    // Clean
    await umbracoApi.template.ensureNameNotExists(templateName);
  });

  test('can delete a template', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const templateAlias = AliasHelper.toAlias(templateName);
    await umbracoApi.template.create(templateName, templateAlias, '');

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await page.locator('umb-tree-item', {hasText: 'Templates'}).locator('#caret-button').click();
    await page.locator('umb-tree-item').locator('[label="' + templateName + '"] >> [label="Open actions menu"]').click();
    await page.getByLabel('Delete').click();
    await page.locator('#confirm').getByLabel('Delete').click();

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    expect(await umbracoApi.template.doesNameExist(templateName)).toBeFalsy();
  });

  test('can set a template as master template', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const templateAlias = AliasHelper.toAlias(templateName);
    const childTemplateName = 'ChildTemplate';
    const childTemplateAlias = AliasHelper.toAlias(childTemplateName);
    await umbracoApi.template.ensureNameNotExists(childTemplateName);
    await umbracoApi.template.create(templateName, templateAlias, '');
    await umbracoApi.template.create(childTemplateName, childTemplateAlias, '');

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToTemplate(childTemplateName);
    await page.getByLabel('Change Master template').click();
    await page.locator('umb-tree-picker-modal').locator('#caret-button').click();
    await page.getByRole('button', {name: templateName}).click();
    await page.getByLabel('Submit').click();
    await page.getByLabel('Save').click();

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    await expect(page.locator('[label="Change Master template"]', {hasText: 'Master template: ' + templateName})).toBeVisible();
    // Checks if the childTemplate has the masterTemplate set
    const childTemplate = await umbracoApi.template.getByName(childTemplateName);
    const masterTemplate = await umbracoApi.template.getByName(templateName);
    expect(childTemplate.masterTemplateId).toBe(masterTemplate.id);

    // Clean
    await umbracoApi.template.ensureNameNotExists(templateName);
    await umbracoApi.template.ensureNameNotExists(childTemplateName);
  });

  test('can use query builder for a template', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const templateAlias = AliasHelper.toAlias(templateName);
    await umbracoApi.template.create(templateName, templateAlias, '');
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
      '@using Umbraco.Cms.Web.Common.PublishedModels;\r\n' +
      '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n' +
      '@{\r\n' +
      '\tLayout = null;\r\n' +
      '}';

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToTemplate(templateName);
    await page.locator('#query-builder-button').getByLabel('Query builder').click();
    await page.waitForTimeout(1000);
    await page.locator('#property-alias-dropdown').getByLabel('Property alias').click({force: true});
    await page.waitForTimeout(1000);
    await page.locator('#property-alias-dropdown').getByText('CreateDate').click({force:true});
    await page.getByLabel('Submit').click();
    await page.getByLabel('Save').click();

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    const templateData = await umbracoApi.template.getByName(templateName);
    expect(templateData.content).toBe(expectedTemplateContent);

    // Clean
    await umbracoApi.template.ensureNameNotExists(templateName);
  });

  test('can insert sections into a template', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const templateAlias = AliasHelper.toAlias(templateName);
    await umbracoApi.template.create(templateName, templateAlias, '');
    const templateContent = '@RenderBody()@using Umbraco.Cms.Web.Common.PublishedModels;\r\n' +
      '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n' +
      '@{\r\n' +
      '\tLayout = null;\r\n' +
      '}';

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToTemplate(templateName);
    await page.locator('#sections-button', {hasText: 'Sections'}).click();
    await page.getByLabel('Submit').click();
    await page.getByLabel('Save').click();

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    const templateData = await umbracoApi.template.getByName(templateName);
    expect(templateData.content).toBe(templateContent);

    // Clean
    await umbracoApi.template.ensureNameNotExists(templateName);
  });

  test('can insert dictionaryItem into a template', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const templateAlias = AliasHelper.toAlias(templateName);
    await umbracoApi.template.create(templateName, templateAlias, '');

    const dictionaryName = 'TestDictionary';
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
    await umbracoApi.dictionary.create(dictionaryName);

    const templateContent = '@Umbraco.GetDictionaryValue("TestDictionary")@using Umbraco.Cms.Web.Common.PublishedModels;\r\n' +
      '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n' +
      '@{\r\n' +
      '\tLayout = null;\r\n' +
      '}';

    // Act
    await page.goto(umbracoApi.baseUrl + '/umbraco');
    await umbracoUi.goToTemplate(templateName);
    await page.getByLabel('Choose value to insert').click();
    await page.getByLabel('Insert Dictionary item').click({force: true});
    // We need to wait for the modal to load before clicking the button
    await page.waitForTimeout(1000);
    await page.locator('umb-tree-picker-modal').locator('#caret-button').click({force: true});
    await page.getByLabel(dictionaryName).click();
    await page.getByLabel('Submit').click();
    await page.getByLabel('Save').click();

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    const templateData = await umbracoApi.template.getByName(templateName);
    expect(templateData.content).toBe(templateContent);

    // Clean
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
    await umbracoApi.template.ensureNameNotExists(templateName);
  });
});
