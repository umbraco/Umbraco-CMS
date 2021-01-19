/// <reference types="Cypress" />
context('Languages', () => {

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
  });

  it('Add language', () => {
    const name = "Kyrgyz (Kyrgyzstan)"; // Must be an option in the select box

     cy.umbracoEnsureLanguageNameNotExists(name);

    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

    cy.umbracoTreeItem("settings", ["Languages"]).click();

    cy.umbracoButtonByLabelKey("languages_addLanguage").click();

    cy.get('select[name="newLang"]').select(name);

    // //Save
    cy.get('.btn-success').click();

    //Assert
    cy.umbracoSuccessNotification().should('be.visible');

    //Clean up
    cy.umbracoEnsureLanguageNameNotExists(name);
   });

});
