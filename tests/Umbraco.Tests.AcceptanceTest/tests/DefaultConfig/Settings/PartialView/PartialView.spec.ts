import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Partial View tests', () => {
  const partialViewName = 'TestPartialView';
  const partialViewFileName = partialViewName + '.cshtml';
  const dictionaryName = 'TestDictionaryPartialView';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.partialView.goToSection(ConstantHelper.sections.settings);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  });

  test('can create an empty partial view', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.partialView.clickActionsMenuAtRoot();
    await umbracoUi.partialView.clickCreateThreeDotsButton();
    await umbracoUi.partialView.clickNewEmptyPartialViewButton();
    await umbracoUi.partialView.enterPartialViewName(partialViewName);
    await umbracoUi.partialView.clickSaveButton();

    // Assert
    await umbracoUi.partialView.isSuccessNotificationVisible();
    expect(await umbracoApi.partialView.doesNameExist(partialViewFileName)).toBeTruthy();
    // Verify the new partial view is displayed under the Partial Views section
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await expect(umbracoUi.partialView.checkItemNameUnderPartialViewTree(partialViewFileName)).toBeVisible();
  })

  test('can create a partial view from snippet', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedPartialViewContentWindows = '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n@using Umbraco.Cms.Core.Routing\r\n@using Umbraco.Extensions\r\n\n@inject IPublishedUrlProvider PublishedUrlProvider\r\n@*\r\n    This snippet makes a breadcrumb of parents using an unordered HTML list.\r\n\r\n    How it works:\r\n    - It uses the Ancestors() method to get all parents and then generates links so the visitor can go back\r\n    - Finally it outputs the name of the current page (without a link)\r\n*@\r\n\r\n@{ var selection = Model.Ancestors().ToArray(); }\r\n\r\n@if (selection?.Length > 0)\r\n{\r\n    <ul class=\"breadcrumb\">\r\n        @* For each page in the ancestors collection which have been ordered by Level (so we start with the highest top node first) *@\r\n        @foreach (var item in selection.OrderBy(x => x.Level))\r\n        {\r\n            <li><a href=\"@item.Url(PublishedUrlProvider)\">@item.Name</a> <span class=\"divider\">/</span></li>\r\n        }\r\n\r\n        @* Display the current page as the last item in the list *@\r\n        <li class=\"active\">@Model.Name</li>\r\n    </ul>\r\n}';
    const expectedPartialViewContentLinux = '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@using Umbraco.Cms.Core.Routing\n@using Umbraco.Extensions\n\n@inject IPublishedUrlProvider PublishedUrlProvider\n@*\n    This snippet makes a breadcrumb of parents using an unordered HTML list.\n\n    How it works:\n    - It uses the Ancestors() method to get all parents and then generates links so the visitor can go back\n    - Finally it outputs the name of the current page (without a link)\n*@\n\n@{ var selection = Model.Ancestors().ToArray(); }\n\n@if (selection?.Length > 0)\n{\n    <ul class=\"breadcrumb\">\n        @* For each page in the ancestors collection which have been ordered by Level (so we start with the highest top node first) *@\n        @foreach (var item in selection.OrderBy(x => x.Level))\n        {\n            <li><a href=\"@item.Url(PublishedUrlProvider)\">@item.Name</a> <span class=\"divider\">/</span></li>\n        }\n\n        @* Display the current page as the last item in the list *@\n        <li class=\"active\">@Model.Name</li>\n    </ul>\n}';

    // Act
    await umbracoUi.partialView.clickActionsMenuAtRoot();
    await umbracoUi.partialView.clickCreateThreeDotsButton();
    await umbracoUi.partialView.clickNewPartialViewFromSnippetButton();
    await umbracoUi.partialView.clickBreadcrumbButton();
    await umbracoUi.partialView.enterPartialViewName(partialViewName);
    await umbracoUi.partialView.clickSaveButton();

    // Assert
    await umbracoUi.partialView.isSuccessNotificationVisible();
    expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();
    const partialViewData = await umbracoApi.partialView.getByName(partialViewFileName);

    switch (process.platform) {
      case 'win32':
        expect(partialViewData.content).toBe(expectedPartialViewContentWindows);
        break;
      case 'linux':
        expect(partialViewData.content).toBe(expectedPartialViewContentLinux);
        break;
      default:
        throw new Error(`Untested platform: ${process.platform}`);
    }

    // Verify the new partial view is displayed under the Partial Views section
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await expect(umbracoUi.partialView.checkItemNameUnderPartialViewTree(partialViewFileName)).toBeVisible();
  });

  test.skip('can update a partial view name', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongPartialViewName = 'WrongName';
    const wrongPartialViewFileName = wrongPartialViewName + '.cshtml';

    await umbracoApi.partialView.ensureNameNotExists(wrongPartialViewFileName);
    await umbracoApi.partialView.create(wrongPartialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");
    expect(await umbracoApi.partialView.doesExist(wrongPartialViewFileName)).toBeTruthy();

    //Act
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await umbracoUi.partialView.clickActionsMenuForPartialView(wrongPartialViewFileName);
    await umbracoUi.partialView.rename(partialViewName);

    // Assert
    await umbracoUi.partialView.isSuccessNotificationVisible();
    expect(await umbracoApi.partialView.doesNameExist(partialViewFileName)).toBeTruthy();
    expect(await umbracoApi.partialView.doesNameExist(wrongPartialViewFileName)).toBeFalsy();
    // Verify the old partial view is NOT displayed under the Partial Views section
    await expect(umbracoUi.partialView.checkItemNameUnderPartialViewTree(wrongPartialViewFileName)).not.toBeVisible();
    // Verify the new partial view is displayed under the Partial Views section
    await expect(umbracoUi.partialView.checkItemNameUnderPartialViewTree(partialViewFileName)).toBeVisible();

  });

  test('can update a partial view content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const updatedPartialViewContent = '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n' +
      '@{\r\n' +
      '\tLayout = null;\r\n' +
      '}\r\n' +
      '<p>AcceptanceTests</p>';

    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");
    expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();

    //Act
    await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
    await umbracoUi.partialView.enterPartialViewContent(updatedPartialViewContent);
    await umbracoUi.partialView.clickSaveButton();

    // Assert
    const updatedPartialView = await umbracoApi.partialView.getByName(partialViewFileName);
    expect(updatedPartialView.content).toBe(updatedPartialViewContent);
  });

  test('can use query builder with Order By statement for a partial view', async ({umbracoApi, umbracoUi}) => {
    //Arrange
    const propertyAliasValue = 'UpdateDate';
    const isAscending = false;
    const expectedCode = 'Umbraco.ContentAtRoot().FirstOrDefault()\r\n' +
    '    .Children()\r\n' +
    '    .Where(x => x.IsVisible())\r\n' +
    '    .OrderByDescending(x => x.' + propertyAliasValue + ')';
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
      '\r\n' +
      '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n';

    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");
    expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();

    // Act
    await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
    await umbracoUi.partialView.addQueryBuilderWithOrderByStatement(propertyAliasValue, isAscending);
    // Verify that the code is shown
    await umbracoUi.partialView.isQueryBuilderCodeShown(expectedCode);
    await umbracoUi.partialView.clickSubmitButton();
    await umbracoUi.partialView.clickSaveButton();

    // Assert
    const updatedPartialView = await umbracoApi.partialView.getByName(partialViewFileName);
    expect(updatedPartialView.content).toBe(expectedTemplateContent);
  });

  test.skip('can use query builder with Where statement for a partial view', async ({umbracoApi, umbracoUi}) => {
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
      '\r\n' +
      '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n';

    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");
    expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();

    // Act
    await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
    await umbracoUi.partialView.addQueryBuilderWithWhereStatement(propertyAliasValue, operatorValue, constrainValue);
    // Verify that the code is shown
    await umbracoUi.partialView.isQueryBuilderCodeShown(expectedCode);
    await umbracoUi.partialView.clickSubmitButton();
    await umbracoUi.partialView.clickSaveButton();

    // Assert
    const updatedPartialView = await umbracoApi.partialView.getByName(partialViewFileName);
    expect(updatedPartialView.content).toBe(expectedTemplateContent);
  });

  test('can insert dictionaryItem into a partial view', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");
    expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();

    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
    await umbracoApi.dictionary.create(dictionaryName);

    const partialViewContent = '@Umbraco.GetDictionaryValue("' + dictionaryName + '")@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n';

    // Act
    await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
    await umbracoUi.partialView.insertDictionaryByName(dictionaryName);
    await umbracoUi.partialView.clickSaveButton();

    // Assert
    const partialViewData = await umbracoApi.partialView.getByName(partialViewFileName);
    expect(partialViewData.content).toBe(partialViewContent);
  });

  test('can delete a partial view', async ({umbracoApi, umbracoUi}) => {
    //Arrange
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");
    expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();

    //Act
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await umbracoUi.partialView.clickActionsMenuForPartialView(partialViewFileName);
    await umbracoUi.partialView.deletePartialView();

    // Assert
    await umbracoUi.partialView.isSuccessNotificationVisible();
    expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeFalsy();
    // Verify the partial view is NOT displayed under the Partial Views section
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await expect(umbracoUi.partialView.checkItemNameUnderPartialViewTree(partialViewFileName)).not.toBeVisible();
  });
});
