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

  it('Update user', () => {
    // Set userdata
    const name = "Alice Bobson";
    const email = "alice-bobson@acceptancetest.umbraco";
    const startContentIds = [];
    const startMediaIds = [];
    const userGroups = ["admin"];    
    
    var userData =
    {
        "id": -1,
        "parentId": -1,
        "name": name,
        "username": email,
        "culture": "en-US",
        "email": email,
        "startContentIds": startContentIds,
        "startMediaIds": startMediaIds,
        "userGroups": userGroups,
        "message": ""
    };

    // Ensure user doesn't exist
    cy.umbracoEnsureUserEmailNotExists(email);
  
    // Create user through API
    cy.getCookie('UMB-XSRF-TOKEN', { log: false }).then((token) => {
      cy.request({
        method: 'POST',
        url: '/umbraco/backoffice/umbracoapi/users/PostCreateUser',
        followRedirect: true,
        headers: {
          Accept: 'application/json',
          'X-UMB-XSRF-TOKEN': token.value,
        },
        body: userData,
        log: false,
      }).then((response) => {        
        return;
      });
    });

    // Go to the user and edit their name
    cy.umbracoSection('users');
    cy.get('.umb-user-card__name').contains(name).click();
    cy.get('#headerName').type('{movetoend}son');
    cy.umbracoButtonByLabelKey('buttons_save').click();

    // assert save succeeds
    cy.umbracoSuccessNotification().should('be.visible');
    cy.umbracoEnsureUserEmailNotExists(email);
  })
  
  it('Delete user', () => {
    // Set userdata
    const name = "Alice Bobson";
    const email = "alice-bobson@acceptancetest.umbraco";
    const startContentIds = [];
    const startMediaIds = [];
    const userGroups = ["admin"];    

    var userData =
    {
        "id": -1,
        "parentId": -1,
        "name": name,
        "username": email,
        "culture": "en-US",
        "email": email,
        "startContentIds": startContentIds,
        "startMediaIds": startMediaIds,
        "userGroups": userGroups,
        "message": ""
    };

    // Ensure user doesn't exist
    cy.umbracoEnsureUserEmailNotExists(email);

    // Create user through API
    cy.getCookie('UMB-XSRF-TOKEN', { log: false }).then((token) => {
      cy.request({
        method: 'POST',
        url: '/umbraco/backoffice/umbracoapi/users/PostCreateUser',
        followRedirect: true,
        headers: {
          Accept: 'application/json',
          'X-UMB-XSRF-TOKEN': token.value,
        },
        body: userData,
        log: false,
      }).then((response) => {        
        return;
      });
    });

    // Go to the user and delete them
    cy.umbracoSection('users');
    cy.get('.umb-user-card__name').contains(name).click();
    cy.umbracoButtonByLabelKey("user_deleteUser").click();
    cy.get('umb-button[label="Yes, delete"]').click();

    // assert deletion succeeds
    cy.umbracoSuccessNotification().should('be.visible');
    cy.umbracoEnsureUserEmailNotExists(email);
  })
});