// type definitions for Cypress object "cy"
/// <reference types="Cypress" />

// type definitions for custom commands like "createDefaultTodos"
// <reference types="support" />

export {};

declare global {
    // eslint-disable-next-line @typescript-eslint/no-namespace
    namespace Cypress {
        interface Chainable<Subject> {
            /**
             * Checks to see if the language with specified culture does not exist
             * If it does it will automatically delete it
             * @param  {string} culture Culture of language to delete
             * @example cy.umbracoEnsureLanguageCultureNotExists('da-DK');
             */
             umbracoEnsureLanguageCultureNotExists: (culture: string) => Chainable<void>;
            /**
             * Creates a language from a culture
             * @param {string} culture Culture of the language - fx "da_DK"
             * @param {boolean} isMandatory Set whether the language is mandatory or not. Defaults to false
             * @param {string} fallbackLanguageId of the language to fallback to. Defaults to 1 which is en_US
             * @example cy.umbracoCreateLanguage('da', true, '1');
             */
            umbracoCreateLanguage: (culture: string, isMandatory: boolean, fallbackLanguageId: string) => Chainable<void>;
        }
    }
}