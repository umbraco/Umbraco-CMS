import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const partialViewName = 'TestPartialView';
const partialViewFileName = partialViewName + '.cshtml';
const dictionaryName = 'TestDictionaryPartialView';
const defaultPartialViewContent = '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.partialView.goToSection(ConstantHelper.sections.settings);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
});

test('can create an empty partial view', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.partialView.clickActionsMenuAtRoot();
  await umbracoUi.partialView.clickCreateButton();
  await umbracoUi.partialView.clickNewEmptyPartialViewButton();
  await umbracoUi.partialView.enterPartialViewName(partialViewName);
  await umbracoUi.partialView.clickSaveButton();

  // Assert
  //await umbracoUi.partialView.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.partialView.isErrorNotificationVisible(false);
  expect(await umbracoApi.partialView.doesNameExist(partialViewFileName)).toBeTruthy();
  // Verify the new partial view is displayed under the Partial Views section
  await umbracoUi.partialView.isPartialViewRootTreeItemVisible(partialViewFileName);
});

test('can create a partial view from snippet', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedPartialViewContentWindows = '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\r\n@using Umbraco.Cms.Core.PublishedCache\r\n@using Umbraco.Cms.Core.Routing\r\n@using Umbraco.Cms.Core.Services.Navigation\r\n\n@inject IPublishedContentCache PublishedContentCache\r\n@inject IDocumentNavigationQueryService DocumentNavigationQueryService\r\n@inject IPublishedUrlProvider PublishedUrlProvider\r\n@*\r\n    This snippet makes a breadcrumb of parents using an unordered HTML list.\r\n\r\n    How it works:\r\n    - It uses the Ancestors method to get all parents and then generates links so the visitor can go back\r\n    - Finally it outputs the name of the current page (without a link)\r\n*@\r\n\r\n@{ var selection = Model.Ancestors(PublishedContentCache, DocumentNavigationQueryService).ToArray(); }\r\n\r\n@if (selection?.Length > 0)\r\n{\r\n    <ul class=\"breadcrumb\">\r\n        @* For each page in the ancestors collection which have been ordered by Level (so we start with the highest top node first) *@\r\n        @foreach (var item in selection.OrderBy(x => x.Level))\r\n        {\r\n            <li><a href=\"@item.Url(PublishedUrlProvider)\">@item.Name</a> <span class=\"divider\">/</span></li>\r\n        }\r\n\r\n        @* Display the current page as the last item in the list *@\r\n        <li class=\"active\">@Model.Name</li>\r\n    </ul>\r\n}';
  const expectedPartialViewContentLinux = '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@using Umbraco.Cms.Core.PublishedCache\n@using Umbraco.Cms.Core.Routing\n@using Umbraco.Cms.Core.Services.Navigation\n\n@inject IPublishedContentCache PublishedContentCache\n@inject IDocumentNavigationQueryService DocumentNavigationQueryService\n@inject IPublishedUrlProvider PublishedUrlProvider\n@*\n    This snippet makes a breadcrumb of parents using an unordered HTML list.\n\n    How it works:\n    - It uses the Ancestors method to get all parents and then generates links so the visitor can go back\n    - Finally it outputs the name of the current page (without a link)\n*@\n\n@{ var selection = Model.Ancestors(PublishedContentCache, DocumentNavigationQueryService).ToArray(); }\n\n@if (selection?.Length > 0)\n{\n    <ul class=\"breadcrumb\">\n        @* For each page in the ancestors collection which have been ordered by Level (so we start with the highest top node first) *@\n        @foreach (var item in selection.OrderBy(x => x.Level))\n        {\n            <li><a href=\"@item.Url(PublishedUrlProvider)\">@item.Name</a> <span class=\"divider\">/</span></li>\n        }\n\n        @* Display the current page as the last item in the list *@\n        <li class=\"active\">@Model.Name</li>\n    </ul>\n}';

  // Act
  await umbracoUi.partialView.clickActionsMenuAtRoot();
  await umbracoUi.partialView.clickCreateButton();
  await umbracoUi.partialView.clickNewPartialViewFromSnippetButton();
  await umbracoUi.partialView.clickBreadcrumbButton();
  await umbracoUi.partialView.enterPartialViewName(partialViewName);
  await umbracoUi.partialView.clickSaveButton();

  // Assert
  //await umbracoUi.partialView.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.partialView.isErrorNotificationVisible(false);
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
  await umbracoUi.partialView.isPartialViewRootTreeItemVisible(partialViewFileName);
});

