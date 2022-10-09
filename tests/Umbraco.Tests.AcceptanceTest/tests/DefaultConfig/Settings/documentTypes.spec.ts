import { expect } from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {DocumentTypeBuilder} from "@umbraco/json-models-builders";

test.describe('Document types', () => {
  test.beforeEach(async ({ page, umbracoApi }) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
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
});
