import { expect } from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {DocumentTypeBuilder} from "@umbraco/json-models-builders";

test.describe('Document types', () => {
  test.beforeEach(async ({ page, umbracoApi }, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
  });

  test('Create document type', async ({ page, umbracoApi, umbracoUi }) => {
    const name = "Test document type";

    await umbracoApi.documentTypes.ensureNameNotExists(name);
    await umbracoApi.templates.ensureNameNotExists(name);

    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Document Types"]), {button: "right"})

    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.create));
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.documentType));

    await umbracoUi.setEditorHeaderName(name);

    // TODO: Create an GetButtonByDataElement? seems like it could be useful
    // Add a property group
    await page.locator('[data-element="group-add"]').click();
    await page.locator('.umb-group-builder__group-title-input').type('Group name');
    // Add a property
    await page.locator('[data-element="property-add"]').click();
    await page.locator('.editor-label').type('property name');
    await page.locator('[data-element="editor-add"]').click();

    // Search for textstring
    await page.locator('#datatype-search').type('Textstring');

    await page
      .locator('ul.umb-card-grid li [title="Textstring"]')
      .locator("xpath=ancestor::li")
      .click();

    await page.locator(".btn-success").last().click();
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save))

    // Assert
    await umbracoUi.isSuccessNotificationVisible();

    // Clean up
    await umbracoApi.documentTypes.ensureNameNotExists(name);
    await umbracoApi.templates.ensureNameNotExists(name);
  });

  test('Delete document type', async ({ page, umbracoApi, umbracoUi }) => {
    const name = "Test document type";

    await umbracoApi.documentTypes.ensureNameNotExists(name);
    await umbracoApi.templates.ensureNameNotExists(name);

    const documentType = new DocumentTypeBuilder()
      .withName(name)
      .build();

    await umbracoApi.documentTypes.save(documentType);

    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Document Types", name]), {button: "right"})
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.delete));
    await page.locator('label.checkbox').click();

    // This delete button for some reason does not have the usual general_delete label
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey("delete"));

    const docTypeLocator = await page.locator("text=" + name);

    // Assert
    await expect(docTypeLocator).toHaveCount(0);

    // Clean up
    await umbracoApi.documentTypes.ensureNameNotExists(name);
    await umbracoApi.templates.ensureNameNotExists(name);
  });

  test('Correct an invalid alias for a new property on a new doc type', async ({ page, umbracoApi, umbracoUi }) => {
    const name = "Test document type 2";

    await umbracoApi.documentTypes.ensureNameNotExists(name);
    await umbracoApi.templates.ensureNameNotExists(name);

    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Document Types"]), {button: "right"})

    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.create));
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.documentType));

    await umbracoUi.setEditorHeaderName(name);

    // Add a property group
    await page.locator('[data-element="group-add"]').click();
    await page.locator('.umb-group-builder__group-title-input').fill('Group name');
    // Add a property
    await page.locator('[data-element="property-add"]').click();
    // enter a label that will generate an invalid alias
    await page.locator('.editor-label').fill('A');
    await page.locator('[data-element="editor-add"]').click();

    // Search for textstring
    await page.locator('#datatype-search').fill('Textstring');

    await page
      .locator('ul.umb-card-grid li [title="Textstring"]')
      .locator("xpath=ancestor::li")
      .click();

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submit));

    // ensure we got an error that our property alias was invalid
    await expect(page.locator('[alias="model.property.alias"] .umb-validation-label')).toBeVisible();

    // click the lock icon to change the alias
    await page.locator('[alias="model.property.alias"] .umb-locked-field__toggle').click();

    // change the property alias to a valid alias
    await page.locator('[alias="model.property.alias"] [name="lockedField"]').fill('test');

    // submit our new property
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.submit));

    // ensure we no longer get an error that our property alias is invalid
    await expect(page.locator('[alias="model.property.alias"] .umb-validation-label')).toBeHidden();

    // Assert that the doc type has now been saved successfully
    await umbracoUi.isSuccessNotificationVisible();

    // Clean up
    await umbracoApi.documentTypes.ensureNameNotExists(name);
    await umbracoApi.templates.ensureNameNotExists(name);
  });
});
