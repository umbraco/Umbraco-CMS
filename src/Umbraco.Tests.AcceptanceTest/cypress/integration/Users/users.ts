/// <reference types="Cypress" />
context('Users', () => {

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
  });

  it('Create user', () => {
    const name = "Alice Bobson";
    const email = "alice-bobson@acceptancetest.umbraco";

    cy.umbracoEnsureUserEmailNotExists(email);
    cy.umbracoSection('users');
    cy.umbracoButtonByLabelKey("user_createUser").click();


    cy.get('input[name="name"]').type(name);
    cy.get('input[name="email"]').type(email);

    cy.get('.umb-node-preview-add').click();
    cy.get('.umb-user-group-picker-list-item:nth-child(1) > .umb-user-group-picker__action').click();
    cy.get('.umb-user-group-picker-list-item:nth-child(2) > .umb-user-group-picker__action').click();
    cy.get('.btn-success').click();

    cy.get('.umb-button > .btn > .umb-button__content').click();


    cy.umbracoButtonByLabelKey("user_goToProfile").should('be.visible');

    //Clean up
    cy.umbracoEnsureUserEmailNotExists(email);

  });

});
