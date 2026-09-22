import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Textstring';
const firstTabName = 'FirstTab';
const secondTabName = 'SecondTab';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.language.ensureIsoCodeNotExists('da');
  await umbracoApi.language.createDanishLanguage();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists('da');
});

// Product gap (#23706): a client-side mandatory error is lost on re-render after switching tabs and
// culture, even though the field is genuinely still empty when returning to it. Verified live that the
// validation message renders correctly before switching, then never reappears after switching back.
test.skip('client-side mandatory error survives a tab switch and a culture switch', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithMandatoryCultureVaryingPropertyInTwoTabs(documentTypeName, dataTypeName, dataTypeData.id, firstTabName, secondTabName);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  // Trigger a client-side mandatory error on the first tab by attempting to publish with an empty value.
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.nullValue);
  await umbracoUi.content.clickTabWithName(secondTabName);
  await umbracoUi.content.switchLanguage('Danish');
  await umbracoUi.content.switchLanguage('English');
  await umbracoUi.content.clickTabWithName(firstTabName);

  // Assert
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.nullValue);
});
