/// <reference types="Cypress" />
context('Stylesheets', () => {

  const name = "TestStylesheet";
  const fileName = name + ".css";

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));

    cy.umbracoEnsureStylesheetNameNotExists(fileName);
  });

  afterEach(() => {
    // Clean up, this ensures that even if tests break we clean up
    cy.umbracoEnsureStylesheetNameNotExists(fileName);
  });

  it('Create new style sheet file', () => {

    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

    cy.umbracoTreeItem("settings", ["Stylesheets"]).rightclick();

    cy.umbracoContextMenuAction("action-create").click();
    cy.get('.menu-label').first().click(); // TODO: Would be better to use something like cy.umbracoContextMenuAction("action-mediaType").click();
    // We have to wait here till everything is loaded, or worker will throw error
    cy.intercept('/umbraco/lib/ace-builds/src-min-noconflict/worker-css.js').as('aceWorker');
    cy.wait('@aceWorker');

    // Type name
    cy.umbracoEditorHeaderName(name);

    // Save
    cy.get('.btn-success').click();

    // Assert
    cy.umbracoSuccessNotification().should('be.visible');

   });

   it('Deletes a stylesheet', () => {

    var stylesheetData = {
      "virtualPath": "/css/",
      "path": null,
      "name": name,
      "content": "",
      "fileType": "stylesheets",
      "snippet": null,
      "id": "0",
      "notifications": []
    }

    let url = '/umbraco/backoffice/umbracoapi/codefile/PostSave'
    cy.umbracoApiRequest(url, 'POST', stylesheetData);

    // Navigate to Settings section
    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

    // Open stylesheet tree
    cy.get('[data-element="tree-item-stylesheets"] > .umb-tree-item__inner > .umb-tree-item__arrow').click();

    // Delete stylesheet
    cy.get('.umb-tree-item__inner > .umb-tree-item__label').contains(name).rightclick();
    cy.get('.umb-action-link').click();
    cy.get('[ng-if="showConfirm"]').click();

    // Assert
    cy.get('.umb-tree-item__inner > .umb-tree-item__label').contains(name).should('not.exist');

   });
});
