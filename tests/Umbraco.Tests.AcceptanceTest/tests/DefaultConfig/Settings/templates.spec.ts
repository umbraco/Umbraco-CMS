import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {TemplateBuilder} from "@umbraco/json-models-builders";

test.describe('Templates', () => {
  test.beforeEach(async ({page, umbracoApi}) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });

  async function navigateToSettings(page, umbracoUi) {
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);
  }

  async function createTemplate(page, umbracoUi) {
    await navigateToSettings(page, umbracoUi);
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Templates"]), {button: "right"});
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.create));
  }

  test('Create template', async ({page, umbracoApi, umbracoUi}) => {
    const name = "Create template test";
    await umbracoApi.templates.ensureNameNotExists(name);

    await createTemplate(page, umbracoUi);
    // We have to wait for the ace editor to load, because when the editor is loading it will "steal" the focus briefly,
    // which causes the save event to fire if we've added something to the header field, causing errors.
    await page.waitForTimeout(500);

    // Type name
    await umbracoUi.getEditorHeaderName(name)
    // Save
    // We must drop focus for the auto save event to occur.
    await page.focus('.btn-success');
    // And then wait for the auto save event to finish by finding the page in the tree view.
    // This is a bit of a roundabout way to find items in a tree view since we dont use umbracoTreeItem
    // but we must be able to wait for the save event to finish, and we can't do that with umbracoTreeItem
    const label = await page.locator("a:has-text('Create template test')");
    await expect(label).toBeVisible({timeout: 10000});
    // Now that the auto save event has finished we can save
    // and there wont be any duplicates or file in use errors.
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    //Assert
    await umbracoUi.isSuccessNotificationVisible();
    // For some reason cy.umbracoErrorNotification tries to click the element which is not possible
    // if it doesn't actually exist, making should('not.be.visible') impossible.
    await expect(await page.locator('.umb-notifications__notifications > .alert-error')).not.toBeVisible();

    // Clean up
    await umbracoApi.templates.ensureNameNotExists(name);
  });

  test('Unsaved changes stay', async ({page, umbracoApi, umbracoUi}) => {
    const name = "Templates Unsaved Changes Stay test";
    const edit = "var num = 5;";
    await umbracoApi.templates.ensureNameNotExists(name);

    const template = new TemplateBuilder()
      .withName(name)
      .withContent('@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n')
      .build();

    await umbracoApi.templates.saveTemplate(template);

    await navigateToSettings(page, umbracoUi);

    // Open partial view
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Templates", name]));
    // Edit
    await page.locator('.ace_content').type(edit);
    await expect(await page.locator('.ace_content')).toBeVisible();
    await expect(await page.locator('.btn-success')).toBeVisible();

    // Navigate away
    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.content);

    // Click stay button
    await page.locator('umb-button[label="Stay"] button:enabled').click();

    // Assert
    // That the same document is open
    await expect(await page.locator('#headerName')).toHaveValue(name);
    await expect(await page.locator('.ace_content')).toContainText(edit);

    // Clean up
    await umbracoApi.templates.ensureNameNotExists(name);
  });

  test('Discard unsaved changes', async ({page, umbracoApi, umbracoUi}) => {
    const name = "Discard changes test";
    const edit = "var num = 5;";

    await umbracoApi.templates.ensureNameNotExists(name);

    const template = new TemplateBuilder()
      .withName(name)
      .withContent('@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n')
      .build();

    await umbracoApi.templates.saveTemplate(template);

    await navigateToSettings(page, umbracoUi);

    // Open partial view
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Templates", name]));
    // Edit
    await page.locator('.ace_content').type(edit);
    await expect(await page.locator('.ace_content')).toBeVisible();
    await expect(await page.locator('.btn-success')).toBeVisible();

    // Navigate away
    await umbracoUi.goToSection(ConstantHelper.sections.content);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.content);
    // Click discard
    await page.locator('umb-button[label="Discard changes"] button:enabled').click();
    // Navigate back
    await navigateToSettings(page, umbracoUi);

    // Asserts
    await expect(await page.locator('.ace_content')).not.toContainText(edit);
    // cy.umbracoPartialViewExists(fileName).then(exists => { expect(exists).to.be.false; }); TODO: Switch to template
    await umbracoApi.templates.ensureNameNotExists(name);
  });

  test('Insert macro', async ({page, umbracoApi, umbracoUi}) => {
    const name = 'InsertMacroTest';

    await umbracoApi.templates.ensureNameNotExists(name);
    await umbracoApi.macros.ensureNameNotExists(name);

    const template = new TemplateBuilder()
      .withName(name)
      .withContent('')
      .build();

    await umbracoApi.templates.saveTemplate(template);

    await umbracoApi.macros.save(name);

    await navigateToSettings(page, umbracoUi);
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Templates", name]));
    // Insert macro
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.insert));

    await page.locator('.umb-insert-code-box__title >> text=Macro').click();
    await page.locator(`.umb-card-grid-item[title='${name}']`).click();

    // Assert
    await expect(await page.locator('.ace_content')).toContainText('@await Umbraco.RenderMacroAsync("' + name + '")');

    // Clean
    await umbracoApi.templates.ensureNameNotExists(name);
    await umbracoApi.macros.ensureNameNotExists(name);
  });  
  
  test('Insert value', async ({page, umbracoApi, umbracoUi}) => {
    const name = 'Insert Value Test';

    await umbracoApi.templates.ensureNameNotExists(name);

    const partialView = new TemplateBuilder()
      .withName(name)
      .withContent('')
      .build();

    await umbracoApi.templates.saveTemplate(partialView);

    await navigateToSettings(page, umbracoUi);
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Templates", name]));

    // Insert value
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.insert));
    await page.locator('.umb-insert-code-box__title >> text=Value').click();

    await page.selectOption('select', 'umbracoBytes');
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submit));

    // assert
    await expect(await page.locator('.ace_content')).toContainText('@Model.Value("umbracoBytes")');

    // Clean
    await umbracoApi.templates.ensureNameNotExists(name);
  });
});
