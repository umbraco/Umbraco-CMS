import {AliasHelper, test} from '@umbraco/acceptance-test-helpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Date Picker';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

const dateTimes = [
  {type: 'with AM time', value: '2024-10-29 09:09:09', expectedValue: '10/29/2024 9:09:09 AM', dataTypeName: 'Date Picker with time', isDateOnly: false},
  {type: 'with PM time', value: '2024-10-29 21:09:09', expectedValue: '10/29/2024 9:09:09 PM', dataTypeName: 'Date Picker with time', isDateOnly: false},
  // The date-only picker still publishes a DateTime, so the template formats it as a date.
  {type: 'without time', value: '2024-10-29 00:00:00', expectedValue: '10/29/2024', dataTypeName: 'Date Picker', isDateOnly: true}
];

for (const dateTime of dateTimes) {
  test(`can render content with a date ${dateTime.type}`, async ({umbracoApi, umbracoUi}) => {
    const dataTypeData = await umbracoApi.dataType.getByName(dateTime.dataTypeName);
    const propertyAlias = AliasHelper.toAlias(propertyName);
    const templateId = dateTime.isDateOnly
      ? await umbracoApi.template.createTemplateWithDisplayingDateValue(templateName, propertyAlias)
      : await umbracoApi.template.createTemplateWithDisplayingStringValue(templateName, propertyAlias);
    const contentKey = await umbracoApi.document.createPublishedDocumentWithValue(contentName, dateTime.value, dataTypeData.id, templateId, propertyName, documentTypeName);
    const contentURL= await umbracoApi.document.getDocumentUrl(contentKey);

    // Act
    await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

    // Assert
    await umbracoUi.contentRender.doesContentRenderValueContainText(dateTime.expectedValue, true);
  });
}
