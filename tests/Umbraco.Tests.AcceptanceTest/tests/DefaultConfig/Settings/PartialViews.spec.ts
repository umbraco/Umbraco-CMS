import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Partial Views tests', () => {
  const partialViewName = 'TestPartialView';
  const partialViewFileName = partialViewName + ".cshtml";

  test.beforeEach(async ({umbracoUi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
  });

  test('can create an empty partial view', async ({umbracoApi, umbracoUi}) => {
    //Arrange
    await umbracoApi.partialView.ensureNameNotExists(partialViewName);

    // Act
    await umbracoUi.partialView.clickActionsMenuAtRoot();
    await umbracoUi.partialView.clickNewEmptyPartialViewButton();
    await umbracoUi.partialView.enterPartialViewName(partialViewName);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(1000);
    await umbracoUi.partialView.clickSaveButton();

    // Assert
    expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();
    // TODO: when frontend is ready, verify the new partial view is displayed under the Partial Views section
    // TODO: when frontend is ready, verify the notification displays

    // Clean
    await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
  });

  test.skip('can create a partial view from snippet', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
    const expectedTemplateContent = '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n@using Umbraco.Cms.Core.Routing\r\n@using Umbraco.Extensions\r\n\n@inject IPublishedUrlProvider PublishedUrlProvider\r\n@*\r\n    This snippet makes a breadcrumb of parents using an unordered HTML list.\r\n\r\n    How it works:\r\n    - It uses the Ancestors() method to get all parents and then generates links so the visitor can go back\r\n    - Finally it outputs the name of the current page (without a link)\r\n*@\r\n\r\n@{ var selection = Model.Ancestors().ToArray(); }\r\n\r\n@if (selection?.Length > 0)\r\n{\r\n    <ul class=\"breadcrumb\">\r\n        @* For each page in the ancestors collection which have been ordered by Level (so we start with the highest top node first) *@\r\n        @foreach (var item in selection.OrderBy(x => x.Level))\r\n        {\r\n            <li><a href=\"@item.Url(PublishedUrlProvider)\">@item.Name</a> <span class=\"divider\">/</span></li>\r\n        }\r\n\r\n        @* Display the current page as the last item in the list *@\r\n        <li class=\"active\">@Model.Name</li>\r\n    </ul>\r\n}';

    // Act
    await umbracoUi.partialView.clickActionsMenuAtRoot();
    await umbracoUi.partialView.clickNewPartialViewFromSnippetButton();
    await umbracoUi.partialView.clickBreadcrumbButton();
    await umbracoUi.partialView.enterPartialViewName(partialViewName);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(1000);
    await umbracoUi.partialView.clickSaveButton();

    // Assert
    expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();
    const partialViewData = await umbracoApi.partialView.getByName(partialViewFileName);
    expect(partialViewData.content).toBe(expectedTemplateContent);
    // TODO: when frontend is ready, verify the new partial view is displayed under the Partial Views section
    // TODO: when frontend is ready, verify the notification displays

    // Clean
    await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
  });

  test('can update a partial view name', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const updatedPartialViewName = 'TestPartialViewUpdated';
    const updatedPartialViewFileName = updatedPartialViewName + ".cshtml";

    await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");

    //Act
    await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
    await umbracoUi.partialView.enterPartialViewName(updatedPartialViewName);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(1000);
    await umbracoUi.partialView.clickSaveButton();

    // Assert
    expect(await umbracoApi.partialView.doesExist(updatedPartialViewFileName)).toBeTruthy();
    // TODO: when frontend is ready, verify the updated partial view name is displayed under the Partial Views section
    expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeFalsy();
    // TODO: when frontend is ready, verify the old partial view name is NOT displayed under the Partial Views section

    // Clean
    await umbracoApi.partialView.ensureNameNotExists(updatedPartialViewFileName);
  });

  test('can update a partial view content', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const updatedPartialViewContent = '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n' +
      '@{\r\n' +
      '\tLayout = null;\r\n' +
      '}\r\n' +
      '<p>AcceptanceTests</p>';

    await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");

    //Act
    await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
    await umbracoUi.partialView.enterPartialViewContent(updatedPartialViewContent);
    // TODO: Remove this timeout when frontend validation is implemented
    await umbracoUi.waitForTimeout(1000);
    await umbracoUi.partialView.clickSaveButton();

    // Assert
    const updatedPartialView = await umbracoApi.partialView.getByName(partialViewFileName);
    expect(updatedPartialView.content).toBe(updatedPartialViewContent);

    // Clean
    await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
  });

  test('can use query builder for a partial view', async ({umbracoApi, umbracoUi}) => {
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

    await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");

    // Act
    await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
    await umbracoUi.partialView.addQueryBuilderIntoPartialViewWithCreateDateOption();
    await umbracoUi.partialView.clickSaveButton();

    // Assert
    const updatedPartialView = await umbracoApi.partialView.getByName(partialViewFileName);
    expect(updatedPartialView.content).toBe(expectedTemplateContent);

    // Clean
    await umbracoApi.template.ensureNameNotExists(partialViewFileName);
  });

  // Skip this test now as the function delete dictionary is missing
  test.skip('can insert dictionaryItem into a partial view', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dictionaryName = 'TestDictionary';

    await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");

    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
    await umbracoApi.dictionary.create(dictionaryName);

    const partialViewContent = '@Umbraco.GetDictionaryValue("' + dictionaryName + '")@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n';

    // Act
    await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
    await umbracoUi.partialView.insertDictionaryItem(dictionaryName);
    await umbracoUi.partialView.clickSaveButton();

    // Assert
    const partialViewData = await umbracoApi.partialView.getByName(partialViewFileName);
    expect(partialViewData.content).toBe(partialViewContent);

    // Clean
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
    await umbracoApi.template.ensureNameNotExists(partialViewFileName);
  });

  test.skip('can delete a partial view', async ({umbracoApi, umbracoUi}) => {
    //Arrange
    await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
    await umbracoApi.partialView.create(partialViewFileName, "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n", "/");

    //Act
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
    await umbracoUi.partialView.deletePartialView();

    // Assert
    expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeFalsy();
    // TODO: when frontend is ready, verify the partial view is NOT displayed under the Partial Views section
    // TODO: when frontend is ready, verify the notification displays

  });

  test('can create a folder', async ({umbracoApi, umbracoUi}) => {
    //Arrange
    const folderName = 'TestFolder';
    await umbracoApi.partialView.ensureNameNotExists(folderName);

    // Act
    await umbracoUi.partialView.clickActionsMenuAtRoot();
    await umbracoUi.partialView.createNewFolder(folderName);

    // Assert
    expect(await umbracoApi.partialView.doesFolderExist(folderName)).toBeTruthy();
    // TODO: when frontend is ready, verify the new folder is  displayed under the Partial Views section
    // TODO: when frontend is ready, verify the notification displays

    //Clean
    await umbracoApi.partialView.ensureNameNotExists(folderName);
  });

  test.skip('can place a partial view into folder', async ({page, umbracoApi}) => {
    // TODO: implement this later as the frontend is missing now
  });

});
