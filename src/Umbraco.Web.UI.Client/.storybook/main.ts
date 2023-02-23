import { StorybookConfig } from '@storybook/web-components-vite';

const config: StorybookConfig = {
	stories: ['../@(src|libs|apps)/**/*.mdx', '../@(src|libs|apps)/**/*.stories.@(js|jsx|ts|tsx)'],
	addons: ['@storybook/addon-links', '@storybook/addon-essentials', '@storybook/addon-a11y'],
	framework: {
		name: '@storybook/web-components-vite',
		options: {},
	},
	staticDirs: ['../public-assets'],
	typescript: {
		check: true,
	},
	docs: {
		autodocs: true,
	},
};
export default config;
