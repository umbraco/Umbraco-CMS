context('User Groups', () => {

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
  });

  it('Create user group', () => {
    const name = "Test Group";

    cy.umbracoEnsureUserGroupNameNotExists(name);

    cy.umbracoSection('users');
    cy.get('[data-element="sub-view-userGroups"]').click();

    cy.umbracoButtonByLabelKey("actions_createGroup").click();

    //Type name
    cy.umbracoEditorHeaderName(name);

    // Assign sections
    cy.get('.umb-box:nth-child(1) .umb-property:nth-child(1) localize').click();
    cy.get('.umb-tree-item__inner').click({multiple:true, timeout: 10000});
    cy.get('.btn-success').last().click();

    // Save
    cy.get('.btn-success').click();

    //Assert
    cy.umbracoSuccessNotification().should('be.visible');

    //Clean up
    cy.umbracoEnsureUserGroupNameNotExists(name);
   });

});
