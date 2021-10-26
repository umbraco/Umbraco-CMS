/// <reference types="Cypress" />
import {
    DocumentTypeBuilder,
    AliasHelper
  } from 'umbraco-cypress-testhelpers';

  const tabsDocTypeName = 'Tabs Test Document';
  const tabsDocTypeAlias = AliasHelper.toAlias(tabsDocTypeName);

  context('Tabs', () => {

      beforeEach(() => {
          cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'), false);
      });

      afterEach(() =>  {
          cy.umbracoEnsureDocumentTypeNameNotExists(tabsDocTypeName)
      });

      function OpenDocTypeFolder(){
          cy.umbracoSection('settings');
          cy.get('li .umb-tree-root:contains("Settings")').should("be.visible");
          cy.get('.umb-tree-item__inner > .umb-tree-item__arrow').eq(0).click();
          cy.get('.umb-tree-item__inner > .umb-tree-item__label').contains(tabsDocTypeName).click();
      }

      function CreateDocWithTabAndNavigate(){
          cy.umbracoEnsureDocumentTypeNameNotExists(tabsDocTypeName);
          const tabsDocType = new DocumentTypeBuilder()
              .withName(tabsDocTypeName)
              .withAlias(tabsDocTypeAlias)
              .withAllowAsRoot(true)
              .withDefaultTemplate(tabsDocTypeAlias)
              .addTab()
                  .withName('Tab 1')
                  .addGroup()
                    .withName('Tab group')
                    .addUrlPickerProperty()
                      .withAlias("urlPicker")
                    .done()
                  .done()
              .done()
            .build();
          cy.saveDocumentType(tabsDocType);
          OpenDocTypeFolder();
      }

      it('Create tab', () => {
        cy.umbracoEnsureDocumentTypeNameNotExists(tabsDocTypeName);
        cy.deleteAllContent();
        const tabsDocType = new DocumentTypeBuilder()
            .withName(tabsDocTypeName)
            .withAlias(tabsDocTypeAlias)
            .withAllowAsRoot(true)
            .withDefaultTemplate(tabsDocTypeAlias)
            .addGroup()
                .withName('Tabs1Group')
                .addUrlPickerProperty()
                    .withAlias('picker')
                .done()
            .done()
            .build();
        cy.saveDocumentType(tabsDocType);
        OpenDocTypeFolder();
        //Create a tab
        cy.get('.umb-group-builder__tabs__add-tab').click();
        cy.get('ng-form.ng-invalid > .umb-group-builder__group-title-input').type('Tab 1');
        //Create a 2nd tab manually
        cy.get('.umb-group-builder__tabs__add-tab').click();
        cy.get('ng-form.ng-invalid > .umb-group-builder__group-title-input').type('Tab 2');
        //Create a textstring property
        cy.get('[aria-hidden="false"] > .umb-box-content > .umb-group-builder__group-add-property').click();
        cy.get('.editor-label').type('property name');
        cy.get('[data-element="editor-add"]').click();

        //Search for textstring
        cy.get('#datatype-search').type('Textstring');

        // Choose first item
        cy.get('[title="Textstring"]').closest("li").click();

        // Save property
        cy.get('.btn-success').last().click();
        cy.umbracoButtonByLabelKey('buttons_save').click();
        //Assert
        cy.umbracoSuccessNotification().should('be.visible');
        cy.get('[title="tab1"]').should('be.visible');
        cy.get('[title="tab2"]').should('be.visible');
      });

      it('Delete tabs', () => {
        CreateDocWithTabAndNavigate();
        //Check if tab is there, else if it wasnt created, this test would always pass
        cy.get('[title="aTab 1"]').should('be.visible');
        //Delete a tab
        cy.get('.btn-reset > [icon="icon-trash"]').first().click();
        cy.get('.umb-button > .btn').last().click();
        cy.umbracoButtonByLabelKey('buttons_save').click();
        //Assert
        cy.get('[title="aTab 1"]').should('not.exist');
        //Clean
        cy.umbracoEnsureDocumentTypeNameNotExists(tabsDocTypeName);
      });

      it('Delete property in tab', () => {
        cy.umbracoEnsureDocumentTypeNameNotExists(tabsDocTypeName);
        const tabsDocType = new DocumentTypeBuilder()
            .withName(tabsDocTypeName)
            .withAlias(tabsDocTypeAlias)
            .withAllowAsRoot(true)
            .withDefaultTemplate(tabsDocTypeAlias)
            .addTab()
                .withName('Tab 1')
                .addGroup()
                  .withName('Tab group')
                  .addUrlPickerProperty()
                    .withAlias("urlPicker")
                  .done()
                  .addContentPickerProperty()
                    .withAlias('picker')
                  .done()
                .done()
            .done()
            .build();
        cy.saveDocumentType(tabsDocType);
        OpenDocTypeFolder();
        cy.get('[aria-label="Delete property"]').last().click();
        cy.umbracoButtonByLabelKey('actions_delete').click();
        cy.umbracoButtonByLabelKey('buttons_save').click()
        //Assert
        cy.umbracoSuccessNotification().should('be.visible');
        cy.get('[title=urlPicker]').should('be.visible');
        cy.get('[title=picker]').should('not.exist');
      });

      it('Delete group in tab', () => {
        cy.umbracoEnsureDocumentTypeNameNotExists(tabsDocTypeName);
        const tabsDocType = new DocumentTypeBuilder()
            .withName(tabsDocTypeName)
            .withAlias(tabsDocTypeAlias)
            .withAllowAsRoot(true)
            .withDefaultTemplate(tabsDocTypeAlias)
            .addTab()
                .withName('Tab 1')
                .addGroup()
                  .withName('Tab group')
                  .addUrlPickerProperty()
                    .withAlias("urlPicker")
                  .done()
                .done()
                .addGroup()
                  .withName('Content Picker Group')
                  .addContentPickerProperty()
                    .withAlias('picker')
                  .done()
                .done()
            .done()
            .build();
        cy.saveDocumentType(tabsDocType);
        OpenDocTypeFolder();
        //Delete group
        cy.get('.umb-group-builder__group-remove > [icon="icon-trash"]').eq(1).click();
        cy.umbracoButtonByLabelKey('actions_delete').click();
        cy.umbracoButtonByLabelKey('buttons_save').click()
        //Assert
        cy.umbracoSuccessNotification().should('be.visible');
        cy.get('[title=picker]').should('be.visible');
        cy.get('[title=urlPicker]').should('not.exist');
      });

      it('Reorders tab', () => {
        cy.umbracoEnsureDocumentTypeNameNotExists(tabsDocTypeName);
        const tabsDocType = new DocumentTypeBuilder()
            .withName(tabsDocTypeName)
            .withAlias(tabsDocTypeAlias)
            .withAllowAsRoot(true)
            .withDefaultTemplate(tabsDocTypeAlias)
            .addTab()
                .withName('Tab 1')
                .addGroup()
                  .withName('Tab group 1')
                  .addUrlPickerProperty()
                    .withLabel('Url picker 1')
                    .withAlias("urlPicker")
                  .done()
                .done()
            .done()
            .addTab()
                .withName('Tab 2')
                .addGroup()
                  .withName('Tab group 2')
                  .addUrlPickerProperty()
                    .withLabel('Url picker 2')
                    .withAlias("pickerTab 2")
                  .done()
                .done()
            .done()
            .addTab()
                .withName('Tab 3')
                .addGroup()
                  .withName('Tab group')
                  .addUrlPickerProperty()
                    .withLabel('Url picker 3')
                    .withAlias('pickerTab3')
                  .done()
                .done()
            .done()
            .build();
        cy.saveDocumentType(tabsDocType);
        OpenDocTypeFolder();
        //Check if there are any tabs
        cy.get('ng-form.ng-valid-required > .umb-group-builder__group-title-input').should('be.visible');
        cy.get('[alias="reorder"]').click();
        //Type order in
        cy.get('.umb-group-builder__tab-sort-order > .umb-property-editor-tiny').first().type('3');
        cy.get('[alias="reorder"]').click();
        //Assert
        cy.get('.umb-group-builder__group-title-input').eq(0).invoke('attr', 'title').should('eq', 'aTab 2')
        cy.get('.umb-group-builder__group-title-input').eq(1).invoke('attr', 'title').should('eq', 'aTab 3')
        cy.get('.umb-group-builder__group-title-input').eq(2).invoke('attr', 'title').should('eq', 'aTab 1')
      });

      it('Reorders groups in a tab', () => {
        cy.umbracoEnsureDocumentTypeNameNotExists(tabsDocTypeName);
        const tabsDocType = new DocumentTypeBuilder()
            .withName(tabsDocTypeName)
            .withAlias(tabsDocTypeAlias)
            .withAllowAsRoot(true)
            .withDefaultTemplate(tabsDocTypeAlias)
            .addTab()
                .withName('Tab 1')
                .addGroup()
                  .withName('Tab group 1')
                  .addUrlPickerProperty()
                    .withLabel('Url picker 1')
                    .withAlias("urlPicker")
                  .done()
                .done()
                .addGroup()
                  .withName('Tab group 2')
                  .addUrlPickerProperty()
                    .withLabel('Url picker 2')
                    .withAlias('urlPickerTwo')
                  .done()
                .done()
            .done()
            .build();
        cy.saveDocumentType(tabsDocType);
        OpenDocTypeFolder();
        cy.get('[alias="reorder"]').click();
        cy.get('.umb-property-editor-tiny').eq(2).type('1');

        cy.get('[alias="reorder"]').click();
        cy.umbracoButtonByLabelKey('buttons_save').click();
        //Assert
        cy.umbracoSuccessNotification().should('be.visible');
        cy.get('.umb-group-builder__group-title-input').eq(2)
        .invoke('attr', 'title').should('eq', 'aTab 1/aTab group 2');
      });

      it('Reorders properties in a tab', () => {
       cy.umbracoEnsureDocumentTypeNameNotExists(tabsDocTypeName);
       const tabsDocType = new DocumentTypeBuilder()
           .withName(tabsDocTypeName)
           .withAlias(tabsDocTypeAlias)
           .withAllowAsRoot(true)
           .withDefaultTemplate(tabsDocTypeAlias)
           .addTab()
               .withName('Tab 1')
               .addGroup()
                 .withName('Tab group')
                 .addUrlPickerProperty()
                   .withLabel('PickerOne')
                   .withAlias("urlPicker")
                 .done()
                 .addUrlPickerProperty()
                   .withLabel('PickerTwo')
                   .withAlias('urlPickerTwo')
                 .done()
               .done()
           .done()
         .build();
       cy.saveDocumentType(tabsDocType);
       OpenDocTypeFolder();
       //Reorder
       cy.get('[alias="reorder"]').click();
       cy.get('.umb-group-builder__group-sort-value').first().type('2');
       cy.get('[alias="reorder"]').click();
       cy.umbracoButtonByLabelKey('buttons_save').click();
       //Assert
       cy.umbracoSuccessNotification().should('be.visible');
       cy.get('.umb-locked-field__input').last().invoke('attr', 'title').should('eq', 'urlPicker');
      });

      it('Tab name cannot be empty', () => {
        CreateDocWithTabAndNavigate();
        cy.get('.umb-group-builder__group-title-input').first().clear();
        cy.umbracoButtonByLabelKey('buttons_save').click();
        //Assert
        cy.umbracoErrorNotification().should('be.visible');
      });

      it('Two tabs cannot have the same name', () => {
        cy.umbracoEnsureDocumentTypeNameNotExists(tabsDocTypeName);
        const tabsDocType = new DocumentTypeBuilder()
            .withName(tabsDocTypeName)
            .withAlias(tabsDocTypeAlias)
            .withAllowAsRoot(true)
            .withDefaultTemplate(tabsDocTypeAlias)
            .addTab()
                .withName('Tab 1')
                .addGroup()
                  .withName('Tab group')
                  .addUrlPickerProperty()
                    .withAlias("urlPicker")
                  .done()
                .done()
            .done()
          .build();
        cy.saveDocumentType(tabsDocType);
        OpenDocTypeFolder();
        //Create a 2nd tab manually
        cy.get('.umb-group-builder__tabs__add-tab').click();
        cy.get('ng-form.ng-invalid > .umb-group-builder__group-title-input').type('Tab 1');
        cy.umbracoButtonByLabelKey('buttons_save').click();
        //Assert
        cy.umbracoErrorNotification().should('be.visible');
      });

      it('Group name cannot be empty', () => {
        CreateDocWithTabAndNavigate();
        cy.get('.clearfix > .-placeholder').click();
        cy.umbracoButtonByLabelKey('buttons_save').click();
        //Assert
        cy.umbracoErrorNotification().should('be.visible');
      });

      it('Group name cannot have the same name', () => {
        CreateDocWithTabAndNavigate();
        cy.get('.clearfix > .-placeholder').click();
        cy.get('.umb-group-builder__group-title-input').last().type('Tab group');
        cy.umbracoButtonByLabelKey('buttons_save').click();
        //Assert
        cy.umbracoErrorNotification().should('be.visible');
      });

      it('Drag a group into another tab', () => {
        cy.umbracoEnsureDocumentTypeNameNotExists(tabsDocTypeName);
        const tabsDocType = new DocumentTypeBuilder()
            .withName(tabsDocTypeName)
            .withAlias(tabsDocTypeAlias)
            .withAllowAsRoot(true)
            .withDefaultTemplate(tabsDocTypeAlias)
            .addTab()
                .withName('Tab 1')
                .addGroup()
                  .withName('Tab group')
                  .addUrlPickerProperty()
                    .withAlias("urlPicker")
                  .done()
                .done()
            .done()
            .addTab()
              .withName('Tab 2')
              .addGroup()
                .withName('Tab group tab 2')
                .addUrlPickerProperty()
                  .withAlias('urlPickerTabTwo')
                .done()
              .done()
              .addGroup()
                  .withName('Tab group 2')
                  .addUrlPickerProperty()
                    .withAlias('urlPickerTwo')
                  .done()
                .done()
            .done()
          .build();
        cy.saveDocumentType(tabsDocType);
        OpenDocTypeFolder();
        cy.get('[alias="reorder"]').click();
        cy.get('body')
        .then(($body) => {
          while($body.find('.umb-group-builder__tabs-overflow--right > .caret').hasClass('active')){
            cy.click();
          }
        });
        cy.get('.umb-group-builder__tab').last().click();
        cy.get('.umb-group-builder__group-title-icon').last().trigger('mousedown', { which: 1 })
        cy.get('.umb-group-builder__tab').eq(1).trigger('mousemove', {which: 1, force: true});
        cy.get('.umb-group-builder__tab').eq(1).should('have.class', 'is-active').trigger('mouseup', {force:true});
        cy.umbracoButtonByLabelKey('buttons_save').click();
        //Assert
        cy.umbracoSuccessNotification().should('be.visible');
        cy.get('[title="aTab 1/aTab group 2"]').should('be.visible');
      });

      it('Drag and drop reorders a tab', () => {
        cy.umbracoEnsureDocumentTypeNameNotExists(tabsDocTypeName);
        const tabsDocType = new DocumentTypeBuilder()
            .withName(tabsDocTypeName)
            .withAlias(tabsDocTypeAlias)
            .withAllowAsRoot(true)
            .withDefaultTemplate(tabsDocTypeAlias)
            .addTab()
                .withName('Tab 1')
                .addGroup()
                  .withName('Tab group')
                  .addUrlPickerProperty()
                    .withAlias("urlPicker")
                  .done()
                .done()
            .done()
            .addTab()
              .withName('Tab 2')
              .addGroup()
                .withName('Tab group tab 2')
                .addUrlPickerProperty()
                  .withAlias('urlPickerTabTwo')
                .done()
              .done()
              .addGroup()
                  .withName('Tab group 2')
                  .addUrlPickerProperty()
                    .withAlias('urlPickerTwo')
                  .done()
                .done()
            .done()
          .build();
        cy.saveDocumentType(tabsDocType);
        OpenDocTypeFolder();
        cy.get('[alias="reorder"]').click();
        //Scroll right so we can see tab 2
        cy.get('body')
          .then(($body) => {
            while($body.find('.umb-group-builder__tabs-overflow--right > .caret').hasClass('active')){
              cy.click();
            }
          });
        cy.get('.umb-group-builder__tab-title-icon').eq(1).trigger('mousedown', { which: 1 })
        cy.get('.umb-group-builder__tab').eq(1).trigger('mousemove', {which: 1, force: true});
        cy.get('.umb-group-builder__tab').eq(1).should('have.class', 'is-active').trigger('mouseup', {force:true});
        cy.get('[alias="reorder"]').click();
        cy.umbracoButtonByLabelKey('buttons_save').click();
        //Assert
        cy.umbracoSuccessNotification().should('be.visible');
        cy.get('[title="aTab 2"]').should('be.visible');
      });

      it('Drags and drops a property in a tab', () => {
        cy.umbracoEnsureDocumentTypeNameNotExists(tabsDocTypeName);
        const tabsDocType = new DocumentTypeBuilder()
            .withName(tabsDocTypeName)
            .withAlias(tabsDocTypeAlias)
            .withAllowAsRoot(true)
            .withDefaultTemplate(tabsDocTypeAlias)
            .addTab()
                .withName('Tab 1')
                .addGroup()
                  .withName('Tab group')
                  .addUrlPickerProperty()
                    .withAlias("urlPicker")
                    .withLabel('UrlPickerOne')
                  .done()
                .done()
            .done()
            .addTab()
              .withName('Tab 2')
              .addGroup()
                .withName('Tab group tab 2')
                .addUrlPickerProperty()
                  .withAlias('urlPickerTabTwo')
                  .withLabel('UrlPickerTabTwo')
                .done()
                .addUrlPickerProperty()
                    .withAlias('urlPickerTwo')
                    .withLabel('UrlPickerTwo')
                  .done()
              .done()
            .done()
          .build();
        cy.saveDocumentType(tabsDocType);
        OpenDocTypeFolder();
        cy.get('[alias="reorder"]').click();
        //Scroll so we are sure we see tab 2
        cy.get('body')
        .then(($body) => {
          while($body.find('.umb-group-builder__tabs-overflow--right > .caret').hasClass('active')){
            cy.click();
          }
        });
        //Navigate to tab 2
        cy.get('.umb-group-builder__tab').last().click();
        cy.get('.umb-group-builder__property-meta > .flex > .icon').eq(1).trigger('mousedown', {which: 1})
        cy.get('.umb-group-builder__tab').eq(1).trigger('mousemove', {which: 1, force: true});
        cy.get('.umb-group-builder__tab').eq(1).should('have.class', 'is-active');
        cy.get('.umb-group-builder__property')
        .first().trigger('mousemove', {which: 1, force: true}).trigger('mouseup', {which: 1, force:true});
        cy.get('[alias="reorder"]').click();
        cy.umbracoButtonByLabelKey('buttons_save').click();
        //Assert
        cy.umbracoSuccessNotification().should('be.visible');
        cy.get('[title="urlPickerTabTwo"]').should('be.visible');
      });

      it('Drags and drops a group and converts to tab', () => {
        cy.umbracoEnsureDocumentTypeNameNotExists(tabsDocTypeName);
        const tabsDocType = new DocumentTypeBuilder()
            .withName(tabsDocTypeName)
            .withAlias(tabsDocTypeAlias)
            .withAllowAsRoot(true)
            .withDefaultTemplate(tabsDocTypeAlias)
            .addTab()
                .withName('Tab 1')
                .addGroup()
                  .withName('Tab group')
                  .addUrlPickerProperty()
                    .withAlias("urlPicker")
                    .withLabel('UrlPickerOne')
                  .done()
                .done()
                .addGroup()
                .withName('Tab group 2')
                .addUrlPickerProperty()
                    .withAlias('urlPickerTwo')
                    .withLabel('UrlPickerTwo')
                  .done()
              .done()
            .done()
            .addTab()
              .withName('Tab 2')
              .addGroup()
                .withName('Tab group tab 2')
                .addUrlPickerProperty()
                  .withAlias('urlPickerTabTwo')
                  .withLabel('UrlPickerTabTwo')
                .done()
              .done()
            .done()
          .build();
        cy.saveDocumentType(tabsDocType);
        OpenDocTypeFolder();
        cy.get('[alias="reorder"]').click();
        cy.get('.umb-group-builder__group-title-icon').eq(1).trigger('mousedown', {which: 1})
        cy.get('.umb-group-builder__convert-dropzone').trigger('mousemove', {which: 1, force: true});
        cy.get('.umb-group-builder__convert-dropzone').trigger('mouseup', {which: 1, force:true});
        cy.umbracoButtonByLabelKey('buttons_save').click();
        //Assert
        cy.umbracoSuccessNotification().should('be.visible');
        cy.get('[title="tabGroup"]').should('be.visible');
      });
  });
