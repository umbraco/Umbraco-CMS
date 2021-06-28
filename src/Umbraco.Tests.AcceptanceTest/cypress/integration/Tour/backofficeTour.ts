/// <reference types="Cypress" />

context('Backoffice Tour', () => {
    var timeout = 60000;
    beforeEach(() => {
        //arrange
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
        resetTourData();
    });

    it('Backoffice introduction tour should run', () => {
        //act
        cy.umbracoGlobalHelp().should("be.visible");
        cy.umbracoGlobalHelp().click();
        runBackOfficeIntroTour(0, 'Start');

        //assert
        cy.get('[data-element="help-tours"]').should("be.visible");
        cy.get('[data-element="help-tours"]').click();
        getPercentage(17, timeout);
    });

    it('Backoffice introduction tour should run then rerun', () => {
        //act
        cy.umbracoGlobalHelp().should("be.visible");
        cy.umbracoGlobalHelp().click();
        runBackOfficeIntroTour(0, 'Start', timeout);
        runBackOfficeIntroTour(17, 'Rerun', timeout);

        //assert
        cy.get('[data-element="help-tours"]').should("be.visible");
        cy.get('[data-element="help-tours"]').click();
        cy.umbracoGlobalHelp().should("be.visible");
        getPercentage(17, timeout);
    });

    afterEach(() => {
        //cleanup
        resetTourData();
    });
});

function getPercentage(percentage, timeout) {
    cy.get('[data-element="help-tours"] .umb-progress-circle', { timeout: timeout }).get('[percentage]').contains(percentage + '%');
}

function resetTourData() {
    var tourStatus =
    {
        "alias": "umbIntroIntroduction",
        "completed": false,
        "disabled": true
    };

    cy.getCookie('UMB-XSRF-TOKEN', { log: false }).then((token) => {
        cy.request({
            method: 'POST',
            url: '/umbraco/backoffice/UmbracoApi/CurrentUser/PostSetUserTour',
            followRedirect: false,
            headers: {
                ContentType: 'application/json',
                'X-UMB-XSRF-TOKEN': token.value,
            },
            body: tourStatus,
        }).then((resp) => {
            return;
        });
    })
}

function runBackOfficeIntroTour(percentageComplete, buttonText, timeout) {
    cy.get('[data-element="help-tours"]').should("be.visible");
    cy.get('[data-element="help-tours"]').click();
    cy.get('[data-element="help-tours"] .umb-progress-circle', { timeout: timeout }).get('[percentage]').contains(percentageComplete + '%', { timeout: timeout });
    cy.get('[data-element="help-tours"]').click();
    cy.get('[data-element="tour-umbIntroIntroduction"] .umb-button').should("be.visible");
    cy.get('[data-element="tour-umbIntroIntroduction"] .umb-button').contains(buttonText);
    cy.get('[data-element="tour-umbIntroIntroduction"] .umb-button').click();
    //act
    cy.get('.umb-tour-step', { timeout: timeout }).should('be.visible');
    cy.get('.umb-tour-step__footer').should('be.visible');
    cy.get('.umb-tour-step__counter').should('be.visible');

    for (let i = 1; i < 7; i++) {
        cy.get('.umb-tour-step__counter').contains(i + '/12');
        cy.get('.umb-tour-step__footer .umb-button').should('be.visible').click();
    }
    cy.umbracoGlobalUser().click()
    cy.get('.umb-tour-step__counter', { timeout: timeout }).contains('8/12');
    cy.get('.umb-tour-step__footer .umb-button').should('be.visible').click();
    cy.get('.umb-tour-step__counter', { timeout: timeout }).contains('9/12');
    cy.get('.umb-overlay-drawer__align-right .umb-button').should('be.visible').click();
    cy.get('.umb-tour-step__counter', { timeout: timeout }).contains('10/12');
    cy.umbracoGlobalHelp().click()

    for (let i = 11; i < 13; i++) {
        cy.get('.umb-tour-step__counter', { timeout: timeout }).contains(i + '/12');
        cy.get('.umb-tour-step__footer .umb-button').should('be.visible').click();
    }
    cy.get('.umb-tour-step__footer .umb-button').should('be.visible').click();

    cy.umbracoGlobalHelp().should("be.visible");
}
