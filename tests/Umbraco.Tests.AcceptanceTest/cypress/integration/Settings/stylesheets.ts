/// <reference types="Cypress" />
context('Stylesheets', () => {

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
  });

  afterEach(() => {
    //Clean up, this ensures even if tests break we clean up
    cy.umbracoEnsureStylesheetNameNotExists(fileName);
  });

  let fileName = "";


  it('Create new style sheet file', () => {
    const name = "TestStylesheet";
    fileName = name + ".css";

   cy.umbracoEnsureStylesheetNameNotExists(fileName);

    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

    cy.umbracoTreeItem("settings", ["Stylesheets"]).rightclick();

    cy.umbracoContextMenuAction("action-create").click();
    cy.get('.menu-label').first().click(); // TODO: Fucked we cant use something like cy.umbracoContextMenuAction("action-mediaType").click();

    //Type name
    cy.umbracoEditorHeaderName(name);

    //Save
    cy.get('.btn-success').click();

    //Assert
    cy.umbracoSuccessNotification().should('be.visible');

   });

   it('Deletes a stylesheet', () => {
    const name = "TestStylesheet";
    fileName = name + ".css";

    //Ensure stylesheet doesn't already exists
    cy.umbracoEnsureStylesheetNameNotExists(fileName);

    var stylesheetData =
    {
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

    //Navigate to settings ection
    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

    //Open stylesheet tree
    cy.get('[data-element="tree-item-stylesheets"] > .umb-tree-item__inner > .umb-tree-item__arrow').click();
    //Delete stylesheet
    cy.get('.umb-tree-item__inner > .umb-tree-item__label').contains(name).rightclick();
    cy.get('.umb-action-link').click();
    cy.get('[ng-if="showConfirm"]').click();
    //Assert
    cy.get('.umb-tree-item__inner > .umb-tree-item__label').contains(name).should('not.exist');

   });

});
