/// <reference types="Cypress" />

context('Login', () => {	

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'), false);
    });

    it('Uploads an image', () => {
        cy.umbracoSection('media');

        const logo = 'images/logo.png';
        cy.get('.dropzone').attachFile(logo, { subjectType: 'drag-n-drop' });
        // Assert
        cy.contains('Logo').should('exist');

        // Clean
    });
});