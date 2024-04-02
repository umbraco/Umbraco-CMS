import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Template tests', () => {
  const templateName = 'TestTemplate';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoApi.template.ensureNameNotExists(templateName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.template.goToSection(ConstantHelper.sections.settings);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.template.ensureNameNotExists(templateName);
  });

  test.skip('can create a template', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.template.clickActionsMenuAtRoot();
    await umbracoUi.template.clickNewTemplateButton();
    await umbracoUi.template.enterTemplateName(templateName);
    await umbracoUi.template.clickSaveButton();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    expect(await umbracoApi.template.doesNameExist(templateName)).toBeTruthy();
  });

  test('can update a template', async ({umbracoApi, umbracoUi}) => {
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
    await umbracoUi.template.goToTemplate(templateName);
    await umbracoUi.template.enterTemplateContent(updatedTemplateContent);
    await umbracoUi.template.clickSaveButton();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    // Checks if the template was updated
    const updatedTemplate = await umbracoApi.template.getByName(templateName);
    expect(updatedTemplate.content).toBe(updatedTemplateContent);
  });

  test('can delete a template', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const templateAlias = AliasHelper.toAlias(templateName);
    await umbracoApi.template.create(templateName, templateAlias, '');

    // Act
    await umbracoUi.template.clickRootFolderCaretButton();
    await umbracoUi.template.clickActionsMenuForTemplate(templateName);
    await umbracoUi.template.deleteTemplate();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    expect(await umbracoApi.template.doesNameExist(templateName)).toBeFalsy();
  });

  test.skip('can set a template as master template', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const templateAlias = AliasHelper.toAlias(templateName);
    const childTemplateName = 'ChildTemplate';
    const childTemplateAlias = AliasHelper.toAlias(childTemplateName);
    await umbracoApi.template.ensureNameNotExists(childTemplateName);
    await umbracoApi.template.create(templateName, templateAlias, '');
    await umbracoApi.template.create(childTemplateName, childTemplateAlias, '');

    // Act
    await umbracoUi.template.goToTemplate(childTemplateName);
    await umbracoUi.template.clickChangeMasterTemplateButton();
    await umbracoUi.template.clickCaretDictionaryButton();
    await umbracoUi.template.clickButtonWithName(templateName);
    await umbracoUi.template.clickSubmitButton();
    await umbracoUi.template.clickSaveButton();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    expect(await umbracoUi.template.isMasterTemplateNameVisible(templateName)).toBeTruthy();
    // Checks if the childTemplate has the masterTemplate set
    const childTemplate = await umbracoApi.template.getByName(childTemplateName);
    const masterTemplate = await umbracoApi.template.getByName(templateName);
    expect(childTemplate.masterTemplateId).toBe(masterTemplate.id);

    // Clean
    await umbracoApi.template.ensureNameNotExists(childTemplateName);
  });

  test.skip('can use query builder for a template', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const templateAlias = AliasHelper.toAlias(templateName);
    await umbracoApi.template.create(templateName, templateAlias, '');
    const expectedTemplateContent = '\r\n@{\r\n\tvar selection = Umbraco.ContentAtRoot().FirstOrDefault()\r\n    .Children()\r\n    .Where(x =\u003E x.IsVisible())\r\n    .OrderBy(x =\u003E x.CreateDate);\r\n}\r\n\u003Cul\u003E\r\n\t@foreach (var item in selection)\r\n\t{\r\n\t\t\u003Cli\u003E\r\n\t\t\t\u003Ca href=\u0022@item.Url()\u0022\u003E@item.Name()\u003C/a\u003E\r\n\t\t\u003C/li\u003E\r\n\t}\r\n\u003C/ul\u003E\r\n\r\n@using Umbraco.Cms.Web.Common.PublishedModels;\r\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n@{\r\n\tLayout = null;\r\n}';

    // Act
    await umbracoUi.template.goToTemplate(templateName);
    await umbracoUi.template.addQueryBuilderWithOrderByStatement('CreateDate', true);
    await umbracoUi.template.clickSubmitButton();
    await umbracoUi.template.clickSaveButton();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    const templateData = await umbracoApi.template.getByName(templateName);
    expect(templateData.content).toBe(expectedTemplateContent);
  });

  test('can insert sections into a template', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const templateAlias = AliasHelper.toAlias(templateName);
    await umbracoApi.template.create(templateName, templateAlias, '');
    const templateContent = '@RenderBody()@using Umbraco.Cms.Web.Common.PublishedModels;\r\n' +
      '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n' +
      '@{\r\n' +
      '\tLayout = null;\r\n' +
      '}';

    // Act
    await umbracoUi.template.goToTemplate(templateName);
    await umbracoUi.template.clickSectionsButton();
    await umbracoUi.template.clickSubmitButton();
    await umbracoUi.template.clickSaveButton();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    const templateData = await umbracoApi.template.getByName(templateName);
    expect(templateData.content).toBe(templateContent);
  });

  test('can insert dictionaryItem into a template', async ({umbracoApi, umbracoUi}) => {
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
    await umbracoUi.template.goToTemplate(templateName);
    await umbracoUi.template.insertDictionaryByName(dictionaryName);
    await umbracoUi.template.clickSaveButton();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    const templateData = await umbracoApi.template.getByName(templateName);
    expect(templateData.content).toBe(templateContent);

    // Clean
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  });
});
