import defaultConfig from './web-test-runner.config.mjs';

/** @type {import('@web/dev-server').DevServerConfig} */
export default {
	...defaultConfig,
	// Only test with the first browser option ( to keep test times fast )
	browsers: [defaultConfig.browsers[0]],
	testRunnerHtml: (testFramework) => {
		return defaultConfig.testRunnerHtml(testFramework, true);
	},
};
