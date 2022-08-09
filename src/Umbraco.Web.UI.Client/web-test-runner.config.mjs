import { esbuildPlugin } from '@web/dev-server-esbuild';
import { playwrightLauncher } from '@web/test-runner-playwright';

export default {
  nodeResolve: true,
  files: 'src/**/*.test.ts',
  plugins: [esbuildPlugin({ ts: true, target: 'auto' })],
  browsers: [
    playwrightLauncher({ product: 'chromium' }),
    playwrightLauncher({ product: 'firefox' }),
    playwrightLauncher({ product: 'webkit' }),
  ],
	coverageConfig: {
		reporters: ['lcov', 'text-summary'],
	},
  testRunnerHtml: (testFramework) =>
    `<html>
      <body>
        <script type="module" src="${testFramework}"></script>
        <script type="module">
					import 'element-internals-polyfill';
					import '@umbraco-ui/uui';
					import 'router-slot';
        </script>
      </body>
    </html>`,
};
