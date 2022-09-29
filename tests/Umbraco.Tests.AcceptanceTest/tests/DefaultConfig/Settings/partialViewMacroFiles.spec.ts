import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {PartialViewMacroBuilder} from "@umbraco/json-models-builders";

test.describe('Partial View Macro Files', () => {

    test.beforeEach(async ({page, umbracoApi}) => {
        // TODO: REMOVE THIS WHEN SQLITE IS FIXED
        // Wait so we don't bombard the API
        await page.waitForTimeout(1000);
        await umbracoApi.login();
    });

    async function openPartialViewMacroCreatePanel(page, umbracoUi) {
        await umbracoUi.goToSection('settings');
        await umbracoUi.waitForTreeLoad('settings');
        await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Partial View Macro Files"]), {button: "right"});
        await umbracoUi.clickElement(umbracoUi.getContextMenuAction("action-create"));
    }

    async function cleanup(umbracoApi, name, extension = ".cshtml") {
        const fileName = name + extension;
        await umbracoApi.macros.ensureNameNotExists(name);
        await umbracoApi.partialViews.ensureMacroFileNameNotExists(fileName);
    }

    test('Create new partial view macro', async ({page, umbracoApi, umbracoUi}) => {
        const name = "TestPartialViewMacro";

        await cleanup(umbracoApi, name);

        await openPartialViewMacroCreatePanel(page, umbracoUi);

        await page.locator('.menu-label localize[key="create_newPartialViewMacro"]').click();

        //Type name
        await umbracoUi.setEditorHeaderName(name);

        //Save
        await page.locator('.btn-success').click();

        //Assert
        await umbracoUi.isSuccessNotificationVisible();

        //Clean up
        await cleanup(umbracoApi, name);
    });    

    test('Create new partial view macro without macro', async ({page, umbracoApi, umbracoUi}) => {
        const name = "TestPartialMacrolessMacro";

        await cleanup(umbracoApi, name);

        await openPartialViewMacroCreatePanel(page, umbracoUi);

        await page.locator('.menu-label >> nth=1').click();

        // Type name
        await umbracoUi.setEditorHeaderName(name);

        // Save
        await page.locator('.btn-success').click();

        // Assert
        await umbracoUi.isSuccessNotificationVisible();

        // Clean
        await cleanup(umbracoApi, name);
    });    

    test('Create new partial view macro from snippet', async ({page, umbracoApi, umbracoUi}) => {
        const name = "TestPartialFromSnippet";

        await cleanup(umbracoApi, name);

        await openPartialViewMacroCreatePanel(page, umbracoUi);

        await page.locator('.menu-label >> nth=2').click();

        // Select snippet
        await page.locator('.menu-label >> nth=1').click();

        // Type name
        await umbracoUi.setEditorHeaderName(name);

        // Save
        await page.locator('.btn-success').click();

        // Assert
        await umbracoUi.isSuccessNotificationVisible();

        // Clean
        await cleanup(umbracoApi, name);
    });    

    test('Delete partial view macro', async ({page, umbracoApi, umbracoUi}) => {
        const name = "TestDeletePartialViewMacro";
        const fullName = name + ".cshtml"

        await cleanup(umbracoApi, name);

        const partialViewMacro = new PartialViewMacroBuilder()
            .withName(name)
            .withContent("@inherits Umbraco.Web.Macros.PartialViewMacroPage")
            .build();

        await umbracoApi.partialViews.save(partialViewMacro);

        // Navigate to settings
        await umbracoUi.goToSection(ConstantHelper.sections.settings);
        await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);

        // Delete partialViewMacro
        await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Partial View Macro Files", fullName]), {button: "right"});
        await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.delete));
        await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.ok));

        // Assert
        await expect(await page.locator("body",{ hasText: fullName})).not.toBeVisible();

        // Clean
        await cleanup(umbracoApi, name);
    });    

    test('Edit partial view macro', async ({page, umbracoApi, umbracoUi}) => {
        const name = "TestPartialViewMacroEditable";
        const fullName = name + ".cshtml";

        await cleanup(umbracoApi, name);

        const partialViewMacro = new PartialViewMacroBuilder()
            .withName(name)
            .withContent("@inherits Umbraco.Web.Macros.PartialViewMacroPage")
            .build();

        await umbracoApi.partialViews.save(partialViewMacro);

        // Navigate to settings
        await umbracoUi.goToSection(ConstantHelper.sections.settings);
        await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);
        await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Partial View Macro Files", fullName]));

        // Type an edit
        await page.locator('.ace_text-input').type(" // test" );
        
        // Save
        await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

        // Assert
        await umbracoUi.isSuccessNotificationVisible();

        await cleanup(umbracoApi, name);
    });
});
