/// <reference types="Cypress" />
import {
    AliasHelper,
    ApprovedColorPickerDataTypeBuilder
} from 'umbraco-cypress-testhelpers';



context('DataTypes', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'), false);
    });

    // lower level commands missing from testhelpers
    function backOfficeRequest(url, method, body) {
        return cy.getCookie('UMB-XSRF-TOKEN', { log: false }).then((token) => {
          cy.request({
            method: method ?? 'GET',
            url: url,
            body: body,
            timeout: 90000,
            json: true,
            headers: {
              Accept: 'application/json',
              'X-UMB-XSRF-TOKEN': token.value,
            },
          });
        });
    }

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
        cy.get('.btn-000000').click('bottom');
        //Save
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        cy.umbracoSuccessNotification().should('be.visible');
        //Editing template with some content
        cy.editTemplate(name, '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ApprovedColourTest>' +
            '\n@{' +
            '\n    Layout = null;' +
            '\n}' +
            '\n<p style="color:@Model.UmbracoTest">Lorem ipsum dolor sit amet</p>');

        //Assert
        const expected = `<p style="color:000000"> Lorem ipsum dolor sit amet </p>`;
        backOfficeRequest('/').then(response => {
            expect(expected.replace(/\s/g, '')).to.equal(response.body.replace(/\s/g, ''))
        });

        //Pick another colour to verify both work
        cy.get('.btn-FF0000').click('bottom');
        //Save
        cy.umbracoButtonByLabelKey('buttons_saveAndPublish').click();
        cy.umbracoSuccessNotification().should('be.visible');
        //Assert
        const expected2 = '<p style="color:FF0000">Lorem ipsum dolor sit amet</p>';
        backOfficeRequest('/').then(response => {
            expect(expected2.replace(/\s/g, '')).to.equal(response.body.replace(/\s/g, ''))
        });
      

        //Clean
        cy.umbracoEnsureDataTypeNameNotExists(name);
        cy.umbracoEnsureDocumentTypeNameNotExists(name);
        cy.umbracoEnsureTemplateNameNotExists(name);
    });

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
