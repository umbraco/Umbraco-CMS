import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {ScriptBuilder} from "@umbraco/json-models-builders";

test.describe('Scripts', () => {
  test.beforeEach(async ({ page, umbracoApi }) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });

  test('Create new Javascript file', async ({ page, umbracoApi, umbracoUi }) => {
    const name = "CreateNewScript";
    const fileName = name + ".js";

    await umbracoApi.scripts.ensureNameNotExists(fileName);

    await umbracoUi.goToSection("settings");
    await umbracoUi.waitForTreeLoad('settings');

    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Scripts"]), { button: "right"});
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction("action-create"));
    await page.locator('.menu-label localize[key="create_newJavascriptFile"]').click();
    await page.waitForResponse('**/umbraco/lib/ace-builds/src-min-noconflict/worker-javascript.js');

    await umbracoUi.setEditorHeaderName(name);
    await page.locator('.btn-success').click();

    await umbracoUi.isSuccessNotificationVisible();

    expect(await umbracoApi.scripts.exists(fileName)).toBeTruthy();

    await umbracoApi.scripts.ensureNameNotExists(fileName);
  });

  test('Delete a JavaScript file', async ({ page, umbracoApi, umbracoUi}) => {
    const name = "TestDeleteScriptFile";
    const fileName = name + ".js";

    await umbracoApi.scripts.ensureNameNotExists(fileName);

    const script = new ScriptBuilder()
        .withName(name)
        .withContent('alert("this is content");')
        .build();

    await umbracoApi.scripts.save(script);

    await umbracoUi.goToSection("settings");
    await umbracoUi.waitForTreeLoad('settings');

    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Scripts", fileName]), { button: "right"});
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction("action-delete"));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey("general_ok"));

    await expect(await (page.locator(`text=${fileName}`))).toHaveCount(0);
    expect(await (umbracoApi.scripts.exists(fileName))).toBeFalsy();
  });

  test('Update JavaScript file', async ({ page, umbracoApi, umbracoUi }) => {
    const name = "TestEditJavaScriptFile";
    const nameEdit = "Edited";
    const fileName = name + ".js";
    const editedFileName = name + nameEdit + ".js";
    await umbracoApi.scripts.ensureNameNotExists(fileName);
    await umbracoApi.scripts.ensureNameNotExists(editedFileName);

    const originalContent = 'console.log("A script);\n';
    const edit = 'alert("content");';
    const expected = originalContent + edit;

    const script = new ScriptBuilder()
        .withName(name)
        .withContent(originalContent)
        .build();

    await umbracoApi.scripts.save(script);

    await umbracoUi.goToSection("settings");
    await umbracoUi.waitForTreeLoad('settings');
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Scripts", fileName]));

    await page.locator('.ace_text-input').type(edit);

    const header = await page.locator("#headerName");

    await header.click();
    // The ui appends .js automagically, so we have to delete that before we can edit the name.
    await page.keyboard.press("Backspace");
    await page.keyboard.press("Backspace");
    await page.keyboard.press("Backspace");
    await header.type(nameEdit);
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));

    await umbracoUi.isSuccessNotificationVisible();
    expect(await umbracoApi.scripts.verifyContent(editedFileName, expected)).toBeTruthy();

    await umbracoApi.scripts.ensureNameNotExists(fileName);
    await umbracoApi.scripts.ensureNameNotExists(editedFileName);
  });
  
  test('Can delete folder', async ({ page, umbracoApi, umbracoUi }) => {
    
    const folderName = "TestFolder";
    await umbracoApi.scripts.ensureNameNotExists(folderName);
    await umbracoApi.scripts.saveFolder(folderName);
    
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Scripts", folderName]), {button: "right"});
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.delete));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.ok));
    
    await expect(await page.locator(`text=${folderName}`)).toHaveCount(0);
    expect(await umbracoApi.scripts.exists(folderName)).toBeFalsy();

    await umbracoApi.scripts.ensureNameNotExists(folderName);
  });
});