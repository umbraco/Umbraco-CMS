/// <reference types="Cypress" />
import {
    ContentBuilder,
    DocumentTypeBuilder,
} from 'umbraco-cypress-testhelpers';

context('Recycle bin', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
    });

    function refreshContentTree() {
        // Refresh to update the tree
        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();
        cy.umbracoContextMenuAction("action-refreshNode").click();
        // We have to wait in case the execution is slow, otherwise we'll try and click the item before it appears in the UI
        cy.get('.umb-tree-item__inner').should('exist', { timeout: 10000 });
    }
    
    it('Can delete content from recycle bin', () => {
        const contentToDeleteName = "DeleteMe";
        const contentToNotDeleteName = "DontDelete";
        const testType = "TestType";
        
        cy.umbracoEnsureDocumentTypeNameNotExists(testType);
        cy.deleteAllContent();
        
        const docType = new DocumentTypeBuilder()
            .withName(testType)
            .build();
        
        cy.saveDocumentType(docType).then((savedDocType) => {
            const contentToDelete = new ContentBuilder()
                .withContentTypeAlias(savedDocType.alias)
                .withAction("saveNew")
                .addVariant()
                    .withName(contentToDeleteName)
                    .withSave(true)
                .done()
                .build();

            const contentToNotDelete = new ContentBuilder()
                .withContentTypeAlias(savedDocType.alias)
                .withAction("saveNew")
                .addVariant()
                    .withName(contentToNotDeleteName)
                    .withSave(true)
                .done()
                .build();

            // Put it in the recycle bin
            cy.saveContent(contentToDelete).then(savedToDelete => {
                cy.deleteContentById(savedToDelete.id);
            });
            cy.saveContent(contentToNotDelete).then(savedNotToDelete => {
                cy.deleteContentById(savedNotToDelete.id)
            });
        });
        
        refreshContentTree();
        cy.umbracoTreeItem('content', ["Recycle Bin"]).click(); 
        cy.get('.umb-content-grid__content').contains(contentToDeleteName).closest('div').click();
        cy.umbracoButtonByLabelKey('actions_delete').click();
        cy.umbracoButtonByLabelKey('contentTypeEditor_yesDelete').click();

        cy.umbracoSuccessNotification().should('be.visible');

        cy.get('.umb-content-grid__content').contains(contentToDeleteName).should('not.exist');
        cy.umbracoTreeItem('content', ["Recycle Bin", contentToDeleteName]).should('not.exist');

        cy.get('.umb-content-grid__content').contains(contentToNotDeleteName).should('be.visible');
        cy.umbracoTreeItem('content', ["Recycle Bin", contentToNotDeleteName]).should('be.visible');
    });
});
