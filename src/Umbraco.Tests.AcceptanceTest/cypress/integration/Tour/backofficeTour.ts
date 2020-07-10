/// <reference types="Cypress" />

context('Backoffice Tour', () => {

  beforeEach(() => {
    //arrange
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
    resetTourData();
  });

  it('Backoffice introduction tour should run', () => {
    //act
    cy.umbracoGlobalHelp().should("be.visible");
    cy.umbracoGlobalHelp().click();
    runBackOfficeIntroTour(0, 'Start');

    //assert
    cy.get('[data-element="help-tours"]').should("be.visible");
    cy.get('[data-element="help-tours"]').click();
    cy.get('[data-element="help-tours"] .umb-progress-circle', { timeout: 60000 }).contains('17%');
  });

  it('Backoffice introduction tour should run then rerun', () => {
    //act
    cy.umbracoGlobalHelp().should("be.visible");
    cy.umbracoGlobalHelp().click();
    runBackOfficeIntroTour(0, 'Start');
    runBackOfficeIntroTour(17, 'Rerun');

    //assert
    cy.get('[data-element="help-tours"]').should("be.visible");
    cy.get('[data-element="help-tours"]').click();
    cy.get('[data-element="help-tours"] .umb-progress-circle', { timeout: 60000 }).contains('17%');
    cy.umbracoGlobalHelp().should("be.visible");
  });
});

afterEach(() => {
  //cleanup
  resetTourData();
});

function resetTourData() {
  var tourStatus =
  {
    "alias": "umbIntroIntroduction",
    "completed": false,
    "disabled": false
  };

  cy.getCookie('UMB-XSRF-TOKEN', { log: false }).then((token) => {
    cy.request({
      method: 'POST',
      url: '/umbraco/backoffice/UmbracoApi/CurrentUser/PostSetUserTour',
      followRedirect: false,
      headers: {
        ContentType: 'application/json',
        'X-UMB-XSRF-TOKEN': token.value,
      },
      body: tourStatus,
    }).then((resp) => {
      return;
    });
  })
}

function runBackOfficeIntroTour(percentageComplete, buttonText) {
  cy.get('[data-element="help-tours"]').should("be.visible");
  cy.get('[data-element="help-tours"]').click();
  cy.get('[data-element="help-tours"] .umb-progress-circle', { timeout: 60000 }).contains(percentageComplete + '%');
  cy.get('[data-element="help-tours"]').click();
  cy.get('[data-element="tour-umbIntroIntroduction"] .umb-button').should("be.visible");
  cy.get('[data-element="tour-umbIntroIntroduction"] .umb-button').contains(buttonText);
  cy.get('[data-element="tour-umbIntroIntroduction"] .umb-button').click();
  //act
  cy.get('.umb-tour-step', { timeout: 60000 }).should('be.visible');
  cy.get('.umb-tour-step__footer').should('be.visible');
  cy.get('.umb-tour-step__counter').should('be.visible');

  for (let i = 1; i < 7; i++) {
    cy.get('.umb-tour-step__counter').contains(i + '/12');
    cy.get('.umb-tour-step__footer .umb-button').should('be.visible').click();
  }
  cy.umbracoGlobalUser().click()
  cy.get('.umb-tour-step__counter', { timeout: 10000 }).contains('8/12');
  cy.get('.umb-tour-step__footer .umb-button').should('be.visible').click();
  cy.get('.umb-tour-step__counter', { timeout: 10000 }).contains('9/12');
  cy.get('.umb-overlay-drawer__align-right .umb-button').should('be.visible').click();
  cy.get('.umb-tour-step__counter', { timeout: 10000 }).contains('10/12');
  cy.umbracoGlobalHelp().click()

  for (let i = 11; i < 13; i++) {
    cy.get('.umb-tour-step__counter', { timeout: 10000 }).contains(i + '/12');
    cy.get('.umb-tour-step__footer .umb-button').should('be.visible').click();
  }
  cy.get('.umb-tour-step__footer .umb-button').should('be.visible').click();
  
  cy.umbracoGlobalHelp().should("be.visible");
}