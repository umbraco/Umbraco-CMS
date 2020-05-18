/// <reference types="Cypress" />
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
    cy.get('.btn-success').click();

    //Assert
    cy.umbracoSuccessNotification().should('be.visible');

    //Clean up
    cy.umbracoEnsureTemplateNameNotExists(name);
   });

});
