import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const templateName = 'EntityPickerTreeTemplate';
const dataTypeName = 'EntityPickerWithTree';
const propertyName = 'TestProperty';
const treeDataSourceAlias = 'My.PickerDataSource.Tree';

const templateValue = `@using Umbraco.Cms.Core.Models;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage;
@{
    Layout = null;
    var entityDataPickerValue = Model.Value<EntityDataPickerValue>("${AliasHelper.toAlias(propertyName)}");
}

@if (entityDataPickerValue is null)
{ }
else
{
    <div data-mark="data-source-render-value">
        <p>@entityDataPickerValue.DataSource</p>
    </div>

    <div data-mark="content-render-value">
        <ul>

        @foreach (var id in @entityDataPickerValue.Ids)
        {
            <li>@id</li>
        }
        </ul>
    </div>
}
`

const items = {ids: ['4', '2']};
test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test('can render content with a entity picker with the tree data source', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeId = await umbracoApi.dataType.createEntityDataPickerDataType(dataTypeName, treeDataSourceAlias);
  const templateId = await umbracoApi.template.createTemplateWithContent(templateName, templateValue);
  const contentKey = await umbracoApi.document.createPublishedDocumentWithValue(contentName, items, dataTypeId, templateId, propertyName, documentTypeName);
  const contentURL = await umbracoApi.document.getDocumentUrl(contentKey);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesDataSourceRenderValueHaveText(treeDataSourceAlias);
  for (const value of items.ids) {
    await umbracoUi.contentRender.doesContentRenderValueContainText(value);
  }
});
