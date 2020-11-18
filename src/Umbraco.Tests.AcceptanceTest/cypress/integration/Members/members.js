/// <reference types="Cypress" />
context('Members', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
    });

    it('Create member', () => {
        const name = "Alice Bobson";
        const email = "alice-bobson@acceptancetest.umbraco";
        const password = "$AUlkoF*St0kgPiyyVEk5iU5JWdN*F7&";
        const passwordTimeout = 20000

        cy.umbracoEnsureMemberEmailNotExists(email);
        cy.umbracoSection('member');
        cy.get('li .umb-tree-root:contains("Members")').should("be.visible");

        cy.umbracoTreeItem("member", ["Members"]).rightclick();

        cy.umbracoContextMenuAction("action-create").click();
        cy.get('.menu-label').first().click();

        //Type name
        cy.umbracoEditorHeaderName(name);

        cy.get('input#_umb_login').clear().type(email);
        cy.get('input#_umb_email').clear().type(email);
        cy.get('input#password').clear().type(password, { timeout: passwordTimeout });
        cy.get('input#confirmPassword').clear().type(password, { timeout: passwordTimeout });

        // Save
        cy.get('.btn-success').click();

        //Assert
        cy.umbracoSuccessNotification().should('be.visible');

        //Clean up
        cy.umbracoEnsureMemberEmailNotExists(email);

    });

});