test('can rename a partial view', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongPartialViewName = 'WrongName';
  const wrongPartialViewFileName = wrongPartialViewName + '.cshtml';

  await umbracoApi.partialView.ensureNameNotExists(wrongPartialViewFileName);
  await umbracoApi.partialView.create(wrongPartialViewFileName, defaultPartialViewContent, '/');
  expect(await umbracoApi.partialView.doesExist(wrongPartialViewFileName)).toBeTruthy();

  //Act
  await umbracoUi.partialView.reloadPartialViewTree();
  await umbracoUi.partialView.clickActionsMenuForPartialView(wrongPartialViewFileName);
  await umbracoUi.partialView.rename(partialViewName);

  // Assert
  await umbracoUi.partialView.isErrorNotificationVisible(false);
  expect(await umbracoApi.partialView.doesNameExist(partialViewFileName)).toBeTruthy();
  expect(await umbracoApi.partialView.doesNameExist(wrongPartialViewFileName)).toBeFalsy();
  // Verify the old partial view is NOT displayed under the Partial Views section
  await umbracoUi.partialView.isPartialViewRootTreeItemVisible(wrongPartialViewFileName, false, false);
  // Verify the new partial view is displayed under the Partial Views section
  await umbracoUi.partialView.isPartialViewRootTreeItemVisible(partialViewFileName, true, false);
});

test('can update a partial view content', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedPartialViewContent = defaultPartialViewContent +
    '@{\r\n' +
    '\tLayout = null;\r\n' +
    '}\r\n' +
    '<p>AcceptanceTests</p>';

  await umbracoApi.partialView.create(partialViewFileName, defaultPartialViewContent, '/');
  expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();

  //Act
  await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
  await umbracoUi.partialView.enterPartialViewContent(updatedPartialViewContent);
  await umbracoUi.partialView.clickSaveButton();

  // Assert
  //await umbracoUi.partialView.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.partialView.isErrorNotificationVisible(false);
  const updatedPartialView = await umbracoApi.partialView.getByName(partialViewFileName);
  expect(updatedPartialView.content).toBe(updatedPartialViewContent);
});

// Remove .fixme when the issue is fixed: https://github.com/umbraco/Umbraco-CMS/issues/18536
test.fixme('can use query builder with Order By statement for a partial view', async ({umbracoApi, umbracoUi}) => {
  //Arrange
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
    '\r\n' + defaultPartialViewContent;

  await umbracoApi.partialView.create(partialViewFileName, defaultPartialViewContent, '/');
  expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();

  // Act
  await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
  await umbracoUi.partialView.addQueryBuilderWithOrderByStatement(propertyAliasValue, isAscending);
  // Verify that the code is shown
  await umbracoUi.partialView.isQueryBuilderCodeShown(expectedCode);
  await umbracoUi.partialView.clickSubmitButton();
  await umbracoUi.partialView.clickSaveButton();

  // Assert
  //await umbracoUi.partialView.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.partialView.isErrorNotificationVisible(false);
  const updatedPartialView = await umbracoApi.partialView.getByName(partialViewFileName);
  expect(updatedPartialView.content).toBe(expectedTemplateContent);
});

// Remove .fixme when the issue is fixed: https://github.com/umbraco/Umbraco-CMS/issues/18536
test.fixme('can use query builder with Where statement for a partial view', async ({umbracoApi, umbracoUi}) => {
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
    '\r\n' + defaultPartialViewContent;

  await umbracoApi.partialView.create(partialViewFileName, defaultPartialViewContent, '/');
  expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();

  // Act
  await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
  await umbracoUi.partialView.addQueryBuilderWithWhereStatement(propertyAliasValue, operatorValue, constrainValue);
  // Verify that the code is shown
  await umbracoUi.partialView.isQueryBuilderCodeShown(expectedCode);
  await umbracoUi.partialView.clickSubmitButton();
  await umbracoUi.partialView.clickSaveButton();

  // Assert
  //await umbracoUi.partialView.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.partialView.isErrorNotificationVisible(false);
  const updatedPartialView = await umbracoApi.partialView.getByName(partialViewFileName);
  expect(updatedPartialView.content).toBe(expectedTemplateContent);
});

