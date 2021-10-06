/// <reference types="Cypress" />
import { JsonHelper } from 'umbraco-cypress-testhelpers';

context('Languages', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'), false);
      });

    it('Creates language', () => {
        // Setup
        const language = 'Danish';
        cy.umbracoEnsureLanguageNotExists(language);
        cy.umbracoSection('settings');

        // Enter language tree and create new language
        cy.umbracoTreeItem('settings', ['Languages']).click();
        cy.umbracoButtonByLabelKey('languages_addLanguage').click();
        cy.get('select[name="newLang"]').select(language);

        // Save and assert success
        cy.umbracoButtonByLabelKey('buttons_save').click();
        cy.umbracoSuccessNotification().should('be.visible');

        // Cleanup
        cy.umbracoEnsureLanguageNotExists(language);
    });

});

Cypress.Commands.add('umbracoEnsureLanguageNotExists', (name) => {
    cy.getCookie('UMB-XSRF-TOKEN', { log: false }).then((token) => {
        cy.request({
          method: 'GET',
          url: '/umbraco/backoffice/umbracoapi/language/GetAllLanguages',
          followRedirect: true,
          headers: {
            Accept: 'application/json',
            'X-UMB-XSRF-TOKEN': token.value,
          },
          log: false,
        }).then((response) => {
          const searchBody = JsonHelper.getBody(response);
          if (searchBody.length > 0) {
            let languageId = null;
            for (const sb of searchBody) {
              if (sb.name === name) {
                languageId = sb.id;
              }
            }
  
            if (languageId !== null) {
              cy.request({
                method: 'POST',
                url: '/umbraco/backoffice/umbracoapi/language/DeleteLanguage?id=' + languageId,
                followRedirect: false,
                headers: {
                  ContentType: 'application/json',
                  'X-UMB-XSRF-TOKEN': token.value,
                },
              }).then((resp) => {
                return;
              });
            }
          }
        });
      });
});