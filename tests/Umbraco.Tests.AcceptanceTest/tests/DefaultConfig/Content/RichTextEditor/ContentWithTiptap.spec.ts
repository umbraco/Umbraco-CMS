import {
  ConstantHelper,
  NotificationConstantHelper,
  test,
} from "@umbraco/playwright-testhelpers";
import { expect } from "@playwright/test";

const contentName = "TestContent";
const documentTypeName = "TestDocumentTypeForContent";
const customDataTypeName = "Test RTE Tiptap";
let customDataTypeId = null;

test.beforeEach(async ({ umbracoApi }) => {
  customDataTypeId = await umbracoApi.dataType.createDefaultTiptapDataType(
    customDataTypeName
  );
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
});

test.afterEach(async ({ umbracoApi }) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test("can create content with empty RTE Tiptap property editor", async ({
  umbracoApi,
  umbracoUi,
}) => {
  // Arrange
  const expectedState = "Draft";
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
    documentTypeName,
    customDataTypeName,
    customDataTypeId
  );
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values).toEqual([]);
});

test("can create content with non-empty RTE Tiptap property editor", async ({
  umbracoApi,
  umbracoUi,
}) => {
  // Arrange
  const expectedState = "Draft";
  const inputText = "Test Tiptap here";
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
    documentTypeName,
    customDataTypeName,
    customDataTypeId
  );
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.enterRTETipTapEditor(inputText);
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].value.markup).toEqual(
    "<p>" + inputText + "</p>"
  );
});

test("can publish content with RTE Tiptap property editor", async ({
  umbracoApi,
  umbracoUi,
}) => {
  // Arrange
  const expectedState = "Published";
  const inputText = "Test Tiptap here";
  const documentTypeId =
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
      documentTypeName,
      customDataTypeName,
      customDataTypeId
    );
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterRTETipTapEditor(inputText);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].value.markup).toEqual(
    "<p>" + inputText + "</p>"
  );
});

test.fixme(
  "can add a media in RTE Tiptap property editor",
  async ({ umbracoApi, umbracoUi }) => {
    // Arrange
    const iconTitle = "Media Picker";
    const imageName = "Test Image For Content";
    await umbracoApi.media.ensureNameNotExists(imageName);
    await umbracoApi.media.createDefaultMediaWithImage(imageName);
    const documentTypeId =
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
        documentTypeName,
        customDataTypeName,
        customDataTypeId
      );
    await umbracoApi.document.createDefaultDocument(
      contentName,
      documentTypeId
    );
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickTipTapToolbarIconWithTitle(iconTitle);
    // fix this
    await umbracoUi.content.selectMediaWithName(imageName);
    await umbracoUi.content.clickChooseModalButton();
    await umbracoUi.content.clickMediaCaptionAltTextModalSubmitButton();
    await umbracoUi.content.clickSaveButton();

    // Assert
    //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
    await umbracoUi.content.isErrorNotificationVisible(false);
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value.markup).toContain("<img");
    expect(contentData.values[0].value.markup).toContain(imageName);

    // Clean
    await umbracoApi.media.ensureNameNotExists(imageName);
  }
);

test("can add a video in RTE Tiptap property editor", async ({
  umbracoApi,
  umbracoUi,
}) => {
  // Arrange
  const iconTitle = "Embed";
  const videoURL = "https://www.youtube.com/watch?v=Yu29dE-0OoI";
  const documentTypeId =
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
      documentTypeName,
      customDataTypeName,
      customDataTypeId
    );
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTipTapToolbarIconWithTitle(iconTitle);
  await umbracoUi.content.enterEmbeddedURL(videoURL);
  await umbracoUi.content.clickEmbeddedRetrieveButton();
  await umbracoUi.content.waitForEmbeddedPreviewVisible();
  await umbracoUi.content.clickEmbeddedMediaModalConfirmButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.markup).toContain("data-embed-url");
  expect(contentData.values[0].value.markup).toContain(videoURL);
});

test("cannot submit an empty link in RTE Tiptap property editor", async ({
  umbracoApi,
  umbracoUi,
}) => {
  // Arrange
  const iconTitle = "Link";
  const documentTypeId =
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
      documentTypeName,
      customDataTypeName,
      customDataTypeId
    );
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTipTapToolbarIconWithTitle(iconTitle);
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink("");
  await umbracoUi.content.enterAnchorOrQuerystring("");
  await umbracoUi.content.enterLinkTitle("");
  await umbracoUi.content.clickAddButton();

  // Assert
  await umbracoUi.content.isTextWithMessageVisible(
    ConstantHelper.validationMessages.emptyLinkPicker
  );
});

// TODO: Remove skip when the front-end ready. Currently it still accept the empty link with an anchor or querystring
// Issue link: https://github.com/umbraco/Umbraco-CMS/issues/17411
test.skip("cannot submit an empty URL with an anchor or querystring in RTE Tiptap property editor", async ({
  umbracoApi,
  umbracoUi,
}) => {
  // Arrange
  const iconTitle = "Link";
  const documentTypeId =
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
      documentTypeName,
      customDataTypeName,
      customDataTypeId
    );
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTipTapToolbarIconWithTitle(iconTitle);
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink("");
  await umbracoUi.content.enterAnchorOrQuerystring("#value");
  await umbracoUi.content.clickAddButton();

  // Assert
  await umbracoUi.content.isTextWithMessageVisible(
    ConstantHelper.validationMessages.emptyLinkPicker
  );
});

// TODO: Remove skip when the front-end ready. Currently it is impossible to link to unpublished document
// Issue link: https://github.com/umbraco/Umbraco-CMS/issues/17974
test.skip("can insert a link to an unpublished document in RTE Tiptap property editor", async ({
  umbracoApi,
  umbracoUi,
}) => {
  // Arrange
  const iconTitle = "Link";
  const documentTypeId =
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(
      documentTypeName,
      customDataTypeName,
      customDataTypeId
    );
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  // Create a document to link
  const documentTypeForLinkedDocumentName = "TestDocumentType";
  const documentTypeForLinkedDocumentId =
    await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(
      documentTypeForLinkedDocumentName
    );
  const linkedDocumentName = "LinkedDocument";
  await umbracoApi.document.createDefaultDocument(
    linkedDocumentName,
    documentTypeForLinkedDocumentId
  );
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTipTapToolbarIconWithTitle(iconTitle);
  await umbracoUi.content.clickDocumentLinkButton();
  await umbracoUi.content.selectLinkByName(linkedDocumentName);
  await umbracoUi.content.clickButtonWithName("Choose");
  await umbracoUi.content.clickAddButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(
    documentTypeForLinkedDocumentName
  );
  await umbracoApi.document.ensureNameNotExists(linkedDocumentName);
});
