/// <reference types="Cypress" />
import {
    DocumentTypeBuilder,
    ContentBuilder
} from 'umbraco-cypress-testhelpers';

context('Routing', () => {

    let swedishLanguageId = 0;
    const swedishCulture = "sv";
    const danishCulture = "da"
    const nodeName = "Root";
    const childNodeName = "Child";
    const grandChildNodeName = "Grandchild";
    const rootDocTypeName = "Test document type";

    function refreshContentTree() {
        // Refresh to update the tree
        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();
        cy.umbracoContextMenuAction("action-refreshNode").click();
        // We have to wait in case the execution is slow, otherwise we'll try and click the item before it appears in the UI
        cy.get('.umb-tree-item__inner').should('exist', {timeout: 10000});
    }

    function saveNewLanguages() {
        // Save Danish
        const url = "/umbraco/backoffice/umbracoapi/language/SaveLanguage";
        const danishRequestBody = {
            culture: danishCulture,
            name: danishCulture
        }

        cy.umbracoApiRequest(url, "POST", danishRequestBody);

        // Save Swedish
        const swedishRequestBody = {
            culture: swedishCulture,
            name: swedishCulture
        }
        
        cy.umbracoApiRequest(url, "POST", swedishRequestBody).then((responseBody) => {
            swedishLanguageId = responseBody["id"];
        });
    }

    function configureDomain(id, name, lang) {
        //Save domain for child node
        const url = "/umbraco/backoffice/umbracoapi/content/PostSaveLanguageAndDomains"
        const body = {
            nodeId : id,
            domains : [
            {
                name : name,
                lang : lang
            }],
            language : 0
        }
        cy.umbracoApiRequest(url, 'POST', body);
    }

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
        // Ensure cleaned before tests run
        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);
        cy.umbracoEnsureLanguageCultureNotExists(danishCulture);
        cy.umbracoEnsureLanguageCultureNotExists(swedishCulture);
    });

    afterEach(() => {
        // Cleanup after tests
        cy.deleteAllContent();
        cy.umbracoEnsureDocumentTypeNameNotExists(rootDocTypeName);
        cy.umbracoEnsureLanguageCultureNotExists(danishCulture);
        cy.umbracoEnsureLanguageCultureNotExists(swedishCulture);
    })

    it('Root node published in language A, Child node published in language A', () => {

        const rootDocType = new DocumentTypeBuilder()
            .withName(rootDocTypeName)
            .withAllowAsRoot(true)
            .withAllowCultureVariation(true)
            .build();

        saveNewLanguages();

        cy.saveDocumentType(rootDocType).then((generatedRootDocType) => {
            const rootContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .withAction("publishNew")
                .addVariant()
                    .withCulture('en-US')
                    .withName(nodeName)
                    .withSave(true)
                    .withPublish(true)
                .done()
                .build();

            cy.saveContent(rootContentNode).then((generatedRootContent) => {
                const childContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .withAction("saveNew")
                .withParent(generatedRootContent["id"])
                .addVariant()
                    .withCulture('en-US')
                    .withName(childNodeName)
                    .withSave(true)
                .done()
                .build();

            cy.saveContent(childContentNode);
            });
        });

        // Refresh to update the tree
        refreshContentTree();

        cy.umbracoTreeItem("content", [nodeName, childNodeName]).click();
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        // Pop-up with what cultures you want to publish shows, click it
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').last().click();

        // Assert
        cy.get('.alert-success').should('exist');
    });


    it('Root node published in language A, Child node published in language B', () => {

        const rootDocType = new DocumentTypeBuilder()
            .withName(rootDocTypeName)
            .withAllowAsRoot(true)
            .withAllowCultureVariation(true)
            .build();

        saveNewLanguages();

        cy.saveDocumentType(rootDocType).then((generatedRootDocType) => {
            const rootContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .withAction("publishNew")
                .addVariant()
                    .withCulture('en-US')
                    .withName(nodeName)
                    .withSave(true)
                    .withPublish(true)
                .done()
                .build();

            cy.saveContent(rootContentNode).then((generatedRootContent) => {
                const childContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .withAction("saveNew")
                .withParent(generatedRootContent["id"])
                .addVariant()
                    .withCulture('en-US')
                    .withName(childNodeName)
                    .withSave(true)
                .done()
                .addVariant()
                    .withCulture(swedishCulture)
                    .withName("Bärn")
                    .withSave(true)
                .done()
                .build();

            cy.saveContent(childContentNode);
            });
        });

        // Refresh to update the tree
        refreshContentTree();

        cy.umbracoTreeItem("content", [nodeName, childNodeName]).click();
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        // Pop-up with what cultures you want to publish shows, click it
        cy.get('.umb-list').should('be.visible');
        cy.get('.checkbox').last().click();
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').last().click();

        // Assert
        cy.get('.alert-success').should('have.length', 2);
        cy.get('.alert-warning').should('exist');
    });

    it('Root node published in language A, Child node published in language A + B, Grandchild published in A + B', () => {

        const rootDocType = new DocumentTypeBuilder()
            .withName(rootDocTypeName)
            .withAllowAsRoot(true)
            .withAllowCultureVariation(true)
            .build();

        saveNewLanguages();

        cy.saveDocumentType(rootDocType).then((generatedRootDocType) => {
            const rootContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .withAction("publishNew")
                .addVariant()
                    .withCulture('en-US')
                    .withName(nodeName)
                    .withSave(true)
                    .withPublish(true)
                .done()
                .build();

            cy.saveContent(rootContentNode).then((generatedRootContent) => {

                configureDomain(generatedRootContent["id"], "/en", 1);
                const childContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .withAction("saveNew")
                .withParent(generatedRootContent["id"])
                .addVariant()
                    .withCulture('en-US')
                    .withName(childNodeName)
                    .withSave(true)
                .done()
                .addVariant()
                    .withCulture(swedishCulture)
                    .withName("Barn")
                    .withSave(true)
                .done()
                .build();

            cy.saveContent(childContentNode).then((generatedChildContent) => {

                configureDomain(generatedChildContent["id"], "/sv", swedishLanguageId);
                const grandChildContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .withAction("saveNew")
                .withParent(generatedChildContent["id"])
                .addVariant()
                    .withCulture('en-US')
                    .withName(grandChildNodeName)
                    .withSave(true)
                .done()
                .addVariant()
                    .withCulture(swedishCulture)
                    .withName("Barnbarn")
                    .withSave(true)
                .done()
                .build();

            cy.saveContent(grandChildContentNode);
                });
            });
        });

        // Refresh to update the tree
        refreshContentTree();

        // Publish Child
        cy.umbracoTreeItem("content", [nodeName, childNodeName]).click();
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        //Pop-up with what cultures you want to publish shows, click it
        cy.get('.umb-list').should('be.visible');
        cy.get('.checkbox').last().click();
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').last().click();

        // Close success notifications
        cy.get('.alert-success > .close').click({multiple : true});
        // Publish Grandchild
        cy.umbracoTreeItem("content", [nodeName, childNodeName, grandChildNodeName]).click();
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        // Pop-up with what cultures you want to publish shows, click it
        cy.get('.umb-list').should('be.visible');
        cy.get('.checkbox').last().click();
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').last().click();

        // Assert
        cy.get('.alert-success').should('have.length', 2);
        cy.get('.alert-warning').should('not.exist');
    });

    it('Root node published in language A, Child node published in language A + B, Grandchild published in A + B + C', () => {

        const rootDocType = new DocumentTypeBuilder()
            .withName(rootDocTypeName)
            .withAllowAsRoot(true)
            .withAllowCultureVariation(true)
            .build();

        saveNewLanguages();

        cy.saveDocumentType(rootDocType).then((generatedRootDocType) => {
            const rootContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .withAction("publishNew")
                .addVariant()
                    .withCulture('en-US')
                    .withName(nodeName)
                    .withSave(true)
                    .withPublish(true)
                .done()
                .build();

            cy.saveContent(rootContentNode).then((generatedRootContent) => {

                configureDomain(generatedRootContent["id"], "/en", 1);
                const childContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .withAction("saveNew")
                .withParent(generatedRootContent["id"])
                .addVariant()
                    .withCulture('en-US')
                    .withName(childNodeName)
                    .withSave(true)
                .done()
                .addVariant()
                    .withCulture(swedishCulture)
                    .withName("Barn")
                    .withSave(true)
                .done()
                .build();

            cy.saveContent(childContentNode).then((generatedChildContent) => {

                configureDomain(generatedChildContent["id"], "/sv", swedishLanguageId);
                const grandChildContentNode = new ContentBuilder()
                .withContentTypeAlias(generatedRootDocType["alias"])
                .withAction("saveNew")
                .withParent(generatedChildContent["id"])
                .addVariant()
                    .withCulture('en-US')
                    .withName(grandChildNodeName)
                    .withSave(true)
                .done()
                .addVariant()
                    .withCulture(swedishCulture)
                    .withName("Barnbarn")
                    .withSave(true)
                .done()
                .addVariant()
                    .withCulture(danishCulture)
                    .withName("Barnebarn")
                    .withSave(true)
                .done()
                .build();

            cy.saveContent(grandChildContentNode);
                });
            });
        });

        // Refresh to update the tree
        refreshContentTree();

        // Publish Child
        cy.umbracoTreeItem("content", [nodeName, childNodeName]).click();
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        // Pop-up with what cultures you want to publish shows, click it
        cy.get('.umb-list').should('be.visible');
        cy.get('.checkbox').last().click();
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').last().click();

        // Publish Grandchild
        cy.umbracoTreeItem("content", [nodeName, childNodeName, grandChildNodeName]).click();
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        // Pop-up with what cultures you want to publish shows, click it
        cy.get('.umb-list').should('be.visible');
        cy.get('.checkbox').click({multiple : true});
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').last().click();

        // Assert
        cy.get('.alert-success').should('exist');
        cy.get('.alert-warning').should('exist');
    });
});
