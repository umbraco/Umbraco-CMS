const fs = require('fs');
const path = require('path');

const [nameArg, areaArg] = process.argv.slice(2);

// Specs are written under DefaultConfig/, the project matched by `defaultConfig` in playwright.config.ts.
// A file outside a project's directory matches no project and is silently never run.
const PROJECT_DIR = 'DefaultConfig';

function fail(message) {
  console.error(message);
  console.error('Usage: npm run createTest <PascalCaseName> [Area/SubArea under tests/DefaultConfig]');
  process.exit(1);
}

if (!nameArg || !/^[A-Z][A-Za-z0-9]*$/.test(nameArg)) {
  fail(`Expected a PascalCase test name, got: ${nameArg ?? '(none)'}`);
}
if (areaArg !== undefined && !/^[A-Z][A-Za-z0-9]*(\/[A-Z][A-Za-z0-9]*)*$/.test(areaArg)) {
  fail(`Expected an area path such as Content or Settings/Webhook, got: ${areaArg}`);
}

function template(testName) {
  return `import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

const documentTypeName = '${testName}DocumentType';
const contentName = '${testName}Content';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can ${testName.replace(/([a-z0-9])([A-Z])/g, '$1 $2').toLowerCase()}', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
});
`;
}

const targetDir = path.join(__dirname, 'tests', PROJECT_DIR, ...(areaArg ? areaArg.split('/') : []));
const targetFile = path.join(targetDir, `${nameArg}.spec.ts`);

const relativeFile = path.relative(__dirname, targetFile);

if (fs.existsSync(targetFile)) {
  console.error(`Refusing to overwrite existing test: ${relativeFile}`);
  process.exit(1);
}

fs.mkdirSync(targetDir, {recursive: true});
fs.writeFileSync(targetFile, template(nameArg));
console.log(`Created ${relativeFile}`);
