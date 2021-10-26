// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
//
//
// -- This is a parent command --
// Cypress.Commands.add("login", (email, password) => { ... })
//
//
// -- This is a child command --
// Cypress.Commands.add("drag", { prevSubject: 'element'}, (subject, options) => { ... })
//
//
// -- This is a dual command --
// Cypress.Commands.add("dismiss", { prevSubject: 'optional'}, (subject, options) => { ... })
//
//
// -- This will overwrite an existing command --
// Cypress.Commands.overwrite("visit", (originalFn, url, options) => { ... })

import {Command} from 'umbraco-cypress-testhelpers';
import {Chainable} from './chainable';
import { JsonHelper } from 'umbraco-cypress-testhelpers';
new Chainable();
new Command().registerCypressCommands();

Cypress.Commands.add('umbracoCreateLanguage', (culture, isMandatory = false, fallbackLanguageId = 1) => {

    var langData =
    {
        "culture": culture,
        "isMandatory": isMandatory,
        "fallbackLanguageId": fallbackLanguageId        
    };

    // Create language through API
    cy.getCookie('UMB-XSRF-TOKEN', { log: false }).then((token) => {
      cy.request({
        method: 'POST',
        url: '/umbraco/backoffice/umbracoapi/language/SaveLanguage',
        followRedirect: true,
        headers: {
          Accept: 'application/json',
          'X-UMB-XSRF-TOKEN': token.value,
        },
        body: langData,
        log: false,
      }).then((response) => {        
        return;
      });
    });   
}); 

Cypress.Commands.add('umbracoEnsureLanguageNotExists', (culture) => {
    cy.getCookie('UMB-XSRF-TOKEN', { log: false }).then((token) => {
        console.log('hit commands')
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
              if (sb.culture === culture) {
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