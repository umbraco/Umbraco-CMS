/// <reference types="Cypress" />
import { PartialViewMacroBuilder } from "umbraco-cypress-testhelpers";

context('Partial View Macro Files', () => {

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
  });

  function openPartialViewMacroCreatePanel() {
    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

    cy.umbracoTreeItem("settings", ["Partial View Macro Files"]).rightclick();
    cy.umbracoContextMenuAction("action-create").click();
  }

  function cleanup(name, extension = ".cshtml") {
    const fileName = name + extension;

    cy.umbracoEnsureMacroNameNotExists(name);
    cy.umbracoEnsurePartialViewMacroFileNameNotExists(fileName);
  }

  it('Create new partial view macro', () => {
    const name = "TestPartialViewMacro";

    cleanup(name);

    openPartialViewMacroCreatePanel();

    cy.get('.menu-label').first().click(); // TODO: Fucked we cant use something like cy.umbracoContextMenuAction("action-label").click();

    //Type name
    cy.umbracoEditorHeaderName(name);

    //Save
    cy.get('.btn-success').click();

    //Assert
    cy.umbracoSuccessNotification().should('be.visible');
    cy.umbracoMacroExists(name).then(exists => { expect(exists).to.be.true; });

    //Clean up
    cleanup(name);
  });

  it('Create new partial view macro without macro', () => {
    const name = "TestPartialMacrolessMacro";

    cleanup(name);

    openPartialViewMacroCreatePanel();

    cy.get('.menu-label').eq(1).click();

    // Type name
    cy.umbracoEditorHeaderName(name);

    // Save
    cy.get('.btn-success').click();

    // Assert
    cy.umbracoSuccessNotification().should('be.visible');
    cy.umbracoMacroExists(name).then(exists => { expect(exists).to.be.false; });

    // Clean
    cleanup(name);
  });

  it('Create new partial view macro from snippet', () => {
    const name = "TestPartialFromSnippet";

    cleanup(name);

    openPartialViewMacroCreatePanel();

    cy.get('.menu-label').eq(2).click();

    // Select snippet
    cy.get('.menu-label').eq(1).click();

    // Type name
    cy.umbracoEditorHeaderName(name);

    // Save
    cy.get('.btn-success').click();

    // Assert
    cy.umbracoSuccessNotification().should('be.visible');
    cy.umbracoMacroExists(name).then(exists => { expect(exists).to.be.true; });

    // Clean
    cleanup(name);
  });

  it('Delete partial view macro', () => {
    const name = "TestDeletePartialViewMacro";
    const fullName = name + ".cshtml"

    cleanup(name);

    const partialViewMacro = new PartialViewMacroBuilder()
      .withName(name)
      .withContent("@inherits Umbraco.Web.Macros.PartialViewMacroPage")
      .build();

    cy.savePartialViewMacro(partialViewMacro);

    // Navigate to settings
    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");

    // Delete partialViewMacro
    cy.umbracoTreeItem("settings", ["Partial View Macro Files", fullName]).rightclick();
    cy.umbracoContextMenuAction("action-delete").click();
    cy.umbracoButtonByLabelKey("general_ok").click();

    // Assert
    cy.contains(fullName).should('not.exist');

    // Clean
    cleanup(name);
  });

  it('Edit partial view macro', () => {
    const name = "TestPartialViewMacroEditable";
    const fullName = name + ".cshtml";

    cleanup(name);

    const partialViewMacro = new PartialViewMacroBuilder()
      .withName(name)
      .withContent("@inherits Umbraco.Web.Macros.PartialViewMacroPage")
      .build();

    cy.savePartialViewMacro(partialViewMacro);

    // Navigate to settings
    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");
    cy.umbracoTreeItem("settings", ["Partial View Macro Files", fullName]).click();

    // Type an edit
    cy.get('.ace_text-input').type(" // test", {force:true} );
    // Save
    cy.get('.btn-success').click();

    // Assert
    cy.umbracoSuccessNotification().should('be.visible');

    cleanup(name);
  });

});
