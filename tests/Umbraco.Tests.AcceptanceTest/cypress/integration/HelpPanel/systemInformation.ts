/// <reference types="Cypress" />

function openSystemInformation(){
    //We have to wait for page to load, if the site is slow
    cy.get('[data-element="global-help"]').should('be.visible').click();
    cy.get('.umb-help-list-item').last().should('be.visible').click();
    cy.get('.umb-drawer-content').scrollTo('bottom', {ensureScrollable : false});
}

context('System Information', () => {

    beforeEach(() => {
        //arrange
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
        cy.umbracoSetCurrentUserLanguage('en-US');
    });
    afterEach(() => {
        cy.umbracoSetCurrentUserLanguage('en-US');
    });

    it('Check System Info Displays', () => {
        openSystemInformation();
        
        // Assert
        cy.get('.table').find('tr').should('contain', 'Server OS');
        cy.get('.table').find('tr').should('contain', 'Server Framework');
        cy.get('.table').find('tr').should('contain', 'Default Language');
        cy.get('.table').find('tr').should('contain', 'Umbraco Version');
        cy.get('.table').find('tr').should('contain', 'Current Culture');
        cy.get('.table').find('tr').should('contain', 'Current UI Culture');
        cy.get('.table').find('tr').should('contain', 'Current Webserver');
        cy.get('.table').find('tr').should('contain', 'Models Builder Mode');
        cy.get('.table').find('tr').should('contain', 'Debug Mode');
        cy.get('.table').find('tr').should('contain', 'Database Provider');
        cy.get('.table').find('tr').should('contain', 'Current Server Role');
        cy.get('.table').find('tr').should('contain', 'Browser');
        cy.get('.table').find('tr').should('contain', 'Browser OS');
        cy.contains('Default Language').parent().should('contain', 'en-US');
        cy.contains('Current Culture').parent().should('contain', 'en-US');
        cy.contains('Current UI Culture').parent().should('contain', 'en-US');
    });

    it('Checks language displays correctly after switching', () => {

        //Navigate to edit user and change language
        cy.umbracoGlobalUser().click();
        cy.get('[alias="editUser"]').click();
        cy.get('[name="culture"]').select('string:da-DK', { force: true});
        cy.umbracoButtonByLabelKey('buttons_save').click({force: true});
        cy.umbracoSuccessNotification().should('be.visible');

        openSystemInformation();
        //Assert
        cy.contains('Current Culture').parent().should('contain', 'da-DK');
        cy.contains('Current UI Culture').parent().should('contain', 'da-DK');
        cy.get('.umb-button__content').last().click();
        //Clean
        cy.umbracoSetCurrentUserLanguage('en-US');
    });
});
