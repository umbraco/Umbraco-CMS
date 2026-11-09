import {AliasHelper, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const documentTypeName = 'TestDocType';
const documentTypeGroupName = 'TestGroup';
const blockListName = 'VariantList';
const blockName = 'BlockElement';
const blockGroupName = 'BlockGroup';
const textStringName = 'Textstring';
const contentName = 'TestContent';
const englishCulture = 'en-US';
const danishCulture = 'da';
const updatedText = 'updated-english';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.language.createDanishLanguage();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockName);
  await umbracoApi.dataType.ensureNameNotExists(blockListName);
  await umbracoApi.language.ensureIsoCodeNotExists('da');
});

async function createContentWithBlocksInSeveralCultures(umbracoApi) {
  const textStringDataType = await umbracoApi.dataType.getByName(textStringName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(
    blockName, blockGroupName, textStringName, textStringDataType.id, true, true
  );
  const dataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
    documentTypeName, blockListName, dataTypeId, documentTypeGroupName, true, false
  );
  await umbracoApi.document.createDocumentWithBlockListBlocksInCultures(
    contentName,
    documentTypeId,
    AliasHelper.toAlias(blockListName),
    elementTypeId,
    AliasHelper.toAlias(textStringName),
    'Umbraco.TextBox',
    [englishCulture, danishCulture],
    [
      {[englishCulture]: 'first-english', [danishCulture]: 'first-danish'},
      {[englishCulture]: 'second-english', [danishCulture]: 'second-danish'},
    ]
  );
}

test('can navigate away from variant content with block list blocks in several cultures without seeing discard changes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await createContentWithBlocksInSeveralCultures(umbracoApi);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.doesBlockListPropertyHaveBlockAmount(documentTypeGroupName, blockListName, 2);

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.settings);

  // Assert
  await umbracoUi.content.isSectionActive(ConstantHelper.sections.settings);
  await umbracoUi.content.isDiscardChangesModalVisible(false);
});

test('can navigate away from variant content with block list blocks in several cultures after saving a block change without seeing discard changes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await createContentWithBlocksInSeveralCultures(umbracoApi);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName, 0);
  await umbracoUi.content.enterTextstring(updatedText);
  await umbracoUi.content.clickUpdateButton();
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickContainerSaveButtonAndWaitForContentToBeUpdated();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.settings);

  // Assert
  await umbracoUi.content.isSectionActive(ConstantHelper.sections.settings);
  await umbracoUi.content.isDiscardChangesModalVisible(false);
});

test('can see discard changes when navigating away from variant content with a changed block list block value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await createContentWithBlocksInSeveralCultures(umbracoApi);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName, 0);
  await umbracoUi.content.enterTextstring(updatedText);
  await umbracoUi.content.clickUpdateButton();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.settings);

  // Assert
  await umbracoUi.content.isDiscardChangesModalVisible();
});
