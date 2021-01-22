/// <reference types="Cypress" />
import {DocumentTypeBuilder, TemplateBuilder} from "umbraco-cypress-testhelpers";

context('Templates', () => {

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
  });

  it('Create template', () => {
        const name = "Test template";

        cy.umbracoEnsureTemplateNameNotExists(name);

        cy.umbracoSection('settings');
        cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

        cy.umbracoTreeItem("settings", ["Templates"]).rightclick();

        cy.umbracoContextMenuAction("action-create").click();

        //Type name
        cy.umbracoEditorHeaderName(name);

        //Save
        cy.get("form[name='contentForm']").submit();

        //Assert
        cy.umbracoSuccessNotification().should('be.visible');

        //Clean up
        cy.umbracoEnsureTemplateNameNotExists(name);
   });

    it('Delete template', () => {
        const name = "Test template";
        cy.umbracoEnsureTemplateNameNotExists(name);

        const template = new TemplateBuilder()
            .withName(name)
            .build();

        cy.saveTemplate(template);

        cy.umbracoSection('settings');
        cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

        cy.umbracoTreeItem("settings", ["Templates", name]).rightclick();
        cy.umbracoContextMenuAction("action-delete").click();

        cy.umbracoButtonByLabelKey("general_ok").click();

        cy.contains(name).should('not.exist');

        cy.umbracoEnsureTemplateNameNotExists(name);
    });
});
