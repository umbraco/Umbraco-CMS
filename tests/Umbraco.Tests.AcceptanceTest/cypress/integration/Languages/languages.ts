/// <reference types="Cypress" />
context('Languages', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'), false);
      });

    it('Deletes language', () => {
        // Setup
        const language1 = 'da';
        const language2 = 'en-GB';
        cy.umbracoEnsureLanguageNotExists(language1);
        cy.umbracoEnsureLanguageNotExists(language2);
        cy.umbracoCreateLanguage(language1, true, '1');
        cy.umbracoCreateLanguage(language2, true, '1');
        //Enter settings section and wait for everything to load
        cy.umbracoSection('settings');
        cy.get('.umb-box-content').should('be.visible');
        cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

        // Enter language tree and select the language we just created
        cy.umbracoTreeItem('settings', ['Languages']).click();

        // Assert there are 3 languages
        cy.get('tbody > tr').should('have.length', 3);

        // Delete UK Language
        cy.get('umb-button[label-key="general_delete"]').last().click();
        cy.umbracoButtonByLabelKey('contentTypeEditor_yesDelete').click();

        // Assert there is only 2 languages
        cy.get('tbody > tr').should('have.length', 2);

        // Cleanup
        cy.umbracoEnsureLanguageNotExists(language1);
        cy.umbracoEnsureLanguageNotExists(language2);
    });

});
