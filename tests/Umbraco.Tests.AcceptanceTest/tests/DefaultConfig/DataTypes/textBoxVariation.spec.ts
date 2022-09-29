import {expect} from "@playwright/test";
import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {ContentBuilder, DocumentTypeBuilder, DomainBuilder} from "@umbraco/json-models-builders";

test.describe('Vary by culture for TextBox', () => {

    test.beforeEach(async ({page, umbracoApi, umbracoUi}) => {
        // TODO: REMOVE THIS WHEN SQLITE IS FIXED
        // Wait so we don't bombard the API
        await page.waitForTimeout(1000);
        await umbracoApi.login();
    });
    
    test('create documentType with vary by culture with UI with a textbox property which also has vary by culture', async ({page, umbracoApi, umbracoUi}) => {
        const documentTypeName = 'Test Document';
        const textBoxPropertyName = 'TestBox';
        const groupName = 'textBoxGroup';

        await umbracoApi.documentTypes.ensureNameNotExists(documentTypeName);
        await umbracoApi.templates.ensureNameNotExists(documentTypeName);

        await umbracoUi.goToSection(ConstantHelper.sections.settings);

        // Creates document with a template
        await umbracoUi.createNewDocumentTypeWithTemplate();

        // Updates permission for document
        await umbracoUi.updateDocumentPermissionsToAllowCultureVariant();

        await umbracoUi.setEditorHeaderName(documentTypeName);

        // Adds a group with a TextBox editor
        await umbracoUi.goToAddEditor(groupName,textBoxPropertyName);
        await page.locator('[data-element="datatype-Textbox"]').click();
        await page.locator('[data-element="datatypeconfig-Textstring"]').click();
        await page.locator('[data-element="button-submit"]').click();

        // Saves the document
        await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

        // Assert
        await expect(page.locator('.umb-notifications__notifications > .alert-success')).toBeVisible();

        // Clean
        await umbracoApi.documentTypes.ensureNameNotExists(documentTypeName);
        await umbracoApi.templates.ensureNameNotExists(documentTypeName);
    });

    test('create content with two languages with different text', async ({page, umbracoApi, umbracoUi}) => {
        const documentName = "Test Document";
        const languageEn = 'en-US';
        const languageDa = 'da';
        const enValue = "USA";
        const daValue = "DENMARK";
        const alias = AliasHelper.toAlias(documentName);

        await umbracoApi.content.deleteAllContent();
        await umbracoApi.documentTypes.ensureNameNotExists(documentName);
        await umbracoApi.languages.ensureCultureNotExists(languageDa);
        await umbracoApi.templates.ensureNameNotExists(documentName);

        await umbracoApi.languages.createLanguage(languageDa, false, 1);
        const rootDocType = new DocumentTypeBuilder()
            .withName(documentName)
            .withAlias(alias)
            .withAllowAsRoot(true)
            .withAllowCultureVariation(true)
            .withDefaultTemplate(alias)
            .addGroup()
                .withName("Content")
                .addTextBoxProperty()
                    .withLabel("Title")
                    .withAlias("title")
                    .withCultureVariant(true)
                .done()
            .done()
            .build();
        await umbracoApi.documentTypes.save(rootDocType);

        await umbracoUi.goToSection(ConstantHelper.sections.content);

        // Creates content item with the created document type
        await umbracoUi.createContentWithDocumentType(documentName);

        // Adds title and input text for English culture
        await page.locator('[data-element="editor-name-field"]').type(languageEn);
        await page.locator('[name="textbox"]').type(enValue);
        await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
        await page.locator('[alias="overlaySubmit"]').click();

        // Switches to Danish culture
        await umbracoUi.switchCultureInContent("Danish")

        // Adds title and input text for Danish culture
        await page.locator('[data-element="editor-name-field"]').type(languageDa);
        await page.locator('[name="textbox"]').type(daValue);
        await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
        await page.locator('[alias="overlaySubmit"]').click();

        // Assert
        await page.locator('.umb-variant-switcher__toggle').click();
        await page.locator('.umb-variant-switcher__name-wrapper', {hasText: "English (United States)"}).hover();
        await page.locator('[role="button"]', {hasText: "Open in split view"}).click();
        await expect(page.locator('[name="textbox"]').first()).toHaveValue(daValue);
        await expect(page.locator('[name="textbox"]').nth(1)).toHaveValue(enValue);

        // Clean
        await umbracoApi.content.deleteAllContent();
        await umbracoApi.documentTypes.ensureNameNotExists(documentName);
        await umbracoApi.languages.ensureCultureNotExists(languageDa);
        await umbracoApi.templates.ensureNameNotExists(documentName);
    });

    test('publish content with two languages with different text', async ({page, umbracoApi, umbracoUi}) => {
        const documentName = "Test Document";
        const languageEn = 'en-US';
        const languageDa = 'da';
        const enValue = "USA";
        const daValue = "DENMARK";
        const alias = AliasHelper.toAlias(documentName);

        await umbracoApi.content.deleteAllContent();
        await umbracoApi.documentTypes.ensureNameNotExists(documentName);
        await umbracoApi.languages.ensureCultureNotExists(languageDa);
        await umbracoApi.templates.ensureNameNotExists(documentName);

        await umbracoApi.languages.createLanguage(languageDa, false, 1);
        const rootDocType = new DocumentTypeBuilder()
          .withName(documentName)
          .withAlias(alias)
          .withAllowAsRoot(true)
          .withAllowCultureVariation(true)
          .withDefaultTemplate(alias)
          .addGroup()
            .withName("Content")
            .addTextBoxProperty()
                .withLabel("Title")
                .withAlias("title")
                .withCultureVariant(true)
            .done()
          .done()
          .build();

        await umbracoApi.documentTypes.save(rootDocType).then(async (generatedRootDocType) => {
            const childContentNode = new ContentBuilder()
              .withContentTypeAlias(generatedRootDocType["alias"])
              .withAction("publishNew")
              .addVariant()
                .withCulture(languageEn)
                .withName(languageEn)
                .withSave(true)
                .withPublish(false)
                    .addProperty()
                    .withAlias("title")
                    .withValue(enValue)
                .done()
              .done()
              .addVariant()
                .withCulture(languageDa)
                .withName(languageDa)
                .withSave(true)
                .withPublish(false)
                .addProperty()
                    .withAlias("title")
                    .withValue(daValue)
                .done()
              .done()
              .build();
            await umbracoApi.content.save(childContentNode);
        });
        await umbracoUi.refreshContentTree();

        // Opens content
        await page.locator('[data-element="tree-item-' + languageEn + '"]').click();

        // Publishes both languages
        await page.locator('[label-key="buttons_morePublishingOptions"]').click();
        await page.locator('.umb-list-item', {hasText: "Danish"}).locator('.umb-form-check__check').click();
        await page.locator('[alias="overlaySubmit"]').click();

        // Assert
        await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "English"})).toBeVisible();
        await expect(page.locator('.umb-notifications__notifications > .alert-success', {hasText: "Danish"})).toBeVisible();

        // Clean
        await umbracoApi.content.deleteAllContent();
        await umbracoApi.documentTypes.ensureNameNotExists(documentName);
        await umbracoApi.languages.ensureCultureNotExists(languageDa);
        await umbracoApi.templates.ensureNameNotExists(documentName);
    });

    test('Check if content for two languages is published on domains with their own values', async ({page, umbracoApi, umbracoUi}) => {
        const documentName = "Test Document";
        const languageEn = 'en-US';
        const languageDa = 'da';
        const enValue = "USA";
        const daValue = "DENMARK";
        const alias = AliasHelper.toAlias(documentName);
        const daEndpoint = "/";
        const enEndpoint = "/en";

        await umbracoApi.content.deleteAllContent();
        await umbracoApi.documentTypes.ensureNameNotExists(documentName);
        await umbracoApi.languages.ensureCultureNotExists(languageDa);
        await umbracoApi.templates.ensureNameNotExists(documentName);

        await umbracoApi.languages.createLanguage(languageDa, false, 1);
        const rootDocType = new DocumentTypeBuilder()
          .withName(documentName)
          .withAlias(alias)
          .withAllowAsRoot(true)
          .withAllowCultureVariation(true)
          .withDefaultTemplate(alias)
          .addGroup()
            .withName("Content")
            .addTextBoxProperty()
                .withLabel("Title")
                .withAlias("title")
                .withCultureVariant(true)
            .done()
          .done()
          .build();

        await umbracoApi.documentTypes.save(rootDocType).then(async (generatedRootDocType) => {
            const childContentNode = new ContentBuilder()
              .withContentTypeAlias(generatedRootDocType["alias"])
              .withAction("publishNew")
              .addVariant()
                .withCulture(languageEn)
                .withName(languageEn)
                .withSave(true)
                .withPublish(true)
                .addProperty()
                    .withAlias("title")
                    .withValue(enValue)
                .done()
              .done()
              .addVariant()
                .withCulture(languageDa)
                .withName(languageDa)
                .withSave(true)
                .withPublish(true)
                .addProperty()
                    .withAlias("title")
                    .withValue(daValue)
                .done()
              .done()
              .build();
            await umbracoApi.content.save(childContentNode);
        });

        await umbracoApi.templates.edit(documentName, `@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
        @{
            Layout = null;
        }
        @{
            if (Model.HasValue("title")){
                @(Model.Value("title"))
            }
        } `
        );

        // Gets id of content and the languages for creating domains.
        const contentId = await umbracoApi.content.getContentId(languageEn);
        const langIdDa = await umbracoApi.languages.getLanguageId(languageDa);
        const langIdEn = await umbracoApi.languages.getLanguageId(languageEn);
        const domains = new DomainBuilder()
          .withNodeId(contentId)
          .addDomain()
            .withEndpoint(daEndpoint)
            .withLanguageId(langIdDa)
          .done()
          .addDomain()
            .withEndpoint(enEndpoint)
            .withLanguageId(langIdEn)
          .done()
          .build()
        await umbracoApi.domain.save(domains);

        // Assert
        await page.waitForTimeout(500);
        await expect(await umbracoApi.content.verifyRenderedContent(daEndpoint, daValue, true)).toBeTruthy();
        await expect(await umbracoApi.content.verifyRenderedContent(enEndpoint, enValue, true)).toBeTruthy();

        // Clean 
        await umbracoApi.content.deleteAllContent();
        await umbracoApi.documentTypes.ensureNameNotExists(documentName);
        await umbracoApi.languages.ensureCultureNotExists(languageDa);
        await umbracoApi.templates.ensureNameNotExists(documentName);
    });
});

