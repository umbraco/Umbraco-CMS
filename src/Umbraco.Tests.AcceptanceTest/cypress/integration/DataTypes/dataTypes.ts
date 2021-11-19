/// <reference types="Cypress" />
import {
    AliasHelper,
    ApprovedColorPickerDataTypeBuilder,
    TextBoxDataTypeBuilder,
} from 'umbraco-cypress-testhelpers';

context('DataTypes', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'), false);
      });

    it('Tests Approved Colors', () => {
        cy.deleteAllContent();
        const name = 'Approved Colour Test';
        const alias = AliasHelper.toAlias(name);

        cy.umbracoEnsureDocumentTypeNameNotExists(name);
        cy.umbracoEnsureDataTypeNameNotExists(name);

        const pickerDataType = new ApprovedColorPickerDataTypeBuilder()
            .withName(name)
            .withPrevalues(['000000', 'FF0000'])
            .build()

        //umbracoMakeDocTypeWithDataTypeAndContent(name, alias, pickerDataType);
        cy.umbracoCreateDocTypeWithContent(name, alias, pickerDataType);

        // Act
        // Enter content
        cy.umbracoRefreshContentTree();
        cy.umbracoTreeItem("content", [name]).click();
        //Pick a colour
        cy.get('.btn-000000').click();
        //Save 
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        cy.umbracoSuccessNotification().should('be.visible');
        //Editing template with some content
        cy.editTemplate(name, '@inherits Umbraco.Web.Mvc.UmbracoViewPage<ContentModels.ApprovedColourTest>' +
            '\n@using ContentModels = Umbraco.Web.PublishedModels;' +
            '\n@{' +
            '\n    Layout = null;' +
            '\n}' +
            '\n<p style="color:@Model.UmbracoTest">Lorem ipsum dolor sit amet</p>');

        //Assert
        const expected = `<p style="color:000000" > Lorem ipsum dolor sit amet </p>`;
        cy.umbracoVerifyRenderedViewContent('/', expected, true).should('be.true');

        //Pick another colour to verify both work
        cy.get('.btn-FF0000').click();
        //Save 
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        cy.umbracoSuccessNotification().should('be.visible');
        //Assert
        const expected2 = '<p style="color:FF0000">Lorem ipsum dolor sit amet</p>';
        cy.umbracoVerifyRenderedViewContent('/', expected2, true).should('be.true');

        //Clean
        cy.umbracoEnsureDataTypeNameNotExists(name);
        cy.umbracoEnsureDocumentTypeNameNotExists(name);
        cy.umbracoEnsureTemplateNameNotExists(name);
    });

    it('Tests Textbox Maxlength', () => {
        cy.deleteAllContent();
        const name = 'Textbox Maxlength Test';
        const alias = AliasHelper.toAlias(name);

        cy.umbracoEnsureDocumentTypeNameNotExists(name);
        cy.umbracoEnsureDataTypeNameNotExists(name);

        const textBoxDataType = new TextBoxDataTypeBuilder()
            .withName(name)
            .withMaxChars(10)
            .build()

        cy.umbracoCreateDocTypeWithContent(name, alias, textBoxDataType);

        // Act
        // Enter content
        // Assert no helptext with (max-2) chars & can save
        cy.umbracoRefreshContentTree();
        cy.umbracoTreeItem("content", [name]).click();
        cy.get('input[name="textbox"]').type('12345678');
        cy.get('localize[key="textbox_characters_left"]').should('not.exist');
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        cy.umbracoSuccessNotification().should('be.visible');
        cy.get('.property-error').should('not.be.visible');

        // Add char and assert helptext appears - no publish to save time & has been asserted above & below
        cy.get('input[name="textbox"]').type('9');
        cy.get('localize[key="textbox_characters_left"]').contains('characters left').should('exist');
        cy.get('.property-error').should('not.be.visible');

        // Add char and assert errortext appears and can't save
        cy.get('input[name="textbox"]').type('10'); // 1 char over max
        cy.get('localize[key="textbox_characters_exceed"]').contains('too many').should('exist');
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        cy.get('.property-error').should('be.visible');

        // Clean
        cy.umbracoEnsureDataTypeNameNotExists(name);
        cy.umbracoEnsureDocumentTypeNameNotExists(name);
    })

    //   it('Tests Checkbox List', () => {
    //     const name = 'CheckBox List';
    //     const alias = AliasHelper.toAlias(name); 

    //     cy.umbracoEnsureDocumentTypeNameNotExists(name);
    //     cy.umbracoEnsureDataTypeNameNotExists(name);

    //     const pickerDataType = new CheckBoxListDataTypeBuilder()
    //     .withName(name)
    //     .withPrevalues(['Choice 1', 'Choice 2'])
    //     .build()

    //     cy.umbracoCreateDocTypeWithContent(name, alias, pickerDataType);
    //     // Act
    //     // Enter content
    //     cy.umbracoRefreshContentTree();
    //     cy.umbracoTreeItem("content", [name]).click();
    //     //Check box 1
    //     cy.get(':nth-child(1) > umb-checkbox.ng-isolate-scope > .checkbox > .umb-form-check__symbol > .umb-form-check__state > .umb-form-check__check')
    //     .click();
    //     //Save
    //     cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
    //     cy.umbracoSuccessNotification().should('be.visible');
        
    //     //Edit template with content
    //     cy.editTemplate(name, '@inherits Umbraco.Web.Mvc.UmbracoViewPage<ContentModels.CheckboxList>' +
    //     '\n@using ContentModels = Umbraco.Web.PublishedModels;' +
    //     '\n@{' +
    //     '\n    Layout = null;' +
    //     '\n}' +
    //     '\n<p>@Model.UmbracoTest</p>');
    // });
});