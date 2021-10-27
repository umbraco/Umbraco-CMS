/// <reference types="Cypress" />
import {
    DocumentTypeBuilder,
    ContentBuilder,
    AliasHelper
} from 'umbraco-cypress-testhelpers';

context('Url Picker', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'), false);
      });

    it('Test Url Picker', () => {
       
        const urlPickerDocTypeName = 'Url Picker Test';
        const pickerDocTypeAlias = AliasHelper.toAlias(urlPickerDocTypeName); 
        cy.umbracoEnsureDocumentTypeNameNotExists(urlPickerDocTypeName);
        cy.deleteAllContent();
        const pickerDocType = new DocumentTypeBuilder()
            .withName(urlPickerDocTypeName)
            .withAlias(pickerDocTypeAlias)
            .withAllowAsRoot(true)
            .withDefaultTemplate(pickerDocTypeAlias)
            .addGroup()
                .withName('ContentPickerGroup')
                .addUrlPickerProperty()
                    .withAlias('picker')
                .done()
            .done()
            .build();

        cy.saveDocumentType(pickerDocType);
        cy.editTemplate(urlPickerDocTypeName, '@inherits Umbraco.Web.Mvc.UmbracoViewPage<ContentModels.UrlPickerTest>' +
        '\n@using ContentModels = Umbraco.Web.PublishedModels;' +
        '\n@{' +
        '\n    Layout = null;' +
        '\n}' +
        '\n@foreach(var link in @Model.Picker)' +
        '\n{' +
        '\n    <a href="@link.Url">@link.Name</a>' +
        '\n}');
        // Create content with url picker
        cy.get('li .umb-tree-root:contains("Content")').should("be.visible").rightclick();
        cy.get('[data-element="action-create"]').click();
        cy.get('[data-element="action-create-' + pickerDocTypeAlias + '"] > .umb-action-link').click();
        // Fill out content
        cy.umbracoEditorHeaderName('UrlPickerContent');
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        cy.umbracoSuccessNotification().should('be.visible');
        cy.get('.umb-node-preview-add').click();
        // Should really try and find a better way to do this, but umbracoTreeItem tries to click the content pane in the background
        cy.get('#treePicker li:first', {timeout: 6000}).click();
        cy.get('.umb-editor-footer-content__right-side > [button-style="success"] > .umb-button > .btn > .umb-button__content').click();
        cy.get('.umb-node-preview__name').should('be.visible');
        //Save and publish
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        cy.umbracoSuccessNotification().should('be.visible');
        //Waiting to ensure we have saved properly before leaving
        cy.reload();
        //Assert
        cy.get('.umb-notifications__notifications > .alert-error').should('not.exist');
        //Editing template with some content
        
        //Testing if the edits match the expected results
        const expected = '<a href="/">UrlPickerContent</a>';
        cy.umbracoVerifyRenderedViewContent('/', expected, true).should('be.true');        
        //clean
        cy.umbracoEnsureDocumentTypeNameNotExists(urlPickerDocTypeName);
        cy.umbracoEnsureTemplateNameNotExists(urlPickerDocTypeName);

    });
});