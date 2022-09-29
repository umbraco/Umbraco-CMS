import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";
import {LabelDataTypeBuilder} from "@umbraco/json-models-builders";

test.describe('Data Types', () => {
  
  test.beforeEach(async ({page, umbracoApi}) => {
    // TODO: REMOVE THIS WHEN SQLITE IS FIXED
    // Wait so we don't bombard the API
    await page.waitForTimeout(1000);
    await umbracoApi.login();
  });
  
  test('Create data type', async ({ page, umbracoApi, umbracoUi }) => {
    const name = "Test data type";

    await umbracoApi.dataTypes.ensureNameNotExists(name);

    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);

    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Data Types"]), { button: "right" });
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.create));
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.dataType));

    await umbracoUi.setEditorHeaderName(name);

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey('propertyEditorPicker_openPropertyEditorPicker'))
    await page.locator('[title="Date/Time"]').click();

    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    
    // Assert
    await umbracoUi.isSuccessNotificationVisible();

    // Clean up
    await umbracoApi.dataTypes.ensureNameNotExists(name);
  });
  
  test('Delete data type', async ({page, umbracoApi, umbracoUi}) =>  {
    const name = "Test datatype";
    
    await umbracoApi.dataTypes.ensureNameNotExists(name);
    
    const dataType = new LabelDataTypeBuilder()
      .withSaveNewAction()
      .withName(name)
      .build();
    
    await umbracoApi.dataTypes.save(dataType);
    
    await umbracoUi.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.waitForTreeLoad(ConstantHelper.sections.settings);
    
    await umbracoUi.clickElement(umbracoUi.getTreeItem("settings", ["Data Types", name]), { button: "right" });
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.delete));
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.delete));
    
    // Assert
    await expect(await (page.locator(`text=${name}`))).toHaveCount(0);
    
    // Clean up
    await umbracoApi.dataTypes.ensureNameNotExists(name);
  });
});

