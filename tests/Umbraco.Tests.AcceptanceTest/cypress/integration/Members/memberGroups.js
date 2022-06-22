context('Member Groups', () => {

    beforeEach(() => {
        cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
    });

    it('Create member group', () => {
        const name = "Test Group";

        cy.umbracoEnsureMemberGroupNameNotExists(name);

        cy.umbracoSection('member');
        cy.get('li .umb-tree-root:contains("Members")').should("be.visible");

        cy.umbracoTreeItem("member", ["Member Groups"]).rightclick();

        cy.umbracoContextMenuAction("action-create").click();

        //Type name
        cy.umbracoEditorHeaderName(name);

        // Save
        cy.get('.btn-success').click();

        //Assert
        cy.umbracoSuccessNotification().should('be.visible');

        //Clean up
        cy.umbracoEnsureMemberGroupNameNotExists(name);
    });

});
