/// <reference types="Cypress" />

function openSystemInformation(){
    //We have to wait for page to load, if the site is slow
    cy.get('[data-element="global-help"]').should('be.visible', {timeout:10000}).click();
    cy.get('.umb-help-list-item').last().should('be.visible').click();
    cy.get('.umb-drawer-content').scrollTo('bottom', {ensureScrollable : false});
}

context('System Information', () => {

    beforeEach(() => {
        //arrange
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
        cy.umbracoSetCurrentUserLanguage('en-US');
    });

    it('Check System Info Displays', () => {
        openSystemInformation();
        cy.get('.table').find('tr').should('have.length', 10);
        cy.contains('Current Culture').parent().should('contain', 'en-US');
        cy.contains('Current UI Culture').parent().should('contain', 'en-US');
    });
    
    it('Checks language displays correctly after switching', () => {

        //Navigate to edit user and change language
        cy.get('[data-element="global-user"]').click();
        cy.get('[alias="editUser"]').click();
        cy.get('[name="culture"]').select('string:da-DK', {timeout: 10000, force: true});
        cy.umbracoButtonByLabelKey('buttons_save').click({force: true});
        //Refresh site to display new language
        cy.reload();
        openSystemInformation();
        //Assert
        cy.contains('Current Culture').parent().should('contain', 'da-DK');
        cy.contains('Current UI Culture').parent().should('contain', 'da-DK');
        cy.get('.umb-button__content').last().click();
    });
});
