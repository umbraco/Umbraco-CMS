const fs = require('fs');

const args = process.argv.slice(2);

function generate(fileName) {
    fs.writeFileSync(
        `./tests/${fileName}.spec.ts`,
        `import {test} from '@umbraco/playwright-testhelpers';

test.describe('New test file description', () => {
  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.login();
  });

  test('New test', async ({page, umbracoApi, umbracoUi}) => {
  });
});`
    );
}

generate(args[0] || "newTest");