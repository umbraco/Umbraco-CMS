/// <reference types="Cypress" />
context('Partial View Macro Files', () => {

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
  });

  it('Create new partial view macro', () => {
    const name = "TestPartialViewMacro";
    const fileName = name + ".cshtml";

   cy.umbracoEnsurePartialViewMacroFileNameNotExists(fileName);
   cy.umbracoEnsureMacroNameNotExists(name);

    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

    cy.umbracoTreeItem("settings", ["Partial View Macro Files"]).rightclick();

    cy.umbracoContextMenuAction("action-create").click();
    cy.get('.menu-label').first().click(); // TODO: Fucked we cant use something like cy.umbracoContextMenuAction("action-label").click();

    //Type name
    cy.umbracoEditorHeaderName(name);

    //Save
    cy.get('.btn-success').click();

    //Assert
    cy.umbracoSuccessNotification().should('be.visible');

    //Clean up
    cy.umbracoEnsurePartialViewMacroFileNameNotExists(fileName);
    cy.umbracoEnsureMacroNameNotExists(name);
   });

});
