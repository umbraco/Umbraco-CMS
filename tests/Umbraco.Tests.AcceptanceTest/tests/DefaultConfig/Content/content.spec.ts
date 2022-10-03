import {AliasHelper, ApiHelpers, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {
  ContentBuilder,
  DocumentTypeBuilder,
  PartialViewMacroBuilder,
  MacroBuilder,
  GridDataTypeBuilder
} from "@umbraco/json-models-builders";

test.describe('Content tests', () => {

  test.beforeEach(async ({page, umbracoApi}) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });
  
  async function createSimpleMacro(name, umbracoApi: ApiHelpers){
    const insertMacro = new PartialViewMacroBuilder()
      .withName(name)
      .withContent(`@inherits Umbraco.Cms.Web.Common.Macros.PartialViewMacroPage
<h1>Acceptance test</h1>`)
      .build();

    const macroWithPartial = new MacroBuilder()
      .withName(name)
      .withPartialViewMacro(insertMacro)
      .withRenderInEditor()
      .withUseInEditor()
      .build();

    await umbracoApi.macros.saveWithPartial(macroWithPartial);
  }

  test('Copy content', async ({ page, umbracoApi, umbracoUi }) => {
    const rootDocTypeName = "Test document type";
    const childDocTypeName = "Child test document type";
    const firstRootNodeName = "1) Home";
    const childNodeName = "1) Child";
    const secondRootNodeName = "2) Home";

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(childDocTypeName);

    const childDocType = new DocumentTypeBuilder()
        .withName(childDocTypeName)
        .build()

    const createdChildDocType = await umbracoApi.documentTypes.save(childDocType);

    const rootDocType = new DocumentTypeBuilder()
      .withName(rootDocTypeName)
      .withAllowAsRoot(true)
      .withAllowedContentTypes(createdChildDocType.id)
      .build();

    const createdRootDocType = await umbracoApi.documentTypes.save(rootDocType);

    // TODO: Make some constants for actions.
    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(createdRootDocType.alias)
      .withAction("saveNew")
      .addVariant()
        .withName(firstRootNodeName)
        .withSave(true)  // We should probably just default to true...
      .done()
      .build();

    const savedRootNode = await umbracoApi.content.save(rootContentNode);

    const secondRootNode = new ContentBuilder()
      .withContentTypeAlias(createdRootDocType.alias)
      .withAction("saveNew")
      .addVariant()
        .withName(secondRootNodeName)
        .withSave(true)
      .done()
      .build();

    await umbracoApi.content.save(secondRootNode);

    const childContentNode = new ContentBuilder()
      .withContentTypeAlias(createdChildDocType.alias)
      .withAction("saveNew")
      .withParent(savedRootNode.id)
      .addVariant()
        .withName(childNodeName)
        .withSave(true)
      .done()
      .build();

    await umbracoApi.content.save(childContentNode);

    await umbracoUi.refreshContentTree();

    await umbracoUi.clickElement(umbracoUi.getTreeItem("content", [firstRootNodeName, childNodeName]), {button: "right", force: true})
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.copy))
    await page.locator('.umb-pane [data-element="tree-item-' + secondRootNodeName + '"]').click();
    await page.locator('.umb-dialog-footer > .btn-primary').click();
    await expect(page.locator('.alert-success')).toBeVisible();

    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(childDocTypeName);
  });

  test('Move content', async ({ page, umbracoApi, umbracoUi }) => {
    const rootDocTypeName = "Test document type";
    const childDocTypeName = "Child test document type";
    const firstRootNodeName = "1) Home";
    const childNodeName = "1) Child";
    const secondRootNodeName = "2) Home";

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(childDocTypeName);

    const childDocType = new DocumentTypeBuilder()
        .withName(childDocTypeName)
        .build()

    const createdChildDocType = await umbracoApi.documentTypes.save(childDocType);

    const rootDocType = new DocumentTypeBuilder()
        .withName(rootDocTypeName)
        .withAllowAsRoot(true)
        .withAllowedContentTypes(createdChildDocType.id)
        .build();

    const createdRootDocType = await umbracoApi.documentTypes.save(rootDocType);

    const rootContentNode = new ContentBuilder()
        .withContentTypeAlias(createdRootDocType.alias)
        .withAction("saveNew")
        .addVariant()
          .withName(firstRootNodeName)
          .withSave(true)  // We should probably just default to true...
        .done()
        .build();

    const savedRootNode = await umbracoApi.content.save(rootContentNode);

    const secondRootNode = new ContentBuilder()
        .withContentTypeAlias(createdRootDocType.alias)
        .withAction("saveNew")
        .addVariant()
          .withName(secondRootNodeName)
          .withSave(true)
        .done()
        .build();

    await umbracoApi.content.save(secondRootNode);

    const childContentNode = new ContentBuilder()
        .withContentTypeAlias(createdChildDocType.alias)
        .withAction("saveNew")
        .withParent(savedRootNode.id)
        .addVariant()
          .withName(childNodeName)
          .withSave(true)
        .done()
        .build();

    await umbracoApi.content.save(childContentNode);

    await umbracoUi.refreshContentTree();

    await umbracoUi.clickElement(umbracoUi.getTreeItem("content", [firstRootNodeName, childNodeName]), { button: "right", force: true });
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.move))
    await page.locator('.umb-pane [data-element="tree-item-' + secondRootNodeName + '"]').click()
    await page.locator('[key="actions_move"]').click();

    await expect(page.locator('.alert-success')).toBeVisible();

    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(childDocTypeName);
  });

  test('Sort content', async ({ page, umbracoApi, umbracoUi }) => {
    const rootDocTypeName = "Test document type";
    const childDocTypeName = "Child test document type";
    const rootNodeName = "1) Home";
    const firstChildNodeName = "1) Child";
    const secondChildNodeName = "2) Child";

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(childDocTypeName);

    const childDocType = new DocumentTypeBuilder()
        .withName(childDocTypeName)
        .build();
    const createdChildDocType = await umbracoApi.documentTypes.save(childDocType);

    const rootDocType = new DocumentTypeBuilder()
        .withName(rootDocTypeName)
        .withAllowAsRoot(true)
        .withAllowedContentTypes(createdChildDocType.id)
        .build();
    const createdRootDocType = await umbracoApi.documentTypes.save(rootDocType);

    const rootContentNode = new ContentBuilder()
        .withContentTypeAlias(createdRootDocType.alias)
        .withAction("saveNew")
        .addVariant()
          .withName(rootNodeName)
          .withSave(true)
        .done()
        .build();
    const createdRootContentNode = await umbracoApi.content.save(rootContentNode);

    // Add an item under root node
    const firstChildContentNode = new ContentBuilder()
        .withContentTypeAlias(createdChildDocType.alias)
        .withAction("saveNew")
        .withParent(createdRootContentNode.id)
        .addVariant()
          .withName(firstChildNodeName)
          .withSave(true)
        .done()
        .build();
    await umbracoApi.content.save(firstChildContentNode);

    // Add a second item under root node
    const secondChildContentNode = new ContentBuilder()
        .withContentTypeAlias(createdChildDocType.alias)
        .withAction("saveNew")
        .withParent(createdRootContentNode.id)
        .addVariant()
          .withName(secondChildNodeName)
          .withSave(true)
        .done()
        .build();
    await umbracoApi.content.save(secondChildContentNode);

    await umbracoUi.refreshContentTree();
    await umbracoUi.clickElement(umbracoUi.getTreeItem("content", [rootNodeName]), { button: "right", force: true });
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.sort));
    // Drag'n'drop second child to be the first one.
    await page.locator('.ui-sortable-handle >> text=' + secondChildNodeName).hover();
    await page.mouse.down()
    await page.locator('.ui-sortable-handle >> text=' + firstChildNodeName).hover();
    await page.mouse.up();

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.close));

    const childNodes = await page.locator('[node="child"]');
    await expect(childNodes.first()).toContainText(secondChildNodeName);
    await expect(childNodes.nth(2)).toContainText(firstChildNodeName);

    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(childDocTypeName);
  });

  test('Rollback content', async ({ page, umbracoApi, umbracoUi }) => {
    const rootDocTypeName = "Test document type";
    const initialNodeName = "Home node";
    const newNodeName = "Home";

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);

    const rootDocType = new DocumentTypeBuilder()
        .withName(rootDocTypeName)
        .withAllowAsRoot(true)
        .build();
    const createdDocType = await umbracoApi.documentTypes.save(rootDocType);

    const rootContentNode = new ContentBuilder()
        .withContentTypeAlias(createdDocType.alias)
        .addVariant()
          .withName(initialNodeName)
          .withSave(true)
        .done()
        .build();
    await umbracoApi.content.save(rootContentNode);

    await umbracoUi.refreshContentTree();
    await umbracoUi.clickElement(umbracoUi.getTreeItem("content", [initialNodeName]));

    const header = await page.locator('#headerName')
    // Sadly playwright doesn't have a clear method for inputs :( 
    // so we have to triple click to select all, and then hit backspace...
    await header.click({ clickCount: 3 })
    await page.keyboard.press('Backspace');

    await umbracoUi.setEditorHeaderName(newNodeName);
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    await umbracoUi.isSuccessNotificationVisible();

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.rollback));
    // Not a very nice selector, but there's sadly no alternative :(
    await page.locator('.-selectable.cursor-pointer').first().click();
    // Sadly can't use the button by label key here since there's another one in the DOM 
    await page.locator('[action="vm.rollback()"]').click();

    await umbracoUi.refreshContentTree();
    await expect(page.locator('.umb-badge >> text=Save')).toBeVisible();
    await expect(page.locator('.umb-badge >> text=RollBack')).toBeVisible();
    const node = await umbracoUi.getTreeItem("content", [initialNodeName])
    await expect(node).toBeVisible();
  });

  test('View audit trail', async ({ page, umbracoApi, umbracoUi }) => {
    const rootDocTypeName = "Test document type";
    const nodeName = "Home";
    const labelName = "Name";

    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
    await umbracoApi.content.deleteAllContent();

    const rootDocType = new DocumentTypeBuilder()
      .withName(rootDocTypeName)
      .withAllowAsRoot(true)
        .addGroup()
          .addTextBoxProperty()
          .withLabel(labelName)
        .done()
      .done()
      .build();

    const generatedRootDocType = await umbracoApi.documentTypes.save(rootDocType)

    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(generatedRootDocType["alias"])
      .addVariant()
        .withName(nodeName)
        .withSave(true)
      .done()
      .build();

    await umbracoApi.content.save(rootContentNode);

    // Refresh to update the tree
    await umbracoUi.refreshContentTree();

    // Access node
    await umbracoUi.clickElement(umbracoUi.getTreeItem('content', [nodeName]));

    // Navigate to Info app
    await page.locator(ConstantHelper.contentApps.info).click();

    // Assert
    await expect(await page.locator('.history')).toBeDefined();

    // Clean up (content is automatically deleted when document types are gone)
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
  });

  test('Save draft', async ({ page, umbracoApi, umbracoUi }) => {
    const rootDocTypeName = "Test document type";
    const nodeName = "Home";
    const expected = "Unpublished";

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);

    const rootDocType = new DocumentTypeBuilder()
      .withName(rootDocTypeName)
      .withAllowAsRoot(true)
      .build();

    const generatedRootDocType = await umbracoApi.documentTypes.save(rootDocType);

    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(generatedRootDocType["alias"])
      .withAction("saveNew")
      .addVariant()
        .withName(nodeName)
        .withSave(true)
      .done()
      .build();

    await umbracoApi.content.save(rootContentNode)

    // Refresh to update the tree
    await umbracoUi.refreshContentTree();

    // Access node
    await umbracoUi.clickElement(umbracoUi.getTreeItem('content', [nodeName]));

    // Assert
    await expect(page.locator('[data-element="node-info-status"]').locator('.umb-badge')).toContainText(expected);

    // Clean up (content is automatically deleted when document types are gone)
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
  });  

  test('Preview draft', async ({ page, umbracoApi, umbracoUi }) => {
    const rootDocTypeName = "Test document type";
    const nodeName = "Home";

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);

    const rootDocType = new DocumentTypeBuilder()
      .withName(rootDocTypeName)
      .withAllowAsRoot(true)
      .build();

    const generatedRootDocType = await umbracoApi.documentTypes.save(rootDocType);

    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(generatedRootDocType["alias"])
      .withAction("saveNew")
        .addVariant()
        .withName(nodeName)
        .withSave(true)
      .done()
      .build();

    await umbracoApi.content.save(rootContentNode)

    // Refresh to update the tree
    await umbracoUi.refreshContentTree();

    // Access node
    await umbracoUi.clickElement(umbracoUi.getTreeItem('content', [nodeName]));

    // Assert
    await expect(page.locator('[alias="preview"]')).toBeVisible();
    await page.locator('[alias="preview"]').click();
    await umbracoUi.isSuccessNotificationVisible();

    // Clean up (content is automatically deleted when document types are gone)
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
  });  

  test('Publish draft', async ({ page, umbracoApi, umbracoUi }) => {
    const rootDocTypeName = "Test document type";
    const nodeName = "Home";
    const expected = "Published";

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);

    const rootDocType = new DocumentTypeBuilder()
      .withName(rootDocTypeName)
      .withAllowAsRoot(true)
      .build();

    const generatedRootDocType = await umbracoApi.documentTypes.save(rootDocType);

    const rootContentNode = new ContentBuilder()
      .withContentTypeAlias(generatedRootDocType["alias"])
      .addVariant()
        .withName(nodeName)
        .withSave(true)
      .done()
      .build();

    await umbracoApi.content.save(rootContentNode)

    // Refresh to update the tree
    await umbracoUi.refreshContentTree();

    // Access node
    await umbracoUi.clickElement(umbracoUi.getTreeItem('content', [nodeName]));

    // Assert
    await expect(page.locator('[data-element="node-info-status"]').locator('.umb-badge')).toContainText(expected);

    // Clean up (content is automatically deleted when document types are gone)
    await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
  });

  test('Content with contentpicker', async ({ page, umbracoApi, umbracoUi }) => {
    const pickerDocTypeName = 'Content picker doc type';
    const pickerDocTypeAlias = AliasHelper.toAlias(pickerDocTypeName);
    const pickedDocTypeName = 'Picked content document type';
    const pickedDocTypeAlias = AliasHelper.toAlias(pickedDocTypeName);

    await umbracoApi.content.deleteAllContent();
    await umbracoApi.documentTypes.ensureNameNotExists(pickerDocTypeName);
    await umbracoApi.templates.ensureNameNotExists(pickerDocTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(pickedDocTypeName);

    // Create the content type and content we'll be picking from.
    const pickedDocType = new DocumentTypeBuilder()
      .withName(pickedDocTypeName)
      .withAlias(pickedDocTypeAlias)
      .withAllowAsRoot(true)
      .addGroup()
      .addTextBoxProperty()
      .withAlias('text')
      .done()
      .done()
      .build();

    const generatedType = await  umbracoApi.documentTypes.save(pickedDocType)
    const pickedContentNode = new ContentBuilder()
      .withContentTypeAlias(generatedType["alias"])
      .withAction("publishNew")
      .addVariant()
        .withName('Content to pick')
        .withSave(true)
        .withPublish(true)
      .addProperty()
        .withAlias('text')
        .withValue('Acceptance test')
      .done()
      .withSave(true)
        .withPublish(true)
      .done()
      .build();
    await umbracoApi.content.save(pickedContentNode);

    // Create the doctype with a the picker
    const pickerDocType = new DocumentTypeBuilder()
      .withName(pickerDocTypeName)
      .withAlias(pickerDocTypeAlias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(pickerDocTypeAlias)
      .addGroup()
        .withName('ContentPickerGroup')
        .addContentPickerProperty()
          .withAlias('picker')
        .done()
      .done()
      .build();

    await umbracoApi.documentTypes.save(pickerDocType);

    // Ugly wait, but we have to wait for cache to rebuild
    await page.waitForTimeout(1000);
    // Edit it the template to allow us to verify the rendered view.
    await umbracoApi.templates.edit(pickerDocTypeName, `@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentPickerDocType>
        @{
            Layout = null;
            var pickedItem = Model.Picker as PickedContentDocumentType;
        }

        <p>@pickedItem.Text</p>`);

    // Create content with content picker
    await page.locator('.umb-tree-root-link').click({ button: "right"});
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.create));
    await page.locator('[data-element="action-create-' + pickerDocTypeAlias + '"] > .umb-action-link').click();

    // Fill out content
    await umbracoUi.setEditorHeaderName('ContentPickerContent')
    await page.locator('.umb-node-preview-add').click();

    // Should really try and find a better way to do this, but umbracoTreeItem tries to click the content pane in the background
    await page.locator('[ng-if="vm.treeReady"] > .umb-tree .umb-tree-item__inner').click();

    // We have to wait for the picked content to show up or it wont be added.
    await expect(await page.locator('.umb-node-preview__description')).toBeVisible();

    // Save and publish
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    await umbracoUi.isSuccessNotificationVisible();

    // Assert
    const expectedContent = '<p>Acceptance test</p>'
    await expect(await umbracoApi.content.verifyRenderedContent('/contentpickercontent', expectedContent, true)).toBeTruthy();

    // Clean up
    await umbracoApi.documentTypes.ensureNameNotExists(pickerDocTypeName);
    await umbracoApi.templates.ensureNameNotExists(pickerDocTypeName);
    await umbracoApi.documentTypes.ensureNameNotExists(pickedDocTypeName);
  });

  test('Content with macro in RTE', async ({ page, umbracoApi, umbracoUi }) => {
    const viewMacroName = 'Content with macro in RTE';
    const partialFileName = viewMacroName + '.cshtml';

    await umbracoApi.macros.ensureNameNotExists(viewMacroName);
    await umbracoApi.partialViews.ensureMacroFileNameNotExists(partialFileName);
    await umbracoApi.documentTypes.ensureNameNotExists(viewMacroName);
    await umbracoApi.templates.ensureNameNotExists(viewMacroName);
    await umbracoApi.content.deleteAllContent();

    // First thing first we got to create the macro we will be inserting
    await createSimpleMacro(viewMacroName, umbracoApi);

    // Now we need to create a document type with a rich text editor where we can insert the macro
    // The document type must have a template as well in order to ensure that the content is displayed correctly
    const alias = AliasHelper.toAlias(viewMacroName);
    const docType = new DocumentTypeBuilder()
      .withName(viewMacroName)
      .withAlias(alias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(alias)
      .addGroup()
        .addRichTextProperty()
          .withAlias('text')
        .done()
      .done()
      .build();

    const generatedDocType = await umbracoApi.documentTypes.save(docType)
      // Might as wel initally create the content here, the less GUI work during the test the better
    const contentNode = new ContentBuilder()
      .withContentTypeAlias(generatedDocType["alias"])
      .withAction('saveNew')
        .addVariant()
          .withName(viewMacroName)
          .withSave(true)
        .done()
      .build();

    await umbracoApi.content.save(contentNode);

    // Ugly wait but we have to wait for cache to rebuild
    await page.waitForTimeout(1000);

    // Edit the macro template in order to have something to verify on when rendered.
    await umbracoApi.templates.edit(viewMacroName, `@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
@{
    Layout = null;
}
@{
    if (Model.HasValue("text")){
        @(Model.Value("text"))
    }
} `);

    // Enter content
    await umbracoUi.refreshContentTree();
    await umbracoUi.clickElement(umbracoUi.getTreeItem("content", [viewMacroName]));

    // Insert macro
    await page.locator('#mceu_13-button').click();
    await page.locator('.umb-card-grid-item', {hasText: viewMacroName}).click();
    // cy.get('.umb-card-grid-item').contains(viewMacroName).click();

    // Save and publish
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    await umbracoUi.isSuccessNotificationVisible();

    // Ensure that the view gets rendered correctly
    const expected = `<h1>Acceptance test</h1><p> </p>`;
    await expect(await umbracoApi.content.verifyRenderedContent('/', expected, true)).toBeTruthy();

    // Cleanup
    await umbracoApi.macros.ensureNameNotExists(viewMacroName);
    await umbracoApi.partialViews.ensureMacroFileNameNotExists(partialFileName);
    await umbracoApi.documentTypes.ensureNameNotExists(viewMacroName);
    await umbracoApi.templates.ensureNameNotExists(viewMacroName);
  });

  test('Content with macro in grid', async ({ page, umbracoApi, umbracoUi }) => {
    const name = 'Content with macro in grid';
    const macroName = 'Grid macro';
    const macroFileName = macroName + '.cshtml';

    await umbracoApi.dataTypes.ensureNameNotExists(name);
    await umbracoApi.documentTypes.ensureNameNotExists(name);
    await umbracoApi.templates.ensureNameNotExists(name);
    await umbracoApi.macros.ensureNameNotExists(macroName);
    await umbracoApi.partialViews.ensureMacroFileNameNotExists(macroFileName);
    await umbracoApi.content.deleteAllContent();

    await createSimpleMacro(macroName, umbracoApi);

    const grid = new GridDataTypeBuilder()
      .withName(name)
      .withDefaultGrid()
      .build();

    const alias = AliasHelper.toAlias(name);
    
    // Save grid and get the ID
    const dataType = await umbracoApi.dataTypes.save(grid)
    
    // Create a document type using the data type
    const docType = new DocumentTypeBuilder()
      .withName(name)
      .withAlias(alias)
      .withAllowAsRoot(true)
      .withDefaultTemplate(alias)
      .addGroup()
        .addCustomProperty(dataType['id'])
          .withAlias('grid')
        .done()
      .done()
      .build();

    const generatedDocType = await umbracoApi.documentTypes.save(docType);
    const contentNode = new ContentBuilder()
      .withContentTypeAlias(generatedDocType["alias"])
      .addVariant()
        .withName(name)
        .withSave(true)
      .done()
      .build();

    await umbracoApi.content.save(contentNode);
    
    // Ugly wait but we have to wait for cache to rebuild
    await page.waitForTimeout(1000);

    // Edit the template to allow us to verify the rendered view
    await umbracoApi.templates.edit(name, `@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
        @{
            Layout = null;
        }
@Html.GetGridHtml(Model, "grid")`);

    // Act
    // Enter content
    await umbracoUi.refreshContentTree();
    await umbracoUi.clickElement(umbracoUi.getTreeItem("content", [name]));

    // Click add
    await page.locator(':nth-child(2) > .preview-row > .preview-col > .preview-cell').click(); // Choose 1 column layout.
    await page.locator('.umb-column > .templates-preview > :nth-child(2) > small').click(); // Choose headline
    await page.locator('.umb-cell-placeholder').click();
    // Click macro
    await page.locator(':nth-child(4) > .umb-card-grid-item > :nth-child(1)').click();
    // Select the macro
    await page.locator(`.umb-card-grid-item[title='${macroName}']`).click('bottom');


    // Save and publish
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.saveAndPublish));
    await umbracoUi.isSuccessNotificationVisible();
    
    const expected = `
    <div class="umb-grid">
      <div class="grid-section">
        <div>
          <div class="container">
            <div class="row clearfix">
              <div class="col-md-12 column">
                <div>
                  <h1>Acceptance test</h1>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>`

    await expect(await umbracoApi.content.verifyRenderedContent('/', expected, true)).toBeTruthy();

    // Clean
    await umbracoApi.dataTypes.ensureNameNotExists(name);
    await umbracoApi.documentTypes.ensureNameNotExists(name);
    await umbracoApi.templates.ensureNameNotExists(name);
    await umbracoApi.macros.ensureNameNotExists(macroName);
    await umbracoApi.partialViews.ensureMacroFileNameNotExists(macroFileName);
  });
});

