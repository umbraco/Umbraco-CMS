import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Template tests', () => {
  const templateName = 'TestTemplate';
  const defaultTemplateContent = '@using Umbraco.Cms.Web.Common.PublishedModels;\r\n@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n@{\r\n\tLayout = null;\r\n}';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoApi.template.ensureNameNotExists(templateName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.template.goToSection(ConstantHelper.sections.settings);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.template.ensureNameNotExists(templateName);
  });

  test('can create a template', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.template.clickActionsMenuAtRoot();
    await umbracoUi.template.clickCreateButton();
    await umbracoUi.template.enterTemplateName(templateName);
    await umbracoUi.template.clickSaveButton();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    expect(await umbracoApi.template.doesNameExist(templateName)).toBeTruthy();
  });

  test('can update content of a template', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const updatedTemplateContent =
      defaultTemplateContent + '\r\n' + '<p>AcceptanceTests</p>';

    await umbracoApi.template.createDefaultTemplate(templateName);

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
    await umbracoApi.template.createDefaultTemplate(templateName);

    // Act
    await umbracoUi.template.clickRootFolderCaretButton();
    await umbracoUi.template.clickActionsMenuForTemplate(templateName);
    await umbracoUi.template.deleteTemplate();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    expect(await umbracoApi.template.doesNameExist(templateName)).toBeFalsy();
  });

  test('can set a template as master template', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const childTemplateName = 'ChildTemplate';
    await umbracoApi.template.ensureNameNotExists(childTemplateName);
    await umbracoApi.template.createDefaultTemplate(templateName);
    await umbracoApi.template.createDefaultTemplate(childTemplateName);

    // Act
    await umbracoUi.template.goToTemplate(childTemplateName);
    await umbracoUi.template.clickChangeMasterTemplateButton();
    await umbracoUi.template.clickButtonWithName(templateName);
    await umbracoUi.template.clickChooseButton();
    await umbracoUi.template.clickSaveButton();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    await umbracoUi.template.isMasterTemplateNameVisible(templateName);
    // Checks if the childTemplate has the masterTemplate set
    const childTemplateData = await umbracoApi.template.getByName(childTemplateName);
    const masterTemplateData = await umbracoApi.template.getByName(templateName);
    expect(childTemplateData.masterTemplate.id).toBe(masterTemplateData.id);

    // Clean
    await umbracoApi.template.ensureNameNotExists(childTemplateName);
  });

  test('can remove a master template', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const childTemplateName = 'ChildTemplate';
    const templateAlias = AliasHelper.toAlias(templateName);
    const childTemplateAlias = AliasHelper.toAlias(childTemplateName);
    const childTemplateContent = '@{\n\tLayout = \"' + templateAlias + '.cshtml\";\n}\n';
    await umbracoApi.template.ensureNameNotExists(childTemplateName);
    await umbracoApi.template.create(templateName, templateAlias, '');
    await umbracoApi.template.create(childTemplateName, childTemplateAlias, childTemplateContent);

    // Act
    await umbracoUi.template.goToTemplate(templateName, childTemplateName);
    await umbracoUi.template.clickRemoveMasterTemplateButton();
    await umbracoUi.template.clickSaveButton();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    await umbracoUi.template.isMasterTemplateNameVisible('No master');
    const childTemplate = await umbracoApi.template.getByName(childTemplateName);
    expect(childTemplate.masterTemplate).toBe(null);

    // Clean
    await umbracoApi.template.ensureNameNotExists(childTemplateName);
  });

  test('can use query builder with Order By statement for a template', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const propertyAliasValue = 'UpdateDate';
    const isAscending = true;
    const expectedCode = 'Umbraco.ContentAtRoot().FirstOrDefault()\r\n' +
    '    .Children()\r\n' +
    '    .Where(x => x.IsVisible())\r\n' +
    '    .OrderBy(x => x.' + propertyAliasValue + ')';
    const expectedTemplateContent = '\r\n' +
      '@{\r\n' +
      '\tvar selection = ' + expectedCode + ';\r\n' +
      '}\r\n' +
      '<ul>\r\n' +
      '\t@foreach (var item in selection)\r\n' +
      '\t{\r\n' +
      '\t\t<li>\r\n' +
      '\t\t\t<a href="@item.Url()">@item.Name()</a>\r\n' +
      '\t\t</li>\r\n' +
      '\t}\r\n' +
      '</ul>\r\n' +
      '\r\n' + defaultTemplateContent;

      await umbracoApi.template.createDefaultTemplate(templateName);

    // Act
    await umbracoUi.template.goToTemplate(templateName);
    await umbracoUi.template.addQueryBuilderWithOrderByStatement(propertyAliasValue, isAscending);
    // Verify that the code is shown
    await umbracoUi.template.isQueryBuilderCodeShown(expectedCode);
    await umbracoUi.template.clickSubmitButton();
    await umbracoUi.template.clickSaveButton();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    const templateData = await umbracoApi.template.getByName(templateName);
    expect(templateData.content).toBe(expectedTemplateContent);
  });

  test('can use query builder with Where statement for a template', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    //Arrange
    const propertyAliasValue = 'Name';
    const operatorValue = 'is';
    const constrainValue = 'Test Content';
    const expectedCode = 'Umbraco.ContentAtRoot().FirstOrDefault()\r\n' +
    '    .Children()\r\n' +
    '    .Where(x => (x.' + propertyAliasValue + ' == "' + constrainValue + '"))\r\n' +
    '    .Where(x => x.IsVisible())';
    const expectedTemplateContent = '\r\n' +
      '@{\r\n' +
      '\tvar selection = ' + expectedCode + ';\r\n' +
      '}\r\n' +
      '<ul>\r\n' +
      '\t@foreach (var item in selection)\r\n' +
      '\t{\r\n' +
      '\t\t<li>\r\n' +
      '\t\t\t<a href="@item.Url()">@item.Name()</a>\r\n' +
      '\t\t</li>\r\n' +
      '\t}\r\n' +
      '</ul>\r\n' +
      '\r\n' + defaultTemplateContent;

      await umbracoApi.template.createDefaultTemplate(templateName);

    // Act
    await umbracoUi.template.goToTemplate(templateName);
    await umbracoUi.template.addQueryBuilderWithWhereStatement(propertyAliasValue, operatorValue, constrainValue);
    // Verify that the code is shown
    await umbracoUi.template.isQueryBuilderCodeShown(expectedCode);
    await umbracoUi.template.clickSubmitButton();
    await umbracoUi.template.clickSaveButton();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    const templateData = await umbracoApi.template.getByName(templateName);
    expect(templateData.content).toBe(expectedTemplateContent);
  });

  test('can insert sections - render child template into a template', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const sectionType = 'Render child template';
    const insertedContent = '@RenderBody()';
    await umbracoApi.template.createDefaultTemplate(templateName);
    const templateContent = insertedContent + defaultTemplateContent;

    // Act
    await umbracoUi.template.goToTemplate(templateName);
    await umbracoUi.template.insertSection(sectionType);
    await umbracoUi.template.clickSaveButton();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    const templateData = await umbracoApi.template.getByName(templateName);
    expect(templateData.content).toBe(templateContent);
  });

  test('can insert sections - render a named section into a template', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const sectionType = 'Render a named section';
    const sectionName = 'TestSectionName';
    const insertedContent = '@RenderSection("' + sectionName + '", false)';
    await umbracoApi.template.createDefaultTemplate(templateName);
    const templateContent = insertedContent + defaultTemplateContent;

    // Act
    await umbracoUi.template.goToTemplate(templateName);
    await umbracoUi.template.insertSection(sectionType, sectionName);
    await umbracoUi.template.clickSaveButton();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    const templateData = await umbracoApi.template.getByName(templateName);
    expect(templateData.content).toBe(templateContent);
  });

  test('can insert sections - define a named section into a template', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const sectionType = 'Define a named section';
    const sectionName = 'TestSectionName';
    const insertedContent = '@section ' + sectionName + '\r\n{\r\n\r\n\r\n\r\n}';
    await umbracoApi.template.createDefaultTemplate(templateName);
    const templateContent = insertedContent + defaultTemplateContent;

    // Act
    await umbracoUi.template.goToTemplate(templateName);
    await umbracoUi.template.insertSection(sectionType, sectionName);
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
    const templateContent = '@Umbraco.GetDictionaryValue("TestDictionary")' + defaultTemplateContent;

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

  test('can rename a template', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongTemplateName = 'WrongTemplateName';
    const templateAlias = AliasHelper.toAlias(wrongTemplateName);
    await umbracoApi.template.ensureNameNotExists(wrongTemplateName);
    const templateId = await umbracoApi.template.create(wrongTemplateName, templateAlias, '');
    expect(await umbracoApi.template.doesNameExist(wrongTemplateName)).toBeTruthy();

    // Act
    await umbracoUi.template.goToTemplate(wrongTemplateName);
    await umbracoUi.template.enterTemplateName(templateName);
    await umbracoUi.template.clickSaveButton();

    // Assert
    await umbracoUi.template.isSuccessNotificationVisible();
    const templateData = await umbracoApi.template.get(templateId);
    expect(templateData.name).toBe(templateName);
  });
});
