const tsconfigPaths = require('vite-tsconfig-paths').default;
const { mergeConfig } = require('vite');

module.exports = {
	stories: ['../src/**/*.stories.@(js|jsx|ts|tsx|mdx)', '../libs/**/*.stories.@(js|jsx|ts|tsx|mdx)'],
	addons: ['@storybook/addon-links', '@storybook/addon-essentials', '@storybook/addon-a11y'],
	framework: '@storybook/web-components',
	features: {
		previewMdx2: true,
		storyStoreV7: true,
	},
	core: {
		builder: '@storybook/builder-vite',
	},
	staticDirs: ['../public-assets'],
	async viteFinal(config, { configType }) {
		return mergeConfig(config, {
			// customize the Vite config here
			plugins: [tsconfigPaths()],
		});
	},
};
