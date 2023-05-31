import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.describe('Packages', () => {

  test.beforeEach(async ({ page, umbracoApi }, testInfo) => {
    await umbracoApi.report.report(testInfo);
    await umbracoApi.login();
  });


  test('Create member group', async ({page, umbracoApi, umbracoUi}) => {
    const name = "Test Group";
    await umbracoApi.memberGroups.ensureNameNotExists(name);
    await umbracoUi.goToSection(ConstantHelper.sections.member);

    await umbracoUi.clickElement(umbracoUi.getTreeItem(ConstantHelper.sections.member, ["Member Groups"]), { button: "right"});
    await umbracoUi.clickElement(umbracoUi.getContextMenuAction(ConstantHelper.actions.create));
    await umbracoUi.setEditorHeaderName(name)
    await umbracoUi.clickElement(umbracoUi.getButtonByLabelKey(ConstantHelper.buttons.save));
    
    // Assert
    await umbracoUi.isSuccessNotificationVisible();
    
    // Clean up
    await umbracoApi.memberGroups.ensureNameNotExists(name);
  });
});
