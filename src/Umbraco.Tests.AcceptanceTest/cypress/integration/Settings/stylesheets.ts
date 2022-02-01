/// <reference types="Cypress" />
context('Stylesheets', () => {

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
  });

  it('Create new style sheet file', () => {
    const name = "TestStylesheet";
    const fileName = name + ".css";

   cy.umbracoEnsureStylesheetNameNotExists(fileName);

    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

    cy.umbracoTreeItem("settings", ["Stylesheets"]).rightclick();

    cy.umbracoContextMenuAction("action-create").click();
    cy.get('.menu-label').first().click(); // TODO: Fucked we cant use something like cy.umbracoContextMenuAction("action-mediaType").click();
    //We have to wait here till everything is loaded, or worker will throw error
    cy.intercept('/umbraco/lib/ace-builds/src-min-noconflict/worker-css.js').as('aceWorker');
    cy.wait('@aceWorker');
    //Type name
    cy.umbracoEditorHeaderName(name);

    //Save
    cy.get('.btn-success').click();

    //Assert
    cy.umbracoSuccessNotification().should('be.visible');

    //Clean up
    cy.umbracoEnsureStylesheetNameNotExists(fileName);
   });

});
