/// <reference types="Cypress" />	
context('Login', () => {	

    beforeEach(() => {	
      cy.visit('/umbraco');	
    });	
  
    it('Login with correct username and password', () => {	
      const username = Cypress.env('username');	
      const password = Cypress.env('password');	
      //Precondition	
      cy.get('.text-error').should('not.exist');	
  
      //Action	
      cy.get('#umb-username').type(username);	
      cy.get('#umb-passwordTwo').type(password);	
      cy.get('[label-key="general_login"]').click();	
  
      //Assert	
      cy.url().should('include', '/umbraco#/content')	
      cy.get('#umb-username').should('not.exist');	
      cy.get('#umb-passwordTwo').should('not.exist');	
    });	
  
  
    it('Login with correct username but wrong password', () => {	
      const username = Cypress.env('username');	
      const password = 'wrong';	
  
      //Precondition	
      cy.get('.text-error').should('not.exist');	
  
      //Action	
      cy.get('#umb-username').type(username);	
      cy.get('#umb-passwordTwo').type(password);	
      cy.get('[label-key="general_login"]').click();	
  
      //Assert	
      cy.get('.text-error').should('exist');	
      cy.get('#umb-username').should('exist');	
      cy.get('#umb-passwordTwo').should('exist');	
    });	
  
    it('Login with wrong username and wrong password', () => {	
      const username = 'wrong-username';	
      const password = 'wrong';	
  
      //Precondition	
      cy.get('.text-error').should('not.exist');	
  
      //Action	
      cy.get('#umb-username').type(username);	
      cy.get('#umb-passwordTwo').type(password);	
      cy.get('[label-key="general_login"]').click();	
  
      //Assert	
      cy.get('.text-error').should('exist');	
      cy.get('#umb-username').should('exist');	
      cy.get('#umb-passwordTwo').should('exist');	
    });	
  
  });