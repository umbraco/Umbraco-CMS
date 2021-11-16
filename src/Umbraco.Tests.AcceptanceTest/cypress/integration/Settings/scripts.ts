/// <reference types="Cypress" />
import { ScriptBuilder } from "umbraco-cypress-testhelpers";

context('Scripts', () => {

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
  });

  function navigateToSettings() {
    cy.umbracoSection('settings');
    cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");
  }

  it('Create new JavaScript file', () => {
    const name = "TestScript";
    const fileName = name + ".js";

    cy.umbracoEnsureScriptNameNotExists(fileName);

    navigateToSettings()

    cy.umbracoTreeItem("settings", ["Scripts"]).rightclick();

    cy.umbracoContextMenuAction("action-create").click();
    cy.get('.menu-label').first().click(); // TODO: Fucked we cant use something like cy.umbracoContextMenuAction("action-mediaType").click();
    //We have to wait here till everything is loaded, or worker will throw error
    cy.intercept('/umbraco/lib/ace-builds/src-min-noconflict/worker-javascript.js').as('aceWorker');
    cy.wait('@aceWorker');

    //Type name
    cy.umbracoEditorHeaderName(name);

    //Save
    cy.get('.btn-success').click();

    //Assert
    cy.umbracoSuccessNotification().should('be.visible');


    cy.umbracoScriptExists(fileName).should('be.true');


    //Clean up
    cy.umbracoEnsureScriptNameNotExists(fileName);
  });

  it('Delete a JavaScript file', () => {
    const name = "TestDeleteScriptFile";
    const fileName = name + ".js";

    cy.umbracoEnsureScriptNameNotExists(fileName);

    const script = new ScriptBuilder()
      .withName(name)
      .withContent('alert("this is content");')
      .build();

    cy.saveScript(script);

    navigateToSettings()

    cy.umbracoTreeItem("settings", ["Scripts", fileName]).rightclick();
    cy.umbracoContextMenuAction("action-delete").click();
    cy.umbracoButtonByLabelKey("general_ok").click();

    cy.contains(fileName).should('not.exist');
    cy.umbracoScriptExists(name).should('be.false');

    cy.umbracoEnsureScriptNameNotExists(fileName);
  });

  it('Update JavaScript file', () => {
    const name = "TestEditJavaScriptFile";
    const nameEdit = "Edited";
    let fileName = name + ".js";

    const originalContent = 'console.log("A script);\n';
    const edit = 'alert("content");';
    const expected = originalContent + edit;

    cy.umbracoEnsureScriptNameNotExists(fileName);

    const script = new ScriptBuilder()
      .withName(name)
      .withContent(originalContent)
      .build();
    cy.saveScript(script);

    navigateToSettings();
    cy.umbracoTreeItem("settings", ["Scripts", fileName]).click();

    cy.get('.ace_text-input').type(edit, { force: true });

    // Since scripts has no alias it should be safe to not use umbracoEditorHeaderName
    // umbracoEditorHeaderName does not like {backspace}
    cy.get('#headerName').type("{backspace}{backspace}{backspace}" + nameEdit).should('have.value', name+nameEdit);
    fileName = name + nameEdit + ".js";
    cy.get('.btn-success').click();

    cy.umbracoSuccessNotification().should('be.visible');
    cy.umbracoVerifyScriptContent(fileName, expected).should('be.true');

    cy.umbracoEnsureScriptNameNotExists(fileName);
  });

  it('Can Delete folder', () => {
    const folderName = "TestFolder";

    // The way scripts and folders are fetched and deleted are identical
    cy.umbracoEnsureScriptNameNotExists(folderName);
    cy.saveFolder('scripts', folderName);

    navigateToSettings()

    cy.umbracoTreeItem("settings", ["Scripts", folderName]).rightclick();
    cy.umbracoContextMenuAction("action-delete").click();
    cy.umbracoButtonByLabelKey("general_ok").click();

    cy.contains(folderName).should('not.exist');
    cy.umbracoScriptExists(folderName).should('be.false')

    // A script an a folder is the same thing in this case
    cy.umbracoEnsureScriptNameNotExists(folderName);
  });
});
