/// <reference types="Cypress" />
import { PartialViewBuilder } from "umbraco-cypress-testhelpers";

context('Partial Views', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
    });

    function navigateToSettings() {
        cy.umbracoSection('settings');
        cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");
    }

    function openPartialViewsCreatePanel() {
        navigateToSettings();
        cy.umbracoTreeItem("settings", ["Partial Views"]).rightclick();
    }

    it('Create new empty partial view', () => {
        const name = "TestPartialView";
        const fileName = name + ".cshtml";

        cy.umbracoEnsurePartialViewNameNotExists(fileName);

        openPartialViewsCreatePanel();

        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.menu-label').first().click(); // TODO: Fucked we cant use something like cy.umbracoContextMenuAction("action-mediaType").click();

        //Type name
        cy.umbracoEditorHeaderName(name);

        //Save
        cy.get('.btn-success').click();

        //Assert
        cy.umbracoSuccessNotification().should('be.visible');
        cy.umbracoPartialViewExists(fileName).then(exists => { expect(exists).to.be.true; });

        //Clean up
        cy.umbracoEnsurePartialViewNameNotExists(fileName);
    });

    it('Create partial view from snippet', () => {
        const name = "TestPartialViewFromSnippet";
        const fileName = name + ".cshtml";

        cy.umbracoEnsurePartialViewNameNotExists(fileName);

        openPartialViewsCreatePanel();

        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.menu-label').eq(1).click();
        // Select snippet
        cy.get('.menu-label').eq(2).click();

        // Type name
        cy.umbracoEditorHeaderName(name);

        // Save
        cy.get('.btn-success').click();

        // Assert
        cy.umbracoSuccessNotification().should('be.visible');
        cy.umbracoPartialViewExists(fileName).then(exists => { expect(exists).to.be.true; });

        // Clean up
        cy.umbracoEnsurePartialViewNameNotExists(fileName);
    });

    it('Partial view with no name', () => {
        openPartialViewsCreatePanel();

        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.menu-label').first().click();

        // The test would fail intermittently, most likely because the editor didn't have time to load
        // This should ensure that the editor is loaded and the test should no longer fail unexpectedly.
        cy.get('.ace_content', {timeout: 5000}).should('exist');

        // Click save
        cy.get('.btn-success').click();

        // Asserts
        cy.umbracoErrorNotification().should('be.visible');
    });

    it('Delete partial view', () => {
        const name = "TestDeletePartialView";
        const fileName = name + ".cshtml";

        cy.umbracoEnsurePartialViewNameNotExists(fileName);

        // Build and save partial view
        const partialView = new PartialViewBuilder()
            .withName(name)
            .withContent("@inherits Umbraco.Web.Mvc.UmbracoViewPage")
            .build();

        cy.savePartialView(partialView);

        navigateToSettings();

        // Delete partial view
        cy.umbracoTreeItem("settings", ["Partial Views", fileName]).rightclick();
        cy.umbracoContextMenuAction("action-delete").click();
        cy.umbracoButtonByLabelKey("general_ok").click();

        // Assert
        cy.contains(fileName).should('not.exist');
        cy.umbracoPartialViewExists(fileName).then(exists => { expect(exists).to.be.false; });

        // Clean
        cy.umbracoEnsurePartialViewNameNotExists(fileName);
    });

    it('Edit partial view', () => {
        const name = 'EditPartialView';
        const fileName = name + ".cshtml";

        cy.umbracoEnsurePartialViewNameNotExists(fileName);

        const partialView = new PartialViewBuilder()
            .withName(name)
            .withContent("@inherits Umbraco.Web.Mvc.UmbracoViewPage\n")
            .build();

        cy.savePartialView(partialView);

        navigateToSettings();
        // Open partial view
        cy.umbracoTreeItem("settings", ["Partial Views", fileName]).click();
        // Edit
        cy.get('.ace_text-input').type("var num = 5;", {force:true} );
        cy.get('.btn-success').click();

        // Assert
        cy.umbracoSuccessNotification().should('be.visible');
        // Clean
        cy.umbracoEnsurePartialViewNameNotExists(fileName);
    });


});
