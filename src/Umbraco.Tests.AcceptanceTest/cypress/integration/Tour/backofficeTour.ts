/// <reference types="Cypress" />
context('Backoffice Tour', () => {

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
  });

  it('Backoffice introduction tour should run', () => {
    //arrange
    cy.umbracoGlobalHelp().should("be.visible");
    
    //act
    cy.umbracoGlobalHelp().click()
    //assert
    cy.get('[data-element="help-tours"]').should("be.visible");
    //act
    cy.get('[data-element="help-tours"]').click();
    //assert
    cy.get('[data-element="tour-umbIntroIntroduction"] .umb-button').should("be.visible");
    //act
    cy.get('[data-element="tour-umbIntroIntroduction"] .umb-button').click();
    //assert
    cy.get('.umb-tour-step', { timeout: 60000 }).should('be.visible');
    cy.get('.umb-tour-step__footer').should('be.visible'); 
    cy.get('.umb-tour-step__counter').should('be.visible');

    for(let i=1;i<7;i++){
      cy.get('.umb-tour-step__counter').contains(i + '/12');
      cy.get('.umb-tour-step__footer .umb-button').should('be.visible').click();
    }
    cy.umbracoGlobalUser().click()
    cy.get('.umb-tour-step__counter').contains('8/12');
    cy.get('.umb-tour-step__footer .umb-button').should('be.visible').click();
    cy.get('.umb-tour-step__counter').contains('9/12');
    cy.get('.umb-overlay-drawer__align-right .umb-button').should('be.visible').click();
    cy.get('.umb-tour-step__counter').contains('10/12');
    cy.umbracoGlobalHelp().click()

    for(let i=11;i<13;i++){
      cy.get('.umb-tour-step__counter').contains(i + '/12');
      cy.get('.umb-tour-step__footer .umb-button').should('be.visible').click();
    }
    cy.get('.umb-tour-step__footer .umb-button').should('be.visible').click();

    //assert
    cy.umbracoGlobalHelp().should("be.visible");
    cy.get('[data-element="help-tours"] .umb-progress-circle').contains('17%');
   });
});
