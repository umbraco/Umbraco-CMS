/// <reference types="Cypress" />
context('Languages', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'), false);
      });

    it('Updates language', () => {
        // Setup
        const language = 'Danish';
        cy.umbracoEnsureLanguageNotExists(language);
        cy.umbracoCreateLanguage('da', true, '1');
        cy.umbracoSection('settings');

        // Enter language tree and select the language we just created
        cy.umbracoTreeItem('settings', ['Languages']).click();
        cy.get('tr').contains('Danish').click();

        // Edit the language
        cy.get('div[ng-click="vm.toggleMandatory()"]').children('button').click(); // the mandatory language toggle
        cy.get('select[name="fallbackLanguage"]').select('No fall back language')       

        // Save and assert success
        cy.umbracoButtonByLabelKey('buttons_save').click();
        cy.umbracoSuccessNotification().should('be.visible');

        // Cleanup
        cy.umbracoEnsureLanguageNotExists(language);
    });

});