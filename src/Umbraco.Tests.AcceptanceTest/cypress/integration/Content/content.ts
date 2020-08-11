/// <reference types="Cypress" />
import { DocumentTypeBuilder } from 'umbraco-cypress-testhelpers';
context('Content', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
    });

    it('Copy content', () => {
        const rootdocTypeName = "Test document type";
        const childDocTypeName = "Child test document type";
        const nodeName = "1) Home";
        const childNodeName = "1) Child";
        const anotherNodeName = "2) Home";

        const childDocType = new DocumentTypeBuilder()
                    .withName(childDocTypeName)
                    .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
        cy.umbracoEnsureDocumentTypeNameNotExists(childDocTypeName);

        cy.saveDocumentType(childDocType).then((generatedDocType) => 
        {
            const rootDocType = new DocumentTypeBuilder()
                .withName(rootdocTypeName)
                .withAllowAsRoot(true)
                .withAllowedContentTypes(generatedDocType["id"])
                .build();

            cy.saveDocumentType(rootDocType);
        });

        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();

        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.menu-label').first().click();

        // Type name
        cy.umbracoEditorHeaderName(nodeName);

        // Save
        cy.get('.btn-info').click();
        cy.umbracoSuccessNotification().should('be.visible');

        // Add an item under root node
        cy.umbracoTreeItem("content", [nodeName]).rightclick();
        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.umb-actions').first().click();
        cy.umbracoEditorHeaderName(childNodeName);
        cy.get('.btn-info').click();

        // Add second root node
        cy.get('li .umb-tree-root:contains("Content")').rightclick();
        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.menu-label').first().click();
        cy.umbracoEditorHeaderName(anotherNodeName);
        cy.get('.btn-info').click();

        // Copy node
        cy.umbracoTreeItem("content", [nodeName, childNodeName]).rightclick({force:true});
        cy.umbracoContextMenuAction("action-copy").click();
        cy.get('.umb-pane [data-element="tree-item-' + anotherNodeName + '"]').click();
        cy.get('.umb-dialog-footer > .btn-primary').click();
        
        // Assert
        cy.get('.alert-success').should('exist');

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
        cy.umbracoEnsureDocumentTypeNameNotExists(childDocTypeName);
    });

    it('Move content', () => {
        const rootdocTypeName = "Test document type";
        const childDocTypeName = "Child test document type";
        const nodeName = "1) Home";
        const childNodeName = "1) Child";
        const anotherNodeName = "2) Home";

        const childDocType = new DocumentTypeBuilder()
                    .withName(childDocTypeName)
                    .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
        cy.umbracoEnsureDocumentTypeNameNotExists(childDocTypeName);

        cy.saveDocumentType(childDocType).then((generatedDocType) => 
        {
            const rootDocType = new DocumentTypeBuilder()
                .withName(rootdocTypeName)
                .withAllowAsRoot(true)
                .withAllowedContentTypes(generatedDocType["id"])
                .build();

            cy.saveDocumentType(rootDocType);
        });

        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();

        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.menu-label').first().click();

        // Type name
        cy.umbracoEditorHeaderName(nodeName);

        // Save
        cy.get('.btn-info').click();
        cy.umbracoSuccessNotification().should('be.visible');

        // Add an item under root node
        cy.umbracoTreeItem("content", [nodeName]).rightclick();
        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.umb-actions').first().click();
        cy.umbracoEditorHeaderName(childNodeName);
        cy.get('.btn-info').click();

        // Add second root node
        cy.get('li .umb-tree-root:contains("Content")').rightclick();
        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.menu-label').first().click();
        cy.umbracoEditorHeaderName(anotherNodeName);
        cy.get('.btn-info').click();

        // Move node
        cy.umbracoTreeItem("content", [nodeName, childNodeName]).rightclick({force:true});
        cy.umbracoContextMenuAction("action-move").click();
        cy.get('.umb-pane [data-element="tree-item-' + anotherNodeName + '"]').click();
        cy.get('.umb-dialog-footer > .btn-primary').click();
        
        // Assert
        cy.get('.alert-success').should('exist');

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
        cy.umbracoEnsureDocumentTypeNameNotExists(childDocTypeName);
    });

    it('Sort content', () => {
        const rootdocTypeName = "Test document type";
        const childDocTypeName = "Child test document type";
        const nodeName = "1) Home";
        const firstChildNodeName = "1) Child";
        const secondChildNodeName = "2) Child";

        const childDocType = new DocumentTypeBuilder()
            .withName(childDocTypeName)
            .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
        cy.umbracoEnsureDocumentTypeNameNotExists(childDocTypeName);

        cy.saveDocumentType(childDocType).then((generatedDocType) => {
            const rootDocType = new DocumentTypeBuilder()
                .withName(rootdocTypeName)
                .withAllowAsRoot(true)
                .withAllowedContentTypes(generatedDocType["id"])
                .build();

            cy.saveDocumentType(rootDocType);
        });

        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();

        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.menu-label').first().click();

        // Type name
        cy.umbracoEditorHeaderName(nodeName);

        // Save
        cy.get('.btn-info').click();
        cy.umbracoSuccessNotification().should('be.visible');

        // Add an item under root node
        cy.umbracoTreeItem("content", [nodeName]).rightclick();
        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.umb-actions').first().click();
        cy.umbracoEditorHeaderName(firstChildNodeName);
        cy.get('.btn-info').click();

        // Add an item under root node
        cy.umbracoTreeItem("content", [nodeName]).rightclick();
        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.umb-actions').first().click();
        cy.umbracoEditorHeaderName(secondChildNodeName);
        cy.get('.btn-info').click();

        // Sort nodes
        cy.umbracoTreeItem("content", [nodeName]).rightclick({ force: true });
        cy.umbracoContextMenuAction("action-sort").click();
        
        const offsetFirstElm = cy.get('.ui-sortable .ui-sortable-handle :nth-child(2)').eq(0).then($el => $el.offset());
        offsetFirstElm.then((offsetFirstElm) => {
            const pageYFirst = Math.round(offsetFirstElm.top);

            const offsetSecondElm = cy.get('.ui-sortable .ui-sortable-handle :nth-child(2)').eq(1).then($el => $el.offset());
            offsetSecondElm.then((offset) => {
                const pageX = Math.round(offset.left);
                const pageY = Math.round(offset.top);

                cy.get('.table-sortable').find('tbody :nth-child(1)').eq(0)
                    .trigger('mousedown', { which: 1, pageX: pageX, pageY: pageYFirst })
                    .trigger('mousemove', { which: 1, pageX: pageX, pageY: pageY })
                    .trigger('mouseup');
            })
        });

        // Save and close dialog
        cy.get('.umb-modalcolumn .btn-success').click();
        cy.get('.umb-modalcolumn .btn-link').click();

        // Assert
        cy.get('.umb-tree-item [node="child"]').eq(0).should('contain.text', secondChildNodeName);
        cy.get('.umb-tree-item [node="child"]').eq(1).should('contain.text', firstChildNodeName);

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
        cy.umbracoEnsureDocumentTypeNameNotExists(childDocTypeName);
    });

    it('Rollback content', () => {
        const rootdocTypeName = "Test document type";
        const initialNodeName = "Home node";
        const nodeName = "Home";

        const rootDocType = new DocumentTypeBuilder()
            .withName(rootdocTypeName)
            .withAllowAsRoot(true)
            .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
        cy.saveDocumentType(rootDocType);

        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();

        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.menu-label').first().click();

        // Type name
        cy.umbracoEditorHeaderName(initialNodeName);

        // Save and publish
        cy.get('.btn-success').first().click();
        cy.umbracoSuccessNotification().should('be.visible');

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
                const option = elements[elements.length - 1].getAttribute('value');
                cy.get('.umb-box-content > .ng-scope > .input-block-level')
                    .select(option);
            });

        cy.get('.umb-editor-footer-content__right-side > [button-style="success"] > .umb-button > .btn-success').click();
        
        // Assert
        cy.get('.history').find('.umb-badge').eq(0).should('contain.text',"Save");
        cy.get('.history').find('.umb-badge').eq(1).should('contain.text',"Rollback");
        cy.get('#headerName').should('have.value', initialNodeName);

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName).wait(100);
    });

    it('View audit trail', () => {
        const rootdocTypeName = "Test document type";
        const nodeName = "Home";
        const labelName = "Name";

        const rootDocType = new DocumentTypeBuilder()
            .withName(rootdocTypeName)
            .withAllowAsRoot(true)
            .addGroup()
                .addTextBoxProperty()
                    .withLabel(labelName)
                .done()
            .done()
            .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
        cy.saveDocumentType(rootDocType);

        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();

        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.menu-label').first().click();

        // Type name
        cy.umbracoEditorHeaderName(nodeName);

        // Save and publish
        cy.get('.btn-success').first().click();
        cy.umbracoSuccessNotification().should('be.visible');

        // Navigate to Info app
        cy.get(':nth-child(2) > [ng-show="navItem.alias !== \'more\'"]').click();

        // Assert
        cy.get('.history').should('exist');

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
    });

    it('Save draft', () => {
        const rootdocTypeName = "Test document type";
        const nodeName = "Home";

        const rootDocType = new DocumentTypeBuilder()
            .withName(rootdocTypeName)
            .withAllowAsRoot(true)
            .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
        cy.saveDocumentType(rootDocType);

        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();

        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.menu-label').first().click();

        // Type name
        cy.umbracoEditorHeaderName(nodeName);

        // Save
        cy.get('.btn-info').click();
        cy.umbracoSuccessNotification().should('be.visible');
        
        // Assert
        cy.get('[data-element="node-info-status"]').find('.umb-badge').should('contain.text',"Draft");

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
    });

    it('Preview draft', () => {
        const rootdocTypeName = "Test document type";
        const nodeName = "Home";

        const rootDocType = new DocumentTypeBuilder()
            .withName(rootdocTypeName)
            .withAllowAsRoot(true)
            .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
        cy.saveDocumentType(rootDocType);

        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();

        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.menu-label').first().click();

        // Type name
        cy.umbracoEditorHeaderName(nodeName);

        // Save
        cy.get('.btn-info').click();

        // Preview
        cy.get('[alias="preview"]').should('be.visible').click();

        // Assert
        cy.umbracoSuccessNotification({ multiple: true }).should('be.visible');

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
    });

    it('Publish draft', () => {
        const rootdocTypeName = "Test document type";
        const nodeName = "Home";

        const rootDocType = new DocumentTypeBuilder()
            .withName(rootdocTypeName)
            .withAllowAsRoot(true)
            .build();

        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
        cy.saveDocumentType(rootDocType);

        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();

        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.menu-label').first().click();

        // Type name
        cy.umbracoEditorHeaderName(nodeName);

        // Save and publish
        cy.get('.btn-success').first().click();
        cy.umbracoSuccessNotification().should('be.visible');
        
        // Assert
        cy.get('[data-element="node-info-status"]').find('.umb-badge').should('contain.text',"Published");

        // Clean up (content is automatically deleted when document types are gone)
        cy.umbracoEnsureDocumentTypeNameNotExists(rootdocTypeName);
    });
});
