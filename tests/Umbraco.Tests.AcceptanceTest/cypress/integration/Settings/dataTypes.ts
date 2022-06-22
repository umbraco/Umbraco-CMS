/// <reference types="Cypress" />
import {LabelDataTypeBuilder} from 'umbraco-cypress-testhelpers';
context('Data Types', () => {

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
  });

  it('Create data type', () => {
    const name = "Test data type";

   cy.umbracoEnsureDataTypeNameNotExists(name);

    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

    cy.umbracoTreeItem("settings", ["Data Types"]).rightclick();

    cy.umbracoContextMenuAction("action-create").click();
    cy.umbracoContextMenuAction("action-data-type").click();

    //Type name
    cy.umbracoEditorHeaderName(name);


    cy.get('select[name="selectedEditor"]', {timeout: 5000}).select('Label');

    cy.get('.umb-property-editor select').select('Time');

    //Save
    cy.get('.btn-success').click();

    //Assert
    cy.umbracoSuccessNotification().should('be.visible');

    //Clean up
    cy.umbracoEnsureDataTypeNameNotExists(name);
   });

   it('Delete data type', () => {
    const name = "Test data type";
    cy.umbracoEnsureDataTypeNameNotExists(name);

    const dataType = new LabelDataTypeBuilder()
      .withSaveNewAction()
      .withName(name)
      .build();

    cy.saveDataType(dataType);

    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

    cy.umbracoTreeItem("settings", ["Data Types", name]).rightclick();

    cy.umbracoContextMenuAction("action-delete").click();

    cy.umbracoButtonByLabelKey("general_delete").click();

    cy.contains(name).should('not.exist');

    cy.umbracoEnsureDataTypeNameNotExists(name);


  });
});
