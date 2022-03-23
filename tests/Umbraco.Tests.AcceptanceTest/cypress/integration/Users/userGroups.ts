/// <reference types="Cypress" />
import { UserGroupBuilder } from 'umbraco-cypress-testhelpers';

context('User Groups', () => {
  
  function navigateToUserGroups() {
    cy.umbracoSection('users');
    cy.get('[data-element="sub-view-userGroups"]').click();
  }

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
  });

  it('Create user group', () => {
    const name = "Test Group";

    cy.umbracoEnsureUserGroupNameNotExists(name);

    navigateToUserGroups();
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

   it('Can delete user group', () => {
     // Create user group
     const groupName = "Delete user group test"
     cy.umbracoEnsureUserGroupNameNotExists(groupName);

     const userGroup = new UserGroupBuilder()
       .withName(groupName)
       .build();

     cy.saveUserGroup(userGroup);
     navigateToUserGroups();

     // Delete the user group
     cy.get('.umb-table-body > :nth-child(2)').click();
     cy.umbracoButtonByLabelKey("general_delete").click();
     cy.get('umb-button[alias="overlaySubmit"]').click();

     cy.umbracoSuccessNotification().should('be.visible');
     cy.get('.umb-table-body').contains(groupName).should('not.exist');

     cy.umbracoEnsureUserGroupNameNotExists(groupName);
   });

   it('Cannot delete required groups', () => {
     navigateToUserGroups();

     // There's not really a good way to be 100% sure we'll get the admin group, it should be first, but who knows
     // so double check that we actually got the correct one
     const administrators = cy.get('.umb-table-body > :nth-child(1)'); 
     administrators.should('contain', 'Administrators');
     administrators.click({force: true});

     const sensitive = cy.get('.umb-table-body > :nth-child(3)'); 
     sensitive.should('contain', 'Sensitive data');
     sensitive.click({force: true});

     const translators = cy.get('.umb-table-body > :nth-child(4)'); 
     translators.should('contain', 'Translators');
     translators.click({force: true});

     // Now that we've clicked all that we shouldn't be able to delete, ensure that the delete button does not show up 
     cy.get('.umb-editor-sub-header').should('not.contain', 'Delete');
   });
});

