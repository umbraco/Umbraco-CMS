import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const packageName = 'Umbraco Commerce';
const packageVersion = '16.4.0';


test('is commerce package installed', async ({page, umbracoUi}) => {
  // Act
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.packages);
  await umbracoUi.package.clickInstalledTab();

  // Assert
  // DOUBLE DEFINED? IT IS ALSO SET IN THE PIPELINE WITH THE VERSION
  await umbracoUi.package.doesPackageWithNameContainVersion(packageName, packageVersion);
});


