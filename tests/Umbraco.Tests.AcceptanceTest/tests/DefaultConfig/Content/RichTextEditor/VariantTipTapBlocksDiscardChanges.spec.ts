import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const documentTypeName = 'DocumentTypeName';
const documentTypeGroupName = 'DocumentGroup';
const tipTapName = 'TipTapTest';
const blockName = 'BlockName';
const elementGroupName = 'ElementGroup';
const textStringName = 'TextStringName';
const textStringDataTypeName = 'Textstring';
const textStringText = 'ThisIsATextString';
const contentName = 'ContentName';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.language.ensureIsoCodeNotExists('da');
  await umbracoApi.language.createDanishLanguage();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockName);
  await umbracoApi.dataType.ensureNameNotExists(tipTapName);
  await umbracoApi.language.ensureIsoCodeNotExists('da');
});

async function openVariantContentWithTipTapAndAddABlock(umbracoApi, umbracoUi) {
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataType.id, true, true);
  const tipTapId = await umbracoApi.dataType.createTipTapDataTypeWithABlock(tipTapName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, tipTapName, tipTapId, documentTypeGroupName, true);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateModalButton();
}

test('can navigate away from variant content with a saved block in a tiptap RTE without seeing discard changes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await openVariantContentWithTipTapAndAddABlock(umbracoApi, umbracoUi);
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickContainerSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.reloadPage();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.settings);

  // Assert
  await umbracoUi.content.isSectionActive(ConstantHelper.sections.settings);
  await umbracoUi.content.isDiscardChangesModalVisible(false);
});

test('can see discard changes when navigating away from variant content with an added block in a tiptap RTE', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await openVariantContentWithTipTapAndAddABlock(umbracoApi, umbracoUi);

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.settings);

  // Assert
  await umbracoUi.content.isDiscardChangesModalVisible();
});
