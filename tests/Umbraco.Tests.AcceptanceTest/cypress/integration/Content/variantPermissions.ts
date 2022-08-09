/// <reference types="Cypress" />
import {
    ContentBuilder,
    DocumentTypeBuilder,
    UserBuilder,
    UserGroupBuilder,
} from 'umbraco-cypress-testhelpers';

context('Content', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
    });
    
    function CreateOrUpdateUserGroup(name, id, alias, languages){
        const adminUserGroup = new UserGroupBuilder()
            .withName(name)
            .withId(id)
            .withAlias(alias)
            .withLanguages(languages || [])
            .withSections(["content", "media", "settings", "packages", "users", "member", "forms", "translation"])
            .addDefaultPermissions()
            .withAllowAll()
            .done()
            .withAction("save")
            .withUsers([-1])
            .build();
        cy.saveUserGroup(adminUserGroup)
    }

    it('Can publish language you have access to', () => {
        const documentTypeName = "TestType";
        const groupName = "Test group";
        const nodeName = "Home";
        const danishNodeName = "Hjem";
        const language1 = 'da';
        const language2 = 'en-US';
        const email = "test@test.com";


        cy.umbracoEnsureUserEmailNotExists(email);
        cy.umbracoEnsureUserGroupNameNotExists(groupName);
        cy.umbracoEnsureLanguageCultureNotExists(language1);
        cy.umbracoEnsureDocumentTypeNameNotExists(documentTypeName);
        cy.deleteAllContent();

        cy.umbracoCreateLanguage(language1, false, '1');

        CreateOrUpdateUserGroup("Administrators", 1, "admin", [1]);
        CreateOrUpdateUserGroup("Sensitive data", 5, "sensitiveData", [1]);

        const documentType = new DocumentTypeBuilder()
            .withName(documentTypeName)
            .withAllowAsRoot(true)
            .withAllowCultureVariation(true)
            .build();

        cy.saveDocumentType(documentType).then((generatedDocumentType) => {
            const rootContentNode = new ContentBuilder()
                .withAction("saveNew")
                .withContentTypeAlias(generatedDocumentType["alias"])
                .addVariant()
                    .withName(nodeName)
                    .withSave(true)
                    .withCulture(language2)
                .done()
                .addVariant()
                    .withName(danishNodeName)
                    .withSave(true)
                    .withCulture(language1)
                .done()
                .build();

            cy.saveContent(rootContentNode)
        });

        cy.get('.umb-tree-root-link').rightclick();
        cy.umbracoContextMenuAction('action-refreshNode').click();
        // We have to wait in case the execution is slow, otherwise we'll try and click the item before it appears in the UI
        cy.get('.umb-tree-item__inner').should('exist', {timeout: 10000});
        cy.umbracoTreeItem("content", [nodeName]).click();
        cy.umbracoButtonByLabelKey("buttons_saveAndPublish").click();
        cy.get('[data-element="overlay"]').get('[label-key="buttons_saveAndPublish"]').last().click();
        cy.umbracoSuccessNotification().should('be.visible');

        // Clean up
        // This is a bit ugly, but because we have messed with the admin privileges, we have to return them to the same state afterwards

        CreateOrUpdateUserGroup("Administrators", 1, "admin", []);
        CreateOrUpdateUserGroup("Sensitive data", 5, "sensitiveData", []);

        cy.umbracoEnsureUserEmailNotExists(email);
        cy.umbracoEnsureUserGroupNameNotExists(groupName);
        cy.umbracoEnsureLanguageCultureNotExists(language1);
        cy.umbracoEnsureDocumentTypeNameNotExists(documentTypeName);
        cy.deleteAllContent();
    });

    it('Can publish language you have access to', () => {
        const documentTypeName = "TestType";
        const groupName = "Test group";
        const nodeName = "Home";
        const danishNodeName = "Hjem";
        const language1 = 'da';
        const language2 = 'en-US';
        const email = "test@test.com";


        cy.umbracoEnsureUserEmailNotExists(email);
        cy.umbracoEnsureUserGroupNameNotExists(groupName);
        cy.umbracoEnsureLanguageCultureNotExists(language1);
        cy.umbracoEnsureDocumentTypeNameNotExists(documentTypeName);
        cy.deleteAllContent();

        cy.umbracoCreateLanguage(language1, false, '1');

        CreateOrUpdateUserGroup("Administrators", 1, "admin", [1]);
        CreateOrUpdateUserGroup("Sensitive data", 5, "sensitiveData", [1]);


        const documentType = new DocumentTypeBuilder()
            .withName(documentTypeName)
            .withAllowAsRoot(true)
            .withAllowCultureVariation(true)
            .build();

        cy.saveDocumentType(documentType).then((generatedDocumentType) => {
            const rootContentNode = new ContentBuilder()
                .withAction("saveNew")
                .withContentTypeAlias(generatedDocumentType["alias"])
                .addVariant()
                    .withName(nodeName)
                    .withSave(true)
                    .withCulture(language2)
                .done()
                .addVariant()
                    .withName(danishNodeName)
                    .withSave(true)
                    .withCulture(language1)
                .done()
                .build();

            cy.saveContent(rootContentNode)
        });
        cy.reload();
        cy.get('.umb-tour-step', { timeout: 60000 }).should('be.visible'); // We now due to the api calls this will be shown, but slow computers can take a while
        cy.get('.umb-tour-step__close').click();

        cy.get('.umb-language-picker__toggle').click();
        cy.contains('Danish').click();
        cy.umbracoTreeItem("content", [danishNodeName]).click();
        cy.umbracoButtonByLabelKey("buttons_saveAndPublish").click();
        cy.get('[alias="overlaySubmit"] button').click({force: true})
        cy.get('[data-element="overlay"]').should('be.visible');

        // Clean up
        
        // This is a bit ugly, but because we have messed with the admin privileges, we have to return them to the same state afterwards
        CreateOrUpdateUserGroup("Administrators", 1, "admin", []);
        CreateOrUpdateUserGroup("Sensitive data", 5, "sensitiveData", []);

        cy.umbracoEnsureUserEmailNotExists(email);
        cy.umbracoEnsureUserGroupNameNotExists(groupName);
        cy.umbracoEnsureLanguageCultureNotExists(language1);
        cy.umbracoEnsureDocumentTypeNameNotExists(documentTypeName);
        cy.deleteAllContent();
    });
    
});