test('can insert dictionary item into a partial view', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.partialView.create(partialViewFileName, defaultPartialViewContent, '/');
  expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();

  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  await umbracoApi.dictionary.create(dictionaryName);

  const partialViewContent = '@Umbraco.GetDictionaryValue("' + dictionaryName + '")' + defaultPartialViewContent;

  // Act
  await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
  await umbracoUi.partialView.insertDictionaryItem(dictionaryName);
  await umbracoUi.partialView.clickSaveButton();

  // Assert
  //await umbracoUi.partialView.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.partialView.isErrorNotificationVisible(false);
  const partialViewData = await umbracoApi.partialView.getByName(partialViewFileName);
  expect(partialViewData.content).toBe(partialViewContent);
});

test('can insert value into a partial view', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.partialView.create(partialViewFileName, defaultPartialViewContent, '/');
  expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();
  const systemFieldValue = 'createDate';
  const partialViewContent = '@Model.Value("' + systemFieldValue + '")' + defaultPartialViewContent;

  // Act
  await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
  await umbracoUi.waitForTimeout(1000);
  await umbracoUi.template.insertSystemFieldValue(systemFieldValue);
  await umbracoUi.template.clickSaveButton();

  // Assert
  //await umbracoUi.partialView.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.partialView.isErrorNotificationVisible(false);
  const partialViewData = await umbracoApi.partialView.getByName(partialViewFileName);
  expect(partialViewData.content).toBe(partialViewContent);
});

test('can delete a partial view', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.partialView.create(partialViewFileName, partialViewFileName, '/');
  expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();

  //Act
  await umbracoUi.partialView.clickRootFolderCaretButton();
  await umbracoUi.partialView.clickActionsMenuForPartialView(partialViewFileName);
  await umbracoUi.partialView.clickDeleteAndConfirmButton();

  // Assert
  //await umbracoUi.partialView.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  await umbracoUi.partialView.isErrorNotificationVisible(false);
  expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeFalsy();
  // Verify the partial view is NOT displayed under the Partial Views section
  await umbracoUi.partialView.clickRootFolderCaretButton();
  await umbracoUi.partialView.isPartialViewRootTreeItemVisible(partialViewFileName, false, false);
});

// TODO: Remove skip when the front-end is ready. Currently the returned items count is not updated after choosing the root content.
test.skip('can show returned items in query builder ', async ({umbracoApi, umbracoUi}) => {
  //Arrange
  // Create content at root with a child
  const documentTypeName = 'ParentDocumentType';
  const childDocumentTypeName = 'ChildDocumentType';
  const contentName = 'ContentName';
  const childContentName = 'ChildContentName';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(childDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, contentId);
  // Create partial view
  await umbracoApi.partialView.create(partialViewFileName, partialViewFileName, '/');
  expect(await umbracoApi.partialView.doesExist(partialViewFileName)).toBeTruthy();

  //Act
  await umbracoUi.partialView.openPartialViewAtRoot(partialViewFileName);
  await umbracoUi.partialView.clickQueryBuilderButton();
  await umbracoUi.partialView.chooseRootContentInQueryBuilder(contentName);

  // Assert
  await umbracoUi.partialView.doesReturnedItemsHaveCount(1);
  await umbracoUi.partialView.doesQueryResultHaveContentName(childContentName);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('cannot create a partial view with an empty name', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.partialView.clickActionsMenuAtRoot();
  await umbracoUi.partialView.clickCreateButton();
  await umbracoUi.partialView.clickNewEmptyPartialViewButton();
  await umbracoUi.partialView.clickSaveButton();

  // Assert
  // TODO: Uncomment this when the front-end is ready. Currently there is no error displays.
  // await umbracoUi.partialView.isErrorNotificationVisible();
  expect(await umbracoApi.partialView.doesNameExist(partialViewFileName)).toBeFalsy();
});
