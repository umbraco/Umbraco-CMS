/// <reference types="Cypress" />
context('Macros', () => {

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
  });

  it('Create macro', () => {
    const name = "Test macro";

    cy.umbracoEnsureMacroNameNotExists(name);

    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

    cy.umbracoTreeItem("settings", ["Macros"]).rightclick();

    cy.umbracoContextMenuAction("action-create").click();

    cy.get('form[name="createMacroForm"]').within(($form) => {
      cy.get('input[name="itemKey"]').type(name);
      cy.get(".btn-primary").click();
    });

    cy.location().should((loc) => {
      expect(loc.hash).to.include('#/settings/macros/edit/')
    });

    //Clean up
    cy.umbracoEnsureMacroNameNotExists(name);
   });

});
