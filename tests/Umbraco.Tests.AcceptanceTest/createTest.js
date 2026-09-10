const fs = require('fs');
const path = require('path');

const [nameArg, dirArg] = process.argv.slice(2);

// A spec has to sit inside a directory one of the playwright.config.ts projects matches
// (DefaultConfig/**, DeliveryApi/**, SMTP/*.spec.ts, ...). A file written straight into
// tests/ matches no project and is silently never run.
const DEFAULT_PROJECT_DIR = 'DefaultConfig';

function template(testName) {
  return `import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

const documentTypeName = '${testName}DocumentType';
const contentName = '${testName}Content';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
});

test('can ${testName.replace(/([a-z0-9])([A-Z])/g, '$1 $2').toLowerCase()}', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
});
`;
}

function generate(fileName, projectDir) {
  const targetDir = path.join('./tests', projectDir);
  const targetFile = path.join(targetDir, `${fileName}.spec.ts`);

  if (fs.existsSync(targetFile)) {
    console.error(`Refusing to overwrite existing test: ${targetFile}`);
    process.exit(1);
  }

  fs.mkdirSync(targetDir, {recursive: true});
  fs.writeFileSync(targetFile, template(fileName));
  console.log(`Created ${targetFile}`);
}

generate(nameArg || 'NewTest', dirArg || DEFAULT_PROJECT_DIR);
