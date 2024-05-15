import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// Remove smoke tag before merging
test.describe('Content info tab tests @smoke', () => {
  let documentTypeId = '';
  let contentId = '';
  const contentName = 'TestInfoTab';
  const documentTypeName = 'TestDocumentTypeForContent';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.document.ensureNameNotExists(contentName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(contentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  });

  test.skip('can see correct link is shown when published', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const notPublishContentLink = 'This document is published but is not in the cache';
    documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
    contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

    // Act
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
    await umbracoUi.content.openContent(contentName);
    await umbracoUi.content.clickInfoTab();
    await umbracoUi.content.doesLinkHaveText(notPublishContentLink);
    await umbracoUi.content.clickSaveAndPublishButton();
    await umbracoUi.reloadPage();
    
    // Assert
    const contentData = await umbracoApi.document.get(contentId);
    // verify the content link
    await umbracoUi.content.doesLinkHaveText(contentData.urls[0].url);
    // TODO: verify history of content when the front-end is ready
    // TODO: verify publication status when the front-end is ready
    // TODO: verify created date/time when the front-end is ready
    // TODO: verify content id when the front-end is ready 
  });

  test.skip('can open document type', async ({umbracoApi, umbracoUi}) => {
    // TODO: implement this test when the front-end is ready
  });

  test.skip('can switch template', async ({umbracoApi, umbracoUi}) => {
    // TODO: implement this test when the front-end is ready
  });

  test.skip('cannot switch to a template that is not allowed in the document type', async ({umbracoApi, umbracoUi}) => {
    // TODO: implement this test when the front-end is ready
  });

  test.skip('can open template', async ({umbracoApi, umbracoUi}) => {
    // TODO: implement this test when the front-end is ready
  });
});
