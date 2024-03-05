import {AliasHelper, ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {BlockGridDataTypeBuilder} from "@umbraco/json-models-builders/dist/lib/builders/dataTypes";
import {expect} from "@playwright/test";

test.describe('BlockGridEditorDataTypeBlock', () => {
  const blockGridName = 'BlockGridEditorTest';
  const elementName = 'TestElement';
  const elementAlias = AliasHelper.toAlias(elementName);
  const elementNameTwo = 'SecondElement';
  const elementTwoAlias = AliasHelper.toAlias(elementNameTwo);
  const elementNameThree = 'ThirdElement';
  const elementThreeAlias = AliasHelper.toAlias(elementNameThree);

  test.beforeEach(async ({page, umbracoApi}, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridName);
  });

  test.afterEach(async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.dataTypes.ensureNameNotExists(blockGridName);
  });

  async function createDefaultBlockGridWithElement(umbracoApi) {
    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

    const blockGridType = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlock()
        .withContentElementTypeKey(element['key'])
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockGridType);

    return element;
  }

  async function createEmptyBlockGridWithName(umbracoApi) {
    const blockGridType = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .build();
    await umbracoApi.dataTypes.save(blockGridType);

    return blockGridType;
  }

  test('can create empty block grid editor', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoUi.goToSection(ConstantHelper.sections.settings);

    // Creates a new datatype
    await umbracoUi.clickDataElementByElementName('tree-item-dataTypes', {button: 'right'});
    await umbracoUi.clickDataElementByElementName(ConstantHelper.actions.create);
    await umbracoUi.clickDataElementByElementName(ConstantHelper.actions.dataType);

    await umbracoUi.setEditorHeaderName(blockGridName);

    // Adds BlockGrid as property editor
    await umbracoUi.clickDataElementByElementName('property-editor-add');
    await umbracoUi.clickDataElementByElementName('propertyeditor-', {hasText: 'Block Grid'});
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the blockGrid dataType was created
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await umbracoUi.doesDataTypeExist(blockGridName);
  });

  test('can create a block grid datatype with an element', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);

    await createEmptyBlockGridWithName(umbracoApi);
    await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

    await umbracoUi.navigateToDataType(blockGridName);

    // Adds an element to the block grid
    await umbracoUi.clickElement(umbracoUi.getButtonByKey('blockEditor_addBlockType'));
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementName + '"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submitChanges));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the element is added
    await expect(page.locator('umb-block-card', {hasText: elementName})).toBeVisible();
    // Checks if the datatype was created
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await umbracoUi.doesDataTypeExist(blockGridName);

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  });

  test('can create block grid datatype with two elements', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);

    await umbracoApi.documentTypes.createDefaultElementType(elementNameTwo, elementTwoAlias);

    await createDefaultBlockGridWithElement(umbracoApi);

    await umbracoUi.navigateToDataType(blockGridName);

    // Adds an element to the block grid
    await umbracoUi.clickElement(umbracoUi.getButtonByKey('blockEditor_addBlockType'));
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementNameTwo + '"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submitChanges));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the elements are added
    await expect(page.locator('umb-block-card', {hasText: elementName})).toBeVisible();
    await expect(page.locator('umb-block-card', {hasText: elementNameTwo})).toBeVisible();
    // Checks if the datatype was created
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await umbracoUi.doesDataTypeExist(blockGridName);

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
  });

  test('can create a block grid datatype with an element in a group', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);

    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    await createEmptyBlockGridWithName(umbracoApi);

    await umbracoUi.navigateToDataType(blockGridName);

    // Creates the group
    await umbracoUi.clickElement(umbracoUi.getButtonByKey('blockEditor_addBlockGroup'));
    await page.locator('[title="group name"]').fill('TestGroup');

    // Adds the element to the created group
    await page.locator('[key="blockEditor_addBlockType"]').nth(1).click();
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementName + '"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submitChanges));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the element is added to TestGroup
    await expect(page.locator('.umb-block-card-group').nth(1).locator('[data-content-element-type-key="' + element['key'] + '"]')).toBeVisible();
    // Checks if the datatype was created
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await umbracoUi.doesDataTypeExist(blockGridName);

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  });

  test('can create a block grid datatype with multiple elements in a group', async ({page, umbracoApi, umbracoUi}) => {
    const groupOne = 'GroupOne';

    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameThree);

    const elementOne = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    const elementTwo = await umbracoApi.documentTypes.createDefaultElementType(elementNameTwo, elementTwoAlias);
    const elementThree = await umbracoApi.documentTypes.createDefaultElementType(elementNameThree, elementThreeAlias);

    const blockGridType = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlockGroups()
        .withName(groupOne)
      .done()
      .addBlock()
        .withContentElementTypeKey(elementOne['key'])
        .withGroupName(groupOne)
      .done()
      .addBlock()
        .withContentElementTypeKey(elementTwo['key'])
        .withGroupName(groupOne)
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockGridType);

    await umbracoUi.navigateToDataType(blockGridName);

    // Adds the element to the created group
    await page.locator('[key="blockEditor_addBlockType"]').nth(1).click();
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementNameThree + '"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submitChanges));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the elements are added to GroupOne
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await expect(page.locator('.umb-block-card-group').nth(1).locator('[data-content-element-type-key="' + elementOne['key'] + '"]')).toBeVisible();
    await expect(page.locator('.umb-block-card-group').nth(1).locator('[data-content-element-type-key="' + elementTwo['key'] + '"]')).toBeVisible();
    await expect(page.locator('.umb-block-card-group').nth(1).locator('[data-content-element-type-key="' + elementThree['key'] + '"]')).toBeVisible();
    // Checks if the datatype was created
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await umbracoUi.doesDataTypeExist(blockGridName);

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameThree);
  });

  test('can create a block grid datatype with multiple groups with an element in each', async ({page, umbracoApi, umbracoUi}) => {
    const groupOne = 'GroupOne';
    const groupTwo = 'GroupTwo';

    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameThree);

    const elementOne = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    const elementTwo = await umbracoApi.documentTypes.createDefaultElementType(elementNameTwo, elementTwoAlias);
    const elementThree = await umbracoApi.documentTypes.createDefaultElementType(elementNameThree, elementThreeAlias);

    const blockGridType = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlockGroups()
        .withName(groupOne)
      .done()
      .addBlockGroups()
        .withName(groupTwo)
      .done()
      .addBlock()
        .withContentElementTypeKey(elementOne['key'])
      .done()
      .addBlock()
        .withContentElementTypeKey(elementTwo['key'])
        .withGroupName(groupOne)
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockGridType);

    await umbracoUi.navigateToDataType(blockGridName);

    // Adds another element to GroupTwo
    // We need to have a nth because all the add block groups are the same in the html
    await page.locator('[key="blockEditor_addBlockType"]').nth(2).click();
    await umbracoUi.clickDataElementByElementName("tree-item-" + elementNameThree);
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submitChanges));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the elements is are added to their correct groups
    await expect(page.locator('.umb-block-card-group').nth(0).locator('[data-content-element-type-key="' + elementOne['key'] + '"]')).toBeVisible();
    await expect(page.locator('.umb-block-card-group').nth(1).locator('[data-content-element-type-key="' + elementTwo['key'] + '"]')).toBeVisible();
    await expect(page.locator('.umb-block-card-group').nth(2).locator('[data-content-element-type-key="' + elementThree['key'] + '"]')).toBeVisible();
    // Checks if the datatype was created
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await umbracoUi.doesDataTypeExist(blockGridName);

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameThree);
  });

  test('can create a block grid datatype with multiple groups and multiple element in each group', async ({page, umbracoApi, umbracoUi},testInfo) => {
    await testInfo.slow();

    const GroupOne = 'GroupOne';
    const elementNameFourth = 'FourthElement';
    const elementFourthAlias = AliasHelper.toAlias(elementNameFourth);
    const elementNameFifth = 'FifthElement';
    const elementFifthAlias = AliasHelper.toAlias(elementNameFifth);
    const elementNameSixth = 'SixthElement';
    const elementSixthAlias = AliasHelper.toAlias(elementNameSixth);

    const GroupTwo = 'GroupTwo';
    const elementNameSeventh = 'SeventhElement';
    const elementSeventhAlias = AliasHelper.toAlias(elementNameSeventh);
    const elementNameEighth = 'EightElement';
    const elementEighthAlias = AliasHelper.toAlias(elementNameEighth);
    const elementNameNinth = 'NinthElement';
    const elementNinthAlias = AliasHelper.toAlias(elementNameNinth);

    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameThree);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameFourth);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameFifth);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameSixth);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameSeventh);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameEighth);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameNinth);

    const elementOne = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    const elementTwo = await umbracoApi.documentTypes.createDefaultElementType(elementNameTwo, elementTwoAlias);
    const elementThree = await umbracoApi.documentTypes.createDefaultElementType(elementNameThree, elementThreeAlias);
    const elementFour = await umbracoApi.documentTypes.createDefaultElementType(elementNameFourth, elementFourthAlias);
    const elementFive = await umbracoApi.documentTypes.createDefaultElementType(elementNameFifth, elementFifthAlias);
    const elementSix = await umbracoApi.documentTypes.createDefaultElementType(elementNameSixth, elementSixthAlias);
    const elementSeven = await umbracoApi.documentTypes.createDefaultElementType(elementNameSeventh, elementSeventhAlias);
    const elementEight = await umbracoApi.documentTypes.createDefaultElementType(elementNameEighth, elementEighthAlias);
    const elementNine = await umbracoApi.documentTypes.createDefaultElementType(elementNameNinth, elementNinthAlias);

    const blockGridType = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlockGroups()
        .withName(GroupOne)
      .done()
      .addBlockGroups()
        .withName(GroupTwo)
      .done()
      .addBlock()
        .withContentElementTypeKey(elementOne['key'])
      .done()
      .addBlock()
        .withContentElementTypeKey(elementTwo['key'])
      .done()
      .addBlock()
        .withContentElementTypeKey(elementThree['key'])
      .done()
      .addBlock()
        .withContentElementTypeKey(elementFour['key'])
        .withGroupName(GroupOne)
      .done()
      .addBlock()
        .withContentElementTypeKey(elementFive['key'])
        .withGroupName(GroupOne)
      .done()
      .addBlock()
        .withContentElementTypeKey(elementSix['key'])
        .withGroupName(GroupOne)
      .done()
      .addBlock()
        .withContentElementTypeKey(elementSeven['key'])
        .withGroupName(GroupTwo)
      .done()
      .addBlock()
        .withContentElementTypeKey(elementEight['key'])
        .withGroupName(GroupTwo)
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockGridType);

    await umbracoUi.navigateToDataType(blockGridName);

    // Adds the ninth element to GroupTwo
    await page.locator('[key="blockEditor_addBlockType"]').nth(2).click();
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementNameNinth + '"]').click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submitChanges));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the elements is are added to their correct groups
    await expect(page.locator('.umb-block-card-group').nth(0).locator('[data-content-element-type-key="' + elementOne['key'] + '"]')).toBeVisible();
    await expect(page.locator('.umb-block-card-group').nth(0).locator('[data-content-element-type-key="' + elementTwo['key'] + '"]')).toBeVisible();
    await expect(page.locator('.umb-block-card-group').nth(0).locator('[data-content-element-type-key="' + elementThree['key'] + '"]')).toBeVisible();
    await expect(page.locator('.umb-block-card-group').nth(1).locator('[data-content-element-type-key="' + elementFour['key'] + '"]')).toBeVisible();
    await expect(page.locator('.umb-block-card-group').nth(1).locator('[data-content-element-type-key="' + elementFive['key'] + '"]')).toBeVisible();
    await expect(page.locator('.umb-block-card-group').nth(1).locator('[data-content-element-type-key="' + elementSix['key'] + '"]')).toBeVisible();
    await expect(page.locator('.umb-block-card-group').nth(2).locator('[data-content-element-type-key="' + elementSeven['key'] + '"]')).toBeVisible();
    await expect(page.locator('.umb-block-card-group').nth(2).locator('[data-content-element-type-key="' + elementEight['key'] + '"]')).toBeVisible();
    await expect(page.locator('.umb-block-card-group').nth(2).locator('[data-content-element-type-key="' + elementNine['key'] + '"]')).toBeVisible();
    // Checks if the datatype was created
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await umbracoUi.doesDataTypeExist(blockGridName);

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameThree);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameFourth);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameFifth);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameSixth);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameSeventh);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameEighth);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameNinth);
  });

  test('cant add an element which already exists in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);

    const element = await createDefaultBlockGridWithElement(umbracoApi);

    await umbracoUi.navigateToDataType(blockGridName);

    // Tries to add the same element to the block grid
    await umbracoUi.clickElement(umbracoUi.getButtonByKey('blockEditor_addBlockType'));
    await page.locator('[data-element="editor-container"]').locator('[data-element="tree-item-' + elementName + '"]').click();

    // Assert
    await expect(page.locator('.not-allowed', {hasText: elementName})).toBeVisible();
    // Checks if the button create New Element Type is still visible. If visible the element was not clickable.
    await expect(page.locator('[label-key="blockEditor_labelcreateNewElementType"]')).toBeVisible();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.close));
    await expect(page.locator('.umb-block-card-group').nth(0).locator('[data-content-element-type-key="' + element['key'] + '"]')).toHaveCount(1);
    // Checks if the datatype was created
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await umbracoUi.doesDataTypeExist(blockGridName);

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  });

  test('can remove an element from a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);

    const element = await createDefaultBlockGridWithElement(umbracoApi);

    await umbracoUi.navigateToDataType(blockGridName);

    // Removes the element
    await page.locator('[data-content-element-type-key="' + element['key'] + '"]').locator('.btn-reset').click();
    // Cant use the constant key because the constant key is "action-delete"
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey("actions_delete"));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks to make sure the element is removed
    await expect(page.locator('[data-content-element-type-key="' + element['key'] + '"]')).not.toBeVisible();
    // Checks if the datatype was created
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await umbracoUi.doesDataTypeExist(blockGridName);

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  });

  test('can delete a group without elements from a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
    const Group = "GroupToBeDeleted";

    const blockGridType = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlockGroups()
        .withName(Group)
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockGridType);

    await umbracoUi.navigateToDataType(blockGridName);

    // Removes the empty group
    await page.locator('.umb-block-card-group').nth(1).locator('[title="Delete"]').click();
    // Cant use the constant key because the correct constant key is "action-delete"
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey("actions_delete"));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks to make sure the element is removed
    await expect(page.locator('.umb-block-card-group').nth(1)).not.toBeVisible();
    // Checks if the datatype exists
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await umbracoUi.doesDataTypeExist(blockGridName);
  });

  test('can delete a group with elements from a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);

    const elementOne = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    const elementTwo = await umbracoApi.documentTypes.createDefaultElementType(elementNameTwo, elementTwoAlias);

    const Group = "GroupToBeDeleted";

    const blockGridType = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlockGroups()
        .withName(Group)
      .done()
      .addBlock()
        .withContentElementTypeKey(elementOne['key'])
        .withGroupName(Group)
      .done()
      .addBlock()
        .withContentElementTypeKey(elementTwo['key'])
        .withGroupName(Group)
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockGridType);

    await umbracoUi.navigateToDataType(blockGridName);

    // Removes the group with elements
    await page.locator('.umb-block-card-group').nth(1).locator('[title="Delete"]').click();
    // Cant use the constant key because the correct constant key is "action-delete"
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey("actions_delete"));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks to make sure the element is removed
    await expect(page.locator('.umb-block-card-group').nth(1)).not.toBeVisible();
    await expect(page.locator('[data-content-element-type-key="' + elementOne['key'] + '"]')).not.toBeVisible();
    await expect(page.locator('[data-content-element-type-key="' + elementTwo['key'] + '"]')).not.toBeVisible();
    // Checks if the datatype exists
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await umbracoUi.doesDataTypeExist(blockGridName);

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
  });

  test('can delete an empty block grid editor', async ({page, umbracoApi, umbracoUi}) => {
    const blockGridType = await createEmptyBlockGridWithName(umbracoApi);

    await umbracoUi.goToSection(ConstantHelper.sections.settings);

    // Deletes the empty block grid editor
    await page.locator('[data-element="tree-item-dataTypes"]').locator('[data-element="tree-item-expand"]').click();
    await umbracoUi.clickDataElementByElementName("tree-item-" + blockGridName, {button: 'right'});
    await umbracoUi.clickDataElementByElementName(ConstantHelper.actions.delete);
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.delete));

    // Assert
    // Checks if the block grid editor still exists
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.clickDataElementByElementName('tree-item-dataTypes', {button: "right"});
    await umbracoUi.clickDataElementByElementName('action-refreshNode');
    await expect(page.locator('[data-element="tree-item-dataTypes"] >> [data-element="tree-item-' + blockGridType + '"]')).not.toBeVisible();
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(false);
  });

  test('can delete an block grid editor with elements and groups', async ({page, umbracoApi, umbracoUi}) => {
    const groupOne = 'GroupOne';

    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);

    const elementOne = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    const elementTwo = await umbracoApi.documentTypes.createDefaultElementType(elementNameTwo, elementTwoAlias);

    const blockGridType = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlockGroups()
        .withName(groupOne)
      .done()
      .addBlock()
        .withContentElementTypeKey(elementOne['key'])
      .done()
      .addBlock()
        .withContentElementTypeKey(elementTwo['key'])
        .withGroupName(groupOne)
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockGridType);

    await umbracoUi.goToSection(ConstantHelper.sections.settings);

    // Deletes the empty block grid editor
    await page.locator('[data-element="tree-item-dataTypes"]').locator('[data-element="tree-item-expand"]').click();
    await umbracoUi.clickDataElementByElementName("tree-item-" + blockGridName, {button: 'right'});
    await umbracoUi.clickDataElementByElementName(ConstantHelper.actions.delete);
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.delete));

    // We need a wait to make sure the block grid editor is deleted
    await page.waitForTimeout(1000);

    // Assert
    // Checks if the block grid editor still exists
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.clickDataElementByElementName('tree-item-dataTypes', {button: "right"});
    await umbracoUi.clickDataElementByElementName('action-refreshNode');
    await expect(page.locator('[data-element="tree-item-dataTypes"] >> [data-element="tree-item-' + blockGridType + '"]')).not.toBeVisible();
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(false);
  });

  test('can move an element in a block grid editor to another group', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);

    const GroupMoveHere = 'MoveToHere';

    const element = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);

    const blockGridType = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlockGroups()
        .withName(GroupMoveHere)
      .done()
      .addBlock()
        .withContentElementTypeKey(element['key'])
        .withLabel('Moved')
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockGridType);

    await umbracoUi.navigateToDataType(blockGridName);

    // Drags the element from the default group to the 'MoveToHere' Group.
    const dragFrom = await page.locator('.umb-block-card-group').nth(0).locator('[data-content-element-type-key="' + element['key'] + '"]');
    const dragTo = await page.locator('[key="blockEditor_addBlockType"]').nth(1);
    await umbracoUi.dragAndDrop(dragFrom, dragTo, 0, 0, 15);
// We need a wait to make sure the element is moved
    await page.waitForTimeout(2000);
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the elements was moved to the correct group
    await expect(page.locator('.umb-block-card-group').nth(1).locator('[data-content-element-type-key="' + element['key'] + '"]')).toBeVisible();
    // Checks if the element still contains the correct text
    await page.locator('.umb-block-card-group').nth(1).locator('[data-content-element-type-key="' + element['key'] + '"]').click();
    await expect(page.locator('input[name="label"]')).toHaveValue('Moved');
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.close));
    // Checks if the datatype was created
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await umbracoUi.doesDataTypeExist(blockGridName);

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
  });

  test('can move an empty group in a block grid editor', async ({page, umbracoApi, umbracoUi}) => {
    const GroupMove = 'GroupMove';
    const GroupNotMoving = 'GroupNotMoving';

    const blockGridType = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlockGroups()
        .withName(GroupMove)
      .done()
      .addBlockGroups()
        .withName(GroupNotMoving)
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockGridType);

    await umbracoUi.navigateToDataType(blockGridName);

    // Drags the group GroupMove under GroupNotMoving
    const dragFrom = await page.locator('.umb-block-card-group >> [icon="icon-navigation"]').nth(0);
    const dragTo = await page.locator('[key="blockEditor_addBlockType"]').nth(2);
    await umbracoUi.dragAndDrop(dragFrom, dragTo, 0, 0, 15);

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the groups are in the correct order
    // The reason that we are checking nth(0) and not 1, is because the default group has no input.
    await expect(page.locator('.umb-block-card-group >> input[title="group name"]').nth(0)).toHaveValue(GroupNotMoving);
    await expect(page.locator('.umb-block-card-group >> input[title="group name"]').nth(1)).toHaveValue(GroupMove);
    // Checks if the datatype was created
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await umbracoUi.doesDataTypeExist(blockGridName);
  });

  test('can move a group with elements in a block grid editor', async ({page, umbracoApi, umbracoUi}, testInfo) => {
    await testInfo.slow();

    const GroupMove = 'GroupMove';
    const GroupNotMoving = 'GroupNotMoving';

    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameThree);

    const elementOne = await umbracoApi.documentTypes.createDefaultElementType(elementName, elementAlias);
    const elementTwo = await umbracoApi.documentTypes.createDefaultElementType(elementNameTwo, elementTwoAlias);
    const elementThree = await umbracoApi.documentTypes.createDefaultElementType(elementNameThree, elementThreeAlias);

    const blockGridType = new BlockGridDataTypeBuilder()
      .withName(blockGridName)
      .addBlockGroups()
        .withName(GroupMove)
      .done()
      .addBlockGroups()
        .withName(GroupNotMoving)
      .done()
      .addBlock()
        .withContentElementTypeKey(elementOne['key'])
        .withLabel('MovedOne')
        .withGroupName(GroupMove)
      .done()
      .addBlock()
        .withContentElementTypeKey(elementTwo['key'])
        .withLabel('MovedTwo')
        .withGroupName(GroupMove)
      .done()
      .addBlock()
        .withContentElementTypeKey(elementThree['key'])
        .withLabel('MovedThree')
        .withGroupName(GroupNotMoving)
      .done()
      .build();
    await umbracoApi.dataTypes.save(blockGridType);
    await umbracoUi.navigateToDataType(blockGridName);

    // Drags the group GroupMove under GroupNotMoving
    const dragFrom = await page.locator('.umb-block-card-group >> [icon="icon-navigation"]').nth(0);
    const dragTo = await page.locator('[key="blockEditor_addBlockType"]').nth(2);
    await umbracoUi.dragAndDrop(dragFrom, dragTo, 20, 0, 15);

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    // Checks if the elements were moved with their group
    await expect(page.locator('.umb-block-card-group').nth(1).locator('[data-content-element-type-key="' + elementThree['key'] + '"]')).toBeVisible();
    await expect(page.locator('.umb-block-card-group').nth(2).locator('[data-content-element-type-key="' + elementOne['key'] + '"]')).toBeVisible();
    await expect(page.locator('.umb-block-card-group').nth(2).locator('[data-content-element-type-key="' + elementTwo['key'] + '"]')).toBeVisible();
    // Checks if the moved elements still contains the correct text
    // ElementThree in GroupNotMoving
    await page.locator('.umb-block-card-group').nth(1).locator('[data-content-element-type-key="' + elementThree['key'] + '"]').click();
    await expect(page.locator('input[name="label"]')).toHaveValue('MovedThree');
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.close));
    // ElementOne in GroupMove
    await page.locator('.umb-block-card-group').nth(2).locator('[data-content-element-type-key="' + elementOne['key'] + '"]').click();
    await expect(page.locator('input[name="label"]')).toHaveValue('MovedOne');
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.close));
    // ElementTwo in GroupMove
    await page.locator('.umb-block-card-group').nth(2).locator('[data-content-element-type-key="' + elementTwo['key'] + '"]').click();
    await expect(page.locator('input[name="label"]')).toHaveValue('MovedTwo');
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.close));
    // Checks if the datatype was created
    await expect(await umbracoApi.dataTypes.exists(blockGridName)).toBe(true);
    await umbracoUi.doesDataTypeExist(blockGridName);

    // Clean
    await umbracoApi.documentTypes.ensureNameNotExists(elementName);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameTwo);
    await umbracoApi.documentTypes.ensureNameNotExists(elementNameThree);
  });
});
