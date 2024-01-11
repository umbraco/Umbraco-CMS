import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Label tests', () => {
  let dataTypeDefaultData = null;
  let dataTypeData = null;


  test.beforeEach(async ({umbracoUi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  });

  test.afterEach(async ({umbracoApi}) => {
    if (dataTypeDefaultData !== null) {
      await umbracoApi.dataType.update(dataTypeDefaultData.id, dataTypeDefaultData); 
    }    
  });

  const labelTypes = ['Label (bigint)', 'Label (datetime)', 'Label (decimal)', 'Label (integer)', 'Label (string)', 'Label (time)'];
  for (const labelType of labelTypes) {
    test(`can change value type in ${labelType}`, async ({umbracoApi, umbracoUi}) => {  
      // Arrange
      const expectedDataTypeValues = [
        {
          "alias": "umbracoDataValueType",
          "value": "TEXT"
        }
      ];
      
      dataTypeDefaultData = await umbracoApi.dataType.getByName(labelType);
      await umbracoUi.dataType.goToDataType(labelType);

      // Act
      await umbracoUi.dataType.changeValueType('Long String');
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      await umbracoUi.dataType.isSuccessNotificationVisible();
      dataTypeData = await umbracoApi.dataType.getByName(labelType);
      expect(dataTypeData.values).toEqual(expectedDataTypeValues);
    });
  }
});
