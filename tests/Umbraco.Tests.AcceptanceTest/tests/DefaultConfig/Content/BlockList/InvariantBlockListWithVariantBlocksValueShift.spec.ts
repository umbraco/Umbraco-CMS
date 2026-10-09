import {AliasHelper, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

/**
 * Document Type (varies by culture)
 * └── Block List Property (invariant - blocks are shared across languages)
 *     └── Block Element (varies by culture)
 *         └── Property (varies by culture)
 *
 * Deleting a block and then saving only one language must not move the other language's
 * inner block values onto the block that took the deleted block's position.
 */

const documentTypeName = 'TestDocType';
const documentTypeGroupName = 'TestGroup';
const blockListName = 'InvariantBlockList';
const blockName = 'BlockElement';
const blockGroupName = 'BlockGroup';
const textStringName = 'Textstring';
const contentName = 'TestContent';
const englishCulture = 'en-US';
const danishCulture = 'da';

const firstEnglishText = 'first-english';
const secondEnglishText = 'second-english';
const firstDanishText = 'first-danish';
const secondDanishText = 'second-danish';
const secondEnglishTextEdited = 'second-english-edited';

let textStringDataTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.language.createDanishLanguage();
  const textStringDataType = await umbracoApi.dataType.getByName(textStringName);
  textStringDataTypeId = textStringDataType.id;
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockName);
  await umbracoApi.dataType.ensureNameNotExists(blockListName);
  await umbracoApi.language.ensureIsoCodeNotExists('da');
});

test('does not move inner block values onto another block when deleting a block and saving one language', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(
    blockName, blockGroupName, textStringName, textStringDataTypeId, true, true
  );
  const blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  // The document type varies by culture, the block list property does not - the combination under test.
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
    documentTypeName, blockListName, blockListId, documentTypeGroupName, true, false
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
      {[englishCulture]: firstEnglishText, [danishCulture]: firstDanishText},
      {[englishCulture]: secondEnglishText, [danishCulture]: secondDanishText},
    ]
  );

  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.doesBlockListPropertyHaveBlockAmount(documentTypeGroupName, blockListName, 2);

  // Act
  // Everything below happens in the default language, which is the only one that may edit an
  // invariant property under the default AllowEditInvariantFromNonDefault=false. The save
  // therefore covers en-US only, and the Danish values have to be carried over from the
  // persisted data - which is where the pairing goes wrong.

  // Delete the first block. This shrinks the contentData array ahead of the second block, and
  // has to happen in the same session as the save below - a reload resyncs the two arrays.
  await umbracoUi.content.clickDeleteBlockListBlockButtonAtIndex(0);
  await umbracoUi.content.clickConfirmToDeleteButton();
  await umbracoUi.content.doesBlockListPropertyHaveBlockAmount(documentTypeGroupName, blockListName, 1);

  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName, 0);
  await umbracoUi.content.enterTextstring(secondEnglishTextEdited);
  await umbracoUi.content.clickUpdateButton();
  // "Save..." opens the variant picker, which defaults to the active language only.
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickContainerSaveButtonAndWaitForContentToBeUpdated();
  await umbracoUi.reloadPage();

  // Assert
  // The document reopens in the default language, so check the English edit landed first.
  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName, 0);
  await umbracoUi.content.doesPropertyContainValue(textStringName, secondEnglishTextEdited);
  await umbracoUi.content.clickCloseButton();

  // The surviving block must still hold its own Danish value, not the deleted block's.
  await umbracoUi.content.switchLanguage(danishCulture);
  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName, 0);
  await umbracoUi.content.doesPropertyContainValue(textStringName, secondDanishText);
});
