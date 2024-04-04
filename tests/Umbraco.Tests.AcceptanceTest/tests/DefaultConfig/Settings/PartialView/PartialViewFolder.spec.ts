import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Partial View Folder tests', () => {
  const partialViewName = 'TestPartialView';
  const partialViewFileName = partialViewName + '.cshtml';
  const folderName = 'TestFolder';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoApi.partialView.ensureNameNotExists(folderName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.partialView.goToSection(ConstantHelper.sections.settings);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.partialView.ensureNameNotExists(folderName);
  });

  test('can create a folder', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.partialView.clickActionsMenuAtRoot();
    await umbracoUi.partialView.createFolder(folderName);

    // Assert
    await umbracoUi.partialView.isSuccessNotificationVisible(); 
    expect(await umbracoApi.partialView.doesFolderExist(folderName)).toBeTruthy();
    // Verify the partial view folder is displayed under the Partial Views section
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await expect(umbracoUi.partialView.checkItemNameUnderPartialViewTree(folderName)).toBeVisible();  
  });

  test('can delete a folder @smoke', async ({umbracoApi, umbracoUi}) => {
    //Arrange
    await umbracoApi.partialView.createFolder(folderName);
    expect(await umbracoApi.partialView.doesFolderExist(folderName)).toBeTruthy();

    // Act
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await umbracoUi.partialView.clickActionsMenuForPartialView(folderName);
    await umbracoUi.partialView.deleteFolder();

    // Assert
    await umbracoUi.partialView.isSuccessNotificationVisible(); 
    expect(await umbracoApi.partialView.doesFolderExist(folderName)).toBeFalsy();
    // Verify the partial view folder is NOT displayed under the Partial Views section
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await expect(umbracoUi.partialView.checkItemNameUnderPartialViewTree(folderName)).not.toBeVisible(); 
  });

  test('can place a partial view into folder', async ({umbracoApi, umbracoUi}) => {
    //Arrange
    await umbracoApi.partialView.ensureNameNotExists(partialViewFileName);
    const folderPath = await umbracoApi.partialView.createFolder(folderName);
    expect(await umbracoApi.partialView.doesFolderExist(folderName)).toBeTruthy();

    // Act
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await umbracoUi.partialView.clickActionsMenuForPartialView(folderName);
    await umbracoUi.partialView.clickCreateThreeDotsButton();
    await umbracoUi.partialView.clickNewEmptyPartialViewButton();
    await umbracoUi.partialView.enterPartialViewName(partialViewName);
    await umbracoUi.partialView.clickSaveButton();

    // Assert
    await umbracoUi.partialView.isSuccessNotificationVisible();
    const childrenData = await umbracoApi.partialView.getChildren(folderPath);
    expect(childrenData[0].name).toEqual(partialViewFileName);    
    // Verify the partial view is displayed in the folder under the Partial Views section
    await expect(umbracoUi.partialView.checkItemNameUnderPartialViewTree(partialViewFileName)).not.toBeVisible(); 
    await umbracoUi.partialView.clickCaretButtonForName(folderName);
    await expect(umbracoUi.partialView.checkItemNameUnderPartialViewTree(partialViewFileName)).toBeVisible(); 
  });
});
