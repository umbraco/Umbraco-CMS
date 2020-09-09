/// <reference types="Cypress" />
import { DocumentTypeBuilder, ContentBuilder } from 'umbraco-cypress-testhelpers';
context('Content', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
    });

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
        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();
        cy.umbracoContextMenuAction("action-refreshNode").click();

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
        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();
        cy.umbracoContextMenuAction("action-refreshNode").click();

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
        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();
        cy.umbracoContextMenuAction("action-refreshNode").click();

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
        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();
        cy.umbracoContextMenuAction("action-refreshNode").click();

        // Access node
        cy.umbracoTreeItem("content", [initialNodeName]).click();

        // Edit header
        cy.get('#headerName').clear();
        cy.umbracoEditorHeaderName(nodeName);

        // Save and publish
        cy.get('.btn-success').first().click();

        // Rollback
        cy.get('.umb-box-header :button').click();

        cy.get('.umb-box-content > .ng-scope > .input-block-level')
            .find('option[label*=' + new Date().getDate() + ']')
            .then(elements => {
                const option = elements[[elements.length - 1]].getAttribute('value');
                cy.get('.umb-box-content > .ng-scope > .input-block-level')
                    .select(option);
            });

        cy.get('.umb-editor-footer-content__right-side > [button-style="success"] > .umb-button > .btn-success').click();

        cy.reload();

        // Assert
        cy.get('.history').find('.umb-badge').eq(0).should('contain.text', "Save");
        cy.get('.history').find('.umb-badge').eq(1).should('contain.text', "Rollback");
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
        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();
        cy.umbracoContextMenuAction("action-refreshNode").click();

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
        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();
        cy.umbracoContextMenuAction("action-refreshNode").click();

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
        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();
        cy.umbracoContextMenuAction("action-refreshNode").click();

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
        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();
        cy.umbracoContextMenuAction("action-refreshNode").click();

        // Access node
        cy.umbracoTreeItem("content", [nodeName]).click();

        // Assert
        cy.get('[data-element="node-info-status"]').find('.umb-badge').should('contain.text', "Published");

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);
    });
});
