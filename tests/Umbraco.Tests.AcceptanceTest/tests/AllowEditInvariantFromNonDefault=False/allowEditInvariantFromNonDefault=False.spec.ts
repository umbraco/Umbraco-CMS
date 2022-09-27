import {AliasHelper, ApiHelpers, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {ContentBuilder, DocumentTypeBuilder, MacroBuilder, PartialViewMacroBuilder} from "@umbraco/playwright-models";

test.describe('Test for AllowEditInvariantFromNonDefault=False', () => {
    const rootDocTypeName = 'TestDocument';
    const languageEn = 'en-US';
    const languageDa = 'da';
    
    test.beforeEach(async ({page, umbracoApi}) => {
        await umbracoApi.login();
        await umbracoApi.content.deleteAllContent();
        await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
        await umbracoApi.languages.ensureCultureNotExists(languageDa);
        await umbracoApi.templates.ensureNameNotExists(rootDocTypeName);
    });

    test.afterEach(async ({page, umbracoApi}) => {
        await umbracoApi.content.deleteAllContent();
        await umbracoApi.documentTypes.ensureNameNotExists(rootDocTypeName);
        await umbracoApi.languages.ensureCultureNotExists(languageDa);
        await umbracoApi.templates.ensureNameNotExists(rootDocTypeName);
    });
    
    async function createDocWithCultureVariationWithContent(umbracoApi, name, alias, language1, language2, value, isPublished){
        const rootDocType = new DocumentTypeBuilder()
          .withName(name)
          .withAlias(alias)
          .withAllowAsRoot(true)
          .withAllowCultureVariation(true)
          .withDefaultTemplate(alias)
          .addGroup()
            .withName("Content")
            .addTextBoxProperty()
                .withLabel("Title")
                .withAlias("title")
            .done()
          .done()
          .build();

        await umbracoApi.documentTypes.save(rootDocType).then(async (generatedRootDocType) => {
            const childContentNode = new ContentBuilder()
              .withContentTypeAlias(generatedRootDocType["alias"])
              .withAction("publishNew")
              .addVariant()
                .withCulture(language1)
                .withName(language1)
                .withSave(true)
                .withPublish(isPublished)
                .addProperty()
                    .withAlias("title")
                    .withValue(value)
                .done()
              .done()
              .addVariant()
                .withCulture(language2)
                .withName(language2)
                .withSave(true)
                .withPublish(isPublished)
              .done()
              .build();

            await umbracoApi.content.save(childContentNode);
        });
    }
    
    test('No edit button for content when language changed', async ({page, umbracoApi, umbracoUi}) => {
        const alias = AliasHelper.toAlias(rootDocTypeName);

        await umbracoApi.languages.createLanguage(languageDa, false, 1);
        await createDocWithCultureVariationWithContent(umbracoApi, rootDocTypeName, alias, languageEn, languageDa, "", false);

        await umbracoUi.refreshContentTree();
        await page.locator('[data-element="tree-item-' + languageEn + '"]').click();
        await page.locator('.umb-variant-switcher__toggle').click();
        await page.locator('.umb-variant-switcher__name-wrapper', {hasText: "Danish"}).click();

        // Assert
        await expect(await page.locator('.umb-property-editor__lock-overlay')).not.toBeVisible();

        // Cleaned
    });
    
    test('Updating value and publishing non-default language only without saving default should not update value', async ({page, umbracoApi, umbracoUi}) => {
        const text = 'USA';
        const updatedText = 'DENMARK';
        const endpoint = '/';
        const alias = AliasHelper.toAlias(rootDocTypeName);

        await umbracoApi.languages.createLanguage(languageDa, false, 1);
        await createDocWithCultureVariationWithContent(umbracoApi, rootDocTypeName, alias, languageEn, languageDa, text, true);
        await umbracoApi.templates.edit(rootDocTypeName, `@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
        @{
            Layout = null;
        }
        @{
            if (Model.HasValue("title")){
                @(Model.Value("title"))
            }
        } `
        );
        const contentId = await umbracoApi.content.getContentId(languageEn);
        const langId = await umbracoApi.languages.getLanguageId(languageDa);
        await umbracoApi.domain.createDomain(endpoint, contentId, langId);
        
        await umbracoUi.refreshContentTree();
        await page.locator('[data-element="tree-item-' + languageEn + '"]').click();
        await page.locator('.umb-variant-switcher__toggle').click();
        await page.locator('.umb-variant-switcher__name-wrapper', {hasText: "Danish"}).hover();
        await page.locator('[role="button"]', {hasText: "Open in split view"}).click();
        
        await page.locator('.umb-split-view', {hasText: languageEn}).locator('[name="textbox"]').fill(updatedText);
        await page.locator('[label-key="buttons_morePublishingOptions"]').click();
        await page.locator('.umb-list-item', {hasText: "English"}).locator('.umb-form-check__check').click();
        await page.locator('[alias="overlaySubmit"]').click();
        
        // Assert
        await page.waitForTimeout(500);
        await expect(await umbracoApi.content.verifyRenderedContent(endpoint, text, true)).toBeTruthy();
        
        // Cleaned
    });
});