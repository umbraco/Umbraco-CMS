/// <reference types="Cypress" />	

import {MediaBuilder} from 'umbraco-cypress-testhelpers';

context('Media', () => {

  beforeEach(() => {
    cy.umbracoLogin(Cypress.env('username'), Cypress.env('password'));
    cy.umbracoSection("media");
  });

  function refreshMediaTree() {
    // Refresh to update the tree
    cy.get('li .umb-tree-root:contains("Media")').should("be.visible").rightclick();
    //Needs to wait or it can give an error
    cy.wait(1000);
    cy.get(".umb-outline").contains("Reload").click();
  }

  it('Create folder', () => {
    const folderName = 'Media Folder';
    //Ensures that there is not already an existing folder with the same name
    cy.umbracoEnsureMediaNameNotExists(folderName);

    //Action
    //Creates folder
    cy.get(".dropdown-toggle").contains("Create").click({force: true});
    cy.get('[role="menuitem"]').contains("Folder").click({force: true});
    cy.get('[data-element="editor-name-field"]').type(folderName);
    cy.umbracoButtonByLabelKey("buttons_save").click();

    //Assert
    cy.umbracoSuccessNotification().should("be.visible");

    //Cleans up
    cy.umbracoEnsureMediaNameNotExists(folderName);
  });

  it('Create folder inside of folder', () => {
    const folderName = 'Folder';
    const insideFolderName = 'Folder in folder';
    //Ensures that there is not already existing folders with the same names
    cy.umbracoEnsureMediaNameNotExists(folderName);
    cy.umbracoEnsureMediaNameNotExists(insideFolderName);

    //Action
    //Creates the first folder with an API call
    const mediaFolder = new MediaBuilder()
      .withName(folderName)
      .withContentTypeAlias('Folder')
      .build()
    cy.saveMedia(mediaFolder, null);
    //Creates second folder
    refreshMediaTree();
    cy.umbracoTreeItem('media', [folderName]).click();
    cy.get(".dropdown-toggle").contains("Create").click({force: true});
    cy.get('[role="menuitem"]').contains("Folder").click({force: true});
    cy.get('[data-element="editor-name-field"]').type(insideFolderName);
    cy.umbracoButtonByLabelKey("buttons_save").click();

    //Assert
    cy.umbracoSuccessNotification().should("be.visible");

    //Cleans up
    cy.umbracoEnsureMediaNameNotExists(folderName);
    cy.umbracoEnsureMediaNameNotExists(insideFolderName);
  });

  it('Create image', () => {
    const imageName = 'ImageTest';
    //Ensures that there is not already an existing image with the name
    cy.umbracoEnsureMediaNameNotExists(imageName);
    const umbracoFileValue = {"src": "Umbraco.png,"}

    //Action
    const mediaImage = new MediaBuilder()
      .withName(imageName)
      .withContentTypeAlias('Image')
      .addProperty()
      .withAlias("umbracoFile")
      .withValue(umbracoFileValue)
      .done()
      .build()
    const blob = Cypress.Blob.base64StringToBlob("iVBORw0KGgoAAAANSUhEUgAAADcAAAAjCAYAAAAwnJLLAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAGpSURBVFhH7ZRNq0FRFIbPbzD3A/wKSUkZmCgzAyUpkhhRyMT8TIwlEylDI2WgJMyMmJAB+SqS5OvVXjY599ad3eyt/dRpnbXW7rSf1upo+GKUnKwoOVlRcrKi5GRFycmKkpMVJScrSk5WhJDr9/uIRqPYbDa8Aux2O2QyGVitVjidTrTbbd55cLvdUKlUUCgUcDqdeNXIR+XYBev1OtxuNzweD1ar1auu6zrK5TK9j8dj+P1+LJdL6jOazSZisRj2+z2v/OajcuxitVoNk8kEwWDQIMdqh8OBcjbFcDiM0WhE+Xw+RyKRoPgXQqwlk3qX+0m320UymcTxeKQnHo/D4XDA5XIhn89jvV7zk0aEl2MrydbvOaVerwefz4fZbIbr9YpqtYp0Oo3L5UL9d4SWY2KRSITik1arhWKxyDNgOp0ilUq9VvgdYeWYUCgUwnA45JUHg8EA2WwW5/OZ8kajgVwuJ+bk2F/RZrPBbDZTZPl2u4XX64XFYoHJZIKmaRQ7nQ5JlEol2O12Oh8IBLBYLPjXjAgxuf9CycmKkpMVJScrSk5WvlgOuANsVZDROrcwfgAAAABJRU5ErkJggg==");
    const testFile = new File([blob], "test.jpg");
    cy.saveMedia(mediaImage, testFile);
    refreshMediaTree();

    //Assert
    cy.get('[data-element="tree-item-ImageTest"]').should("be.visible");

    //Cleans up
    cy.umbracoEnsureMediaNameNotExists(imageName);
  });

  it('Create SVG', () => {
    const svgName = 'svgTest';
    //Ensures that there is not already an existing SVG with the name
    cy.umbracoEnsureMediaNameNotExists(svgName);

    //Action
    const mediaSVG = new MediaBuilder()
      .withName(svgName)
      .withContentTypeAlias('umbracoMediaVectorGraphics')
      .build()
    cy.saveMedia(mediaSVG, null);
    refreshMediaTree();

    //Assert
    cy.get('[data-element="tree-item-svgTest"]').should("be.visible");

    //Cleans up
    cy.umbracoEnsureMediaNameNotExists(svgName);
  });

  it('Create Audio', () => {
    const audioName = 'audioTest';
    //Ensures that there is not already an existing audio with the name
    cy.umbracoEnsureMediaNameNotExists(audioName);

    //Action
    const mediaAudio = new MediaBuilder()
      .withName(audioName)
      .withContentTypeAlias('umbracoMediaAudio')
      .build()
    cy.saveMedia(mediaAudio, null);
    refreshMediaTree();

    //Assert
    cy.get('[data-element="tree-item-audioTest"]').should("be.visible");

    //Cleans up
    cy.umbracoEnsureMediaNameNotExists(audioName);
  });

  it('Create File', () => {
    const fileName = 'fileTest';
    //Ensures that there is not already an existing file with the name
    cy.umbracoEnsureMediaNameNotExists(fileName);

    //Action
    const mediaFile = new MediaBuilder()
      .withName(fileName)
      .withContentTypeAlias('File')
      .build()
    cy.saveMedia(mediaFile, null);
    refreshMediaTree();

    //Assert
    cy.get('[data-element="tree-item-fileTest"]').should("be.visible");

    //Cleans up
    cy.umbracoEnsureMediaNameNotExists(fileName);
  });

  it('Create Video', () => {
    const videoName = 'videoTest';
    //Ensures that there is not already an existing video with the name
    cy.umbracoEnsureMediaNameNotExists(videoName);

    //Action
    const mediaVideo = new MediaBuilder()
      .withName(videoName)
      .withContentTypeAlias('umbracoMediaVideo')
      .build()
    cy.saveMedia(mediaVideo, null);
    refreshMediaTree();

    //Assert
    cy.get('[data-element="tree-item-videoTest"]').should("be.visible");

    //Cleans up
    cy.umbracoEnsureMediaNameNotExists(videoName);
  });

  it('Create Article', () => {
    const articleName = 'articleTest';
    //Ensures that there is not already an existing article with the name
    cy.umbracoEnsureMediaNameNotExists(articleName);

    //Action
    const mediaArticle = new MediaBuilder()
      .withName(articleName)
      .withContentTypeAlias('umbracoMediaArticle')
      .build()
    cy.saveMedia(mediaArticle, null);
    refreshMediaTree();

    //Assert
    cy.get('[data-element="tree-item-articleTest"]').should("be.visible");

    //Cleans up
    cy.umbracoEnsureMediaNameNotExists(articleName);
  });
}); 