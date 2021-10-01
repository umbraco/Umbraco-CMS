/// <reference types="Cypress" />

function openSystemInformation(){
    cy.get('[data-element="global-help"]').click();
    cy.get('.umb-help-list-item').should('be.visible').click();
    cy.get('.umb-drawer-content').scrollTo('bottom', {ensureScrollable : false});
}

context('System Information', () => {

    beforeEach(() => {
        //arrange
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
    });

    it('Check System Info Displays', () => {
        openSystemInformation();
        cy.get('.table').find('tr').should('have.length', 10)
    });

    it('Checks language displays correctly after switching', () => {
        //Navigate to edit user and change language
        cy.get('[data-element="global-user"]').click();
        cy.get('[alias="editUser"]').click();
        cy.get('.input-block-level').last().select('Danish (Denmark)');
        cy.umbracoButtonByLabelKey('buttons_save').click();
        //Refresh site to display new language
        cy.reload();
        openSystemInformation();
        //Assert
        cy.contains('Current Culture').parent().should('contain', 'da-DK');
        cy.contains('Current UI Culture').parent().should('contain', 'da-DK');

        //Clean
        cy.get('.input-block-level').last().select('English (United States)');
        cy.umbracoButtonByLabelKey('buttons_save').click();
        cy.reload();
    });
});