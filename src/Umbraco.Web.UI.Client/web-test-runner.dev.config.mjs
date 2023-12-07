import defaultConfig from './web-test-runner.config.mjs';

/** @type {import('@web/dev-server').DevServerConfig} */
export default {
	...defaultConfig,
	testRunnerHtml: (testFramework) => {
		return defaultConfig.testRunnerHtml(testFramework, true);
	},
};
