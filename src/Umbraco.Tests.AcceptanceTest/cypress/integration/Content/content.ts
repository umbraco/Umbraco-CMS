/// <reference types="Cypress" />
import {
    DocumentTypeBuilder,
    ContentBuilder,
    AliasHelper,
    GridDataTypeBuilder,
    PartialViewMacroBuilder,
    MacroBuilder
} from 'umbraco-cypress-testhelpers';

context('Content', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
    });

    function refreshContentTree(){
        // Refresh to update the tree
        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();
        cy.umbracoContextMenuAction("action-refreshNode").click();
        // We have to wait in case the execution is slow, otherwise we'll try and click the item before it appears in the UI
        cy.get('.umb-tree-item__inner').should('exist', {timeout: 10000});
    }

    function createSimpleMacro(name){
        const insertMacro = new PartialViewMacroBuilder()
            .withName(name)
            .withContent(`@inherits Umbraco.Web.Macros.PartialViewMacroPage
<h1>Acceptance test</h1>`)
            .build();

        const macroWithPartial = new MacroBuilder()
            .withName(name)
            .withPartialViewMacro(insertMacro)
            .withRenderInEditor()
            .withUseInEditor()
            .build();

        cy.saveMacroWithPartial(macroWithPartial);
    }

    it('Copy content', () => {
        const rootDocTypeName = "Test document type";
        const childDocTypeName = "Child test document type";
        const nodeName = "1) Home";
        const childNodeName = "1) Child";
        const anotherNodeName = "2) Home";

        const childDocType = new DocumentTypeBuilder()
            .withName(childDocTypeName)
            .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);
        cy.umbracoEnsureDocumentTypeNameNotExists(childDocTypeName);

        cy.saveDocumentType(childDocType).then((generatedChildDocType) => {
            const rootDocTypeAlias;
            const createdChildDocType = generatedChildDocType;

            cy.get('li .umb-tree-root:contains("Content")').should("be.visible");

            const rootDocType = new DocumentTypeBuilder()
                .withName(rootDocTypeName)
                .withAllowAsRoot(true)
                .withAllowedContentTypes(createdChildDocType["id"])
                .build();

            cy.saveDocumentType(rootDocType).then((generatedRootDocType) => {
                rootDocTypeAlias = generatedRootDocType["alias"];

                const rootContentNode = new ContentBuilder()
                    .withContentTypeAlias(rootDocTypeAlias)
                    .withAction("saveNew")
                    .addVariant()
                        .withName(nodeName)
                        .withSave(true)
                    .done()
                    .build();

                cy.saveContent(rootContentNode).then((contentNode) => {
                    // Add an item under root node
                    const childContentNode = new ContentBuilder()
                        .withContentTypeAlias(createdChildDocType["alias"])
                        .withAction("saveNew")
                        .withParent(contentNode["id"])
                        .addVariant()
                            .withName(childNodeName)
                            .withSave(true)
                        .done()
                        .build();

                    cy.saveContent(childContentNode);
                });

                const anotherRootContentNode = new ContentBuilder()
                    .withContentTypeAlias(rootDocTypeAlias)
                    .withAction("saveNew")
                    .addVariant()
                        .withName(anotherNodeName)
                        .withSave(true)
                    .done()
                    .build();

                cy.saveContent(anotherRootContentNode);
            });
        });

        // Refresh to update the tree
        refreshContentTree();

        // Copy node
        cy.umbracoTreeItem("content", [nodeName, childNodeName]).rightclick({ force: true });
        cy.umbracoContextMenuAction("action-copy").click();
        cy.get('.umb-pane [data-element="tree-item-' + anotherNodeName + '"]').click();
        cy.get('.umb-dialog-footer > .btn-primary').click();

        // Assert
        cy.get('.alert-success').should('exist');

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);
        cy.umbracoEnsureDocumentTypeNameNotExists(childDocTypeName);
    });

    it('Move content', () => {
        const rootDocTypeName = "Test document type";
        const childDocTypeName = "Child test document type";
        const nodeName = "1) Home";
        const childNodeName = "1) Child";
        const anotherNodeName = "2) Home";

        const childDocType = new DocumentTypeBuilder()
            .withName(childDocTypeName)
            .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);
        cy.umbracoEnsureDocumentTypeNameNotExists(childDocTypeName);

        cy.saveDocumentType(childDocType).then((generatedChildDocType) => {
            const rootDocTypeAlias;
            const createdChildDocType = generatedChildDocType;

            cy.get('li .umb-tree-root:contains("Content")').should("be.visible");

            const rootDocType = new DocumentTypeBuilder()
                .withName(rootDocTypeName)
                .withAllowAsRoot(true)
                .withAllowedContentTypes(createdChildDocType["id"])
                .build();

            cy.saveDocumentType(rootDocType).then((generatedRootDocType) => {
                rootDocTypeAlias = generatedRootDocType["alias"];

                const rootContentNode = new ContentBuilder()
                    .withContentTypeAlias(rootDocTypeAlias)
                    .withAction("saveNew")
                    .addVariant()
                        .withName(nodeName)
                        .withSave(true)
                    .done()
                    .build();

                cy.saveContent(rootContentNode).then((contentNode) => {
                    // Add an item under root node
                    const childContentNode = new ContentBuilder()
                        .withContentTypeAlias(createdChildDocType["alias"])
                        .withAction("saveNew")
                        .withParent(contentNode["id"])
                        .addVariant()
                            .withName(childNodeName)
                            .withSave(true)
                        .done()
                        .build();

                    cy.saveContent(childContentNode);
                });

                const anotherRootContentNode = new ContentBuilder()
                    .withContentTypeAlias(rootDocTypeAlias)
                    .withAction("saveNew")
                    .addVariant()
                        .withName(anotherNodeName)
                        .withSave(true)
                    .done()
                    .build();

                cy.saveContent(anotherRootContentNode);
            });
        });

        // Refresh to update the tree
        refreshContentTree();

        // Move node
        cy.umbracoTreeItem("content", [nodeName, childNodeName]).rightclick({ force: true });
        cy.umbracoContextMenuAction("action-move").click();
        cy.get('.umb-pane [data-element="tree-item-' + anotherNodeName + '"]').click();
        cy.get('.umb-dialog-footer > .btn-primary').click();

        // Assert
        cy.get('.alert-success').should('exist');

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);
        cy.umbracoEnsureDocumentTypeNameNotExists(childDocTypeName);
    });

    it('Sort content', () => {
        const rootDocTypeName = "Test document type";
        const childDocTypeName = "Child test document type";
        const nodeName = "1) Home";
        const firstChildNodeName = "1) Child";
        const secondChildNodeName = "2) Child";

        const childDocType = new DocumentTypeBuilder()
            .withName(childDocTypeName)
            .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);
        cy.umbracoEnsureDocumentTypeNameNotExists(childDocTypeName);

        cy.saveDocumentType(childDocType).then((generatedChildDocType) => {
            const createdChildDocType = generatedChildDocType;

            cy.get('li .umb-tree-root:contains("Content")').should("be.visible");

            const rootDocType = new DocumentTypeBuilder()
                .withName(rootDocTypeName)
                .withAllowAsRoot(true)
                .withAllowedContentTypes(createdChildDocType["id"])
                .build();

            cy.saveDocumentType(rootDocType).then((generatedRootDocType) => {
                const parentId;

                const rootContentNode = new ContentBuilder()
                    .withContentTypeAlias(generatedRootDocType["alias"])
                    .withAction("saveNew")
                    .addVariant()
                        .withName(nodeName)
                        .withSave(true)
                    .done()
                    .build();

                cy.saveContent(rootContentNode).then((contentNode) => {
                    parentId = contentNode["id"];

                    // Add an item under root node
                    const firstChildContentNode = new ContentBuilder()
                        .withContentTypeAlias(createdChildDocType["alias"])
                        .withAction("saveNew")
                        .withParent(parentId)
                        .addVariant()
                            .withName(firstChildNodeName)
                            .withSave(true)
                        .done()
                        .build();

                    cy.saveContent(firstChildContentNode);

                    // Add a second item under root node
                    const secondChildContentNode = new ContentBuilder()
                        .withContentTypeAlias(createdChildDocType["alias"])
                        .withAction("saveNew")
                        .withParent(parentId)
                        .addVariant()
                            .withName(secondChildNodeName)
                            .withSave(true)
                        .done()
                        .build();

                    cy.saveContent(secondChildContentNode);
                });
            });
        });

        // Refresh to update the tree
        refreshContentTree();

        // Sort nodes
        cy.umbracoTreeItem("content", [nodeName]).rightclick({ force: true });
        cy.umbracoContextMenuAction("action-sort").click();

        //Drag and drop
        cy.get('.ui-sortable .ui-sortable-handle :nth-child(2)').eq(0).trigger('mousedown', { which: 1 })
        cy.get('.ui-sortable .ui-sortable-handle :nth-child(2)').eq(1).trigger("mousemove").trigger("mouseup")

        // Save and close dialog
        cy.get('.umb-modalcolumn .btn-success').click();
        cy.get('.umb-modalcolumn .btn-link').click();

        // Assert
        cy.get('.umb-tree-item [node="child"]').eq(0).should('contain.text', secondChildNodeName);
        cy.get('.umb-tree-item [node="child"]').eq(1).should('contain.text', firstChildNodeName);

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);
        cy.umbracoEnsureDocumentTypeNameNotExists(childDocTypeName);
    });

    it('Rollback content', () => {
        const rootDocTypeName = "Test document type";
        const initialNodeName = "Home node";
        const nodeName = "Home";

        const rootDocType = new DocumentTypeBuilder()
            .withName(rootDocTypeName)
            .withAllowAsRoot(true)
            .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);

        cy.saveDocumentType(rootDocType).then((generatedRootDocType) => {
            const rootContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .addVariant()
                    .withName(initialNodeName)
                    .withSave(true)
                .done()
                .build();

            cy.saveContent(rootContentNode)
        });

        // Refresh to update the tree
        refreshContentTree();

        // Access node
        cy.umbracoTreeItem("content", [initialNodeName]).click();

        // Edit header
        cy.get('#headerName').clear();
        cy.umbracoEditorHeaderName(nodeName);

        // Save and publish
        cy.get('.btn-success').first().click();
        cy.umbracoSuccessNotification().should('be.visible');

        // Rollback
        cy.get('.umb-box-header :button').click();

        cy.get('.umb-box-content > div > .input-block-level')
            .find('option[label*=' + new Date().getDate() + ']')
            .then(elements => {
                const option = elements[elements.length - 1].getAttribute('value');
                cy.get('.umb-box-content > div > .input-block-level')
                    .select(option);
            });

        cy.get('.umb-editor-footer-content__right-side > [button-style="success"] > .umb-button > .btn-success').click();

        cy.reload();

        // Assert
        cy.get('.history').find('.umb-badge').contains('Save').should('be.visible');
        cy.get('.history').find('.umb-badge').contains('Rollback').should('be.visible');
        cy.get('#headerName').should('have.value', initialNodeName);

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);
    });

    it('View audit trail', () => {
        const rootDocTypeName = "Test document type";
        const nodeName = "Home";
        const labelName = "Name";

        const rootDocType = new DocumentTypeBuilder()
            .withName(rootDocTypeName)
            .withAllowAsRoot(true)
            .addGroup()
                .addTextBoxProperty()
                    .withLabel(labelName)
                .done()
            .done()
            .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);

        cy.saveDocumentType(rootDocType).then((generatedRootDocType) => {
            const rootContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .addVariant()
                    .withName(nodeName)
                    .withSave(true)
                .done()
                .build();

            cy.saveContent(rootContentNode)
        });

        // Refresh to update the tree
        refreshContentTree();

        // Access node
        cy.umbracoTreeItem("content", [nodeName]).click();

        // Navigate to Info app
        cy.get(':nth-child(2) > [ng-show="navItem.alias !== \'more\'"]').click();

        // Assert
        cy.get('.history').should('exist');

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);
    });

    it('Save draft', () => {
        const rootDocTypeName = "Test document type";
        const nodeName = "Home";

        const rootDocType = new DocumentTypeBuilder()
            .withName(rootDocTypeName)
            .withAllowAsRoot(true)
            .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);

        cy.saveDocumentType(rootDocType).then((generatedRootDocType) => {
            const rootContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .withAction("saveNew")
                .addVariant()
                    .withName(nodeName)
                    .withSave(true)
                .done()
                .build();

            cy.saveContent(rootContentNode)
        });

        // Refresh to update the tree
        refreshContentTree();

        // Access node
        cy.umbracoTreeItem("content", [nodeName]).click();

        // Assert
        cy.get('[data-element="node-info-status"]').find('.umb-badge').should('contain.text', "Draft");

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);
    });

    it('Preview draft', () => {
        const rootDocTypeName = "Test document type";
        const nodeName = "Home";

        const rootDocType = new DocumentTypeBuilder()
            .withName(rootDocTypeName)
            .withAllowAsRoot(true)
            .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);

        cy.saveDocumentType(rootDocType).then((generatedRootDocType) => {
            const rootContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .withAction("saveNew")
                .addVariant()
                    .withName(nodeName)
                    .withSave(true)
                .done()
                .build();

            cy.saveContent(rootContentNode)
        });

        // Refresh to update the tree
        refreshContentTree();

        // Access node
        cy.umbracoTreeItem("content", [nodeName]).click();

        // Preview
        cy.get('[alias="preview"]').should('be.visible').click();

        // Assert
        cy.umbracoSuccessNotification({ multiple: true }).should('be.visible');

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);
    });

    it('Publish draft', () => {
        const rootDocTypeName = "Test document type";
        const nodeName = "Home";

        const rootDocType = new DocumentTypeBuilder()
            .withName(rootDocTypeName)
            .withAllowAsRoot(true)
            .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);

        cy.saveDocumentType(rootDocType).then((generatedRootDocType) => {
            const rootContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .addVariant()
                    .withName(nodeName)
                    .withSave(true)
                .done()
                .build();

            cy.saveContent(rootContentNode)
        });

        // Refresh to update the tree
        refreshContentTree();

        // Access node
        cy.umbracoTreeItem("content", [nodeName]).click();

        // Assert
        cy.get('[data-element="node-info-status"]').find('.umb-badge').should('contain.text', "Published");

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);
    });

    it('Content with contentpicker', () => {
        const pickerDocTypeName = 'Content picker doc type';
        const pickerDocTypeAlias = AliasHelper.toAlias(pickerDocTypeName);
        const pickedDocTypeName = 'Picked content document type';

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(pickerDocTypeName);
        cy.umbracoEnsureTemplateNameNotExists(pickerDocTypeName);
        cy.umbracoEnsureDocumentTypeNameNotExists(pickedDocTypeName);

        // Create the content type and content we'll be picking from.
        const pickedDocType = new DocumentTypeBuilder()
            .withName(pickedDocTypeName)
            .withAllowAsRoot(true)
            .addGroup()
                .addTextBoxProperty()
                    .withAlias('text')
                .done()
            .done()
            .build();

        cy.saveDocumentType(pickedDocType).then((generatedType) => {
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
            cy.saveContent(pickedContentNode);
        });

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

        cy.saveDocumentType(pickerDocType);

        // Edit it the template to allow us to verify the rendered view.
        cy.editTemplate(pickerDocTypeName, `@inherits Umbraco.Web.Mvc.UmbracoViewPage<ContentModels.ContentPickerDocType>
        @using ContentModels = Umbraco.Web.PublishedModels;
        @{
            Layout = null;
        }

        @{
            IPublishedContent typedContentPicker = Model.Value<IPublishedContent>("picker");
            if (typedContentPicker != null)
            {
                <p>@typedContentPicker.Value("text")</p>
            }
        }`);

        // Create content with content picker
        cy.get('.umb-tree-root-link').rightclick();
        cy.get('[data-element="action-create"]').click();
        cy.get('[data-element="action-create-' + pickerDocTypeAlias + '"] > .umb-action-link').click();
        // Fill out content
        cy.umbracoEditorHeaderName('ContentPickerContent');
        cy.get('.umb-node-preview-add').click();
        // Should really try and find a better way to do this, but umbracoTreeItem tries to click the content pane in the background
        cy.get('[ng-if="vm.treeReady"] > .umb-tree .umb-tree-item__inner').click();
        // We have to wait for the picked content to show up or it wont be added.
        cy.get('.umb-node-preview__description').should('be.visible');
        //save and publish
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        cy.umbracoSuccessNotification().should('be.visible');

        // Assert
        cy.log('Checking that content is rendered correctly.')
        const expectedContent = '<p>Acceptance test</p>'
        cy.umbracoVerifyRenderedViewContent('contentpickercontent', expectedContent, true).should('be.true');
        // clean
        cy.umbracoEnsureDocumentTypeNameNotExists(pickerDocTypeName);
        cy.umbracoEnsureTemplateNameNotExists(pickerDocTypeName);
        cy.umbracoEnsureDocumentTypeNameNotExists(pickedDocTypeName);
    });

    it('Content with macro in RTE', () => {
        const viewMacroName = 'Content with macro in RTE';
        const partialFileName = viewMacroName + '.cshtml';

        cy.umbracoEnsureMacroNameNotExists(viewMacroName);
        cy.umbracoEnsurePartialViewMacroFileNameNotExists(partialFileName);
        cy.umbracoEnsureDocumentTypeNameNotExists(viewMacroName);
        cy.umbracoEnsureTemplateNameNotExists(viewMacroName);
        cy.deleteAllContent();

        // First thing first we got to create the macro we will be inserting
        createSimpleMacro(viewMacroName);

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

        cy.saveDocumentType(docType).then((generatedDocType) => {
            // Might as wel initally create the content here, the less GUI work during the test the better
            const contentNode = new ContentBuilder()
                .withContentTypeAlias(generatedDocType["alias"])
                .withAction('saveNew')
                .addVariant()
                    .withName(viewMacroName)
                    .withSave(true)
                .done()
                .build();

            cy.saveContent(contentNode);
        });

        // Edit the macro template in order to have something to verify on when rendered.
        cy.editTemplate(viewMacroName, `@inherits Umbraco.Web.Mvc.UmbracoViewPage
@using ContentModels = Umbraco.Web.PublishedModels;
@{
  Layout = null;
}
@{
    if (Model.HasValue("text")){
        @(Model.Value("text"))
    }
} `);

        // Enter content
        refreshContentTree();
        cy.umbracoTreeItem("content", [viewMacroName]).click();

        // Insert macro
        cy.get('#mceu_13-button').click();
        cy.get('.umb-card-grid-item').contains(viewMacroName).click();

        // Save and publish
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        cy.umbracoSuccessNotification().should('be.visible');

        // Ensure that the view gets rendered correctly
        const expected = `<h1>Acceptance test</h1><p> </p>`;
        cy.umbracoVerifyRenderedViewContent('/', expected, true).should('be.true');

        // Cleanup
        cy.umbracoEnsureMacroNameNotExists(viewMacroName);
        cy.umbracoEnsurePartialViewMacroFileNameNotExists(partialFileName);
        cy.umbracoEnsureDocumentTypeNameNotExists(viewMacroName);
        cy.umbracoEnsureTemplateNameNotExists(viewMacroName);
    });

    it('Content with macro in grid', () => {
        const name = 'Content with macro in grid';
        const macroName = 'Grid macro';
        const macroFileName = macroName + '.cshtml';

        cy.umbracoEnsureDataTypeNameNotExists(name);
        cy.umbracoEnsureDocumentTypeNameNotExists(name);
        cy.umbracoEnsureTemplateNameNotExists(name);
        cy.umbracoEnsureMacroNameNotExists(macroName);
        cy.umbracoEnsurePartialViewMacroFileNameNotExists(macroFileName);
        cy.deleteAllContent();

        createSimpleMacro(macroName);

        const grid = new GridDataTypeBuilder()
            .withName(name)
            .withDefaultGrid()
            .build();

        const alias = AliasHelper.toAlias(name);
        // Save grid and get the ID
        cy.saveDataType(grid).then((dataType) => {
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

            cy.saveDocumentType(docType).then((generatedDocType) => {
                const contentNode = new ContentBuilder()
                    .withContentTypeAlias(generatedDocType["alias"])
                    .addVariant()
                        .withName(name)
                        .withSave(true)
                    .done()
                    .build();

                cy.saveContent(contentNode);
            });
        });

        // Edit the template to allow us to verify the rendered view
        cy.editTemplate(name, `@inherits Umbraco.Web.Mvc.UmbracoViewPage
@using ContentModels = Umbraco.Web.PublishedModels;
@{
  Layout = null;
}
@Html.GetGridHtml(Model, "grid")`);

        // Act
        // Enter content
        refreshContentTree();
        cy.umbracoTreeItem("content", [name]).click();
        // Click add
        cy.get(':nth-child(2) > .preview-row > .preview-col > .preview-cell').click(); // Choose 1 column layout.
        cy.get('.umb-column > .templates-preview > :nth-child(2) > small').click(); // Choose headline
        cy.get('.umb-cell-placeholder').click();
        // Click macro
        cy.get(':nth-child(4) > .umb-card-grid-item > :nth-child(1)').click();
        // Select the macro
        cy.get(`.umb-card-grid-item[title='${macroName}']`).click('bottom');


        // Save and publish
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        cy.umbracoSuccessNotification().should('be.visible');

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

        cy.umbracoVerifyRenderedViewContent('/', expected, true).should('be.true');

        // Clean
        cy.umbracoEnsureDataTypeNameNotExists(name);
        cy.umbracoEnsureDocumentTypeNameNotExists(name);
        cy.umbracoEnsureTemplateNameNotExists(name);
        cy.umbracoEnsureMacroNameNotExists(macroName);
        cy.umbracoEnsurePartialViewMacroFileNameNotExists(macroFileName);
    });
});
