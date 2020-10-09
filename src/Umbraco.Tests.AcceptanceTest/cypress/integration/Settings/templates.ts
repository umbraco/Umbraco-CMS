/// <reference types="Cypress" />
import { TemplateBuilder } from 'umbraco-cypress-testhelpers';

context('Templates', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
    });

    function navigateToSettings() {
        cy.umbracoSection('settings');
        cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");
    }

    function createTemplate() {
        navigateToSettings();
        cy.umbracoTreeItem("settings", ["Templates"]).rightclick();
        cy.umbracoContextMenuAction("action-create").click();
    }



    it('Create template', () => {
        const name = "Test template test";
        cy.umbracoEnsureTemplateNameNotExists(name);

        createTemplate();
        //Type name
        cy.umbracoEditorHeaderName(name);
        /* Make an edit, if you don't the file will be create twice,
        only happens in testing though, probably because the test is too fast
        Certifiably mega wonk regardless */
        cy.get('.ace_text-input').type("var num = 5;", {force:true} );

        //Save
        cy.get('.btn-success').click();

        //Assert
        cy.umbracoSuccessNotification().should('be.visible');

        //Clean up
        cy.umbracoEnsureTemplateNameNotExists(name);
    });

    it('Unsaved changes stay', () => {
        const name = "Templates Unsaved Changes Stay test";
        const edit = "var num = 5;";
        cy.umbracoEnsureTemplateNameNotExists(name);

        const template = new TemplateBuilder()
            .withName(name)
            .withContent('@inherits Umbraco.Web.Mvc.UmbracoViewPage\n')
            .build();

        cy.saveTemplate(template);

        navigateToSettings();

        // Open partial view
        cy.umbracoTreeItem("settings", ["Templates", name]).click();
        // Edit
        cy.get('.ace_text-input').type(edit, {force:true} );

        // Navigate away
        cy.umbracoSection('content');
        // Click stay button
        cy.get('umb-button[label="Stay"] button:enabled').click();

        // Assert
        // That the same document is open
        cy.get('#headerName').should('have.value', name);
        cy.get('.ace_content').contains(edit);

        cy.umbracoEnsureTemplateNameNotExists(name);
    });

    it('Discard unsaved changes', () => {
        const name = "Discard changes test";
        const edit = "var num = 5;";

        cy.umbracoEnsureTemplateNameNotExists(name);

        const template = new TemplateBuilder()
            .withName(name)
            .withContent('@inherits Umbraco.Web.Mvc.UmbracoViewPage\n')
            .build();

        cy.saveTemplate(template);

        navigateToSettings();

        // Open partial view
        cy.umbracoTreeItem("settings", ["Templates", name]).click();
        // Edit
        cy.get('.ace_text-input').type(edit, {force:true} );

        // Navigate away
        cy.umbracoSection('content');
        // Click discard
        cy.get('umb-button[label="Discard changes"] button:enabled').click();
        // Navigate back
        cy.umbracoSection('settings');

        // Asserts
        cy.get('.ace_content').should('not.contain', edit);
        // cy.umbracoPartialViewExists(fileName).then(exists => { expect(exists).to.be.false; }); TODO: Switch to template
        cy.umbracoEnsureTemplateNameNotExists(name);
    });

    it('Insert macro', () => {
        const name = 'InsertMacroTest';

        cy.umbracoEnsureTemplateNameNotExists(name);
        cy.umbracoEnsureMacroNameNotExists(name);

        const template = new TemplateBuilder()
            .withName(name)
            .withContent('')
            .build();

        cy.saveTemplate(template);

        cy.saveMacro(name);

        navigateToSettings();
        cy.umbracoTreeItem("settings", ["Templates", name]).click();
        // Insert macro
        cy.umbracoButtonByLabelKey('general_insert').click();
        cy.get('.umb-insert-code-box__title').contains('Macro').click();
        cy.get('.umb-card-grid-item').contains(name).click();

        // Assert
        cy.get('.ace_content').contains('@Umbraco.RenderMacro("' + name + '")').should('exist');

        // Clean
        cy.umbracoEnsureTemplateNameNotExists(name);
        cy.umbracoEnsureMacroNameNotExists(name);
    });

    it('Insert value', () => {
        const name = 'Insert Value Test';

        cy.umbracoEnsureTemplateNameNotExists(name);

        const partialView = new TemplateBuilder()
            .withName(name)
            .withContent('')
            .build();

        cy.saveTemplate(partialView);

        navigateToSettings();
        cy.umbracoTreeItem("settings", ["Templates", name]).click();

        // Insert value
        cy.umbracoButtonByLabelKey('general_insert').click();
        cy.get('.umb-insert-code-box__title').contains('Value').click();
        cy.get('select').select('umbracoBytes');
        cy.umbracoButtonByLabelKey('general_submit').click();

        // assert
        cy.get('.ace_content').contains('@Model.Value("umbracoBytes")').should('exist');

        // Clean
        cy.umbracoEnsureTemplateNameNotExists(name);
    });

});
