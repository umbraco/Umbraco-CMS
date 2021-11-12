/// <reference types="Cypress" />
context('Member Types', () => {

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
  });

  it('Create member type', () => {
    const name = "Test member type";

    cy.umbracoEnsureMemberTypeNameNotExists(name);

    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

    cy.umbracoTreeItem("settings", ["Member Types"]).rightclick();

    cy.umbracoContextMenuAction("action-create").click();

    //Type name
    cy.umbracoEditorHeaderName(name);


    cy.get('[data-element="group-add"]').click();

    cy.get('.umb-group-builder__group-title-input').type('Group name');
    cy.get('[data-element="property-add"]').click();
    cy.get('.editor-label').type('property name');
    cy.get('[data-element="editor-add"]').click();

    //Search for textstring
    cy.get('#datatype-search').type('Textstring');

    // Choose first item
    cy.get('ul.umb-card-grid [title="Textstring"]').closest("li").click();

    // Save property
    cy.get('.btn-success').last().click();

    //Save
    cy.get('.btn-success').click();

    //Assert
    cy.umbracoSuccessNotification().should('be.visible');

    //Clean up
    cy.umbracoEnsureMemberTypeNameNotExists(name);
   });

});
