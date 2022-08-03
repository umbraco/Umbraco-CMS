module.exports = {
	stories: ['../src/**/*.stories.mdx', '../src/**/*.stories.@(js|jsx|ts|tsx)'],
	addons: ['@storybook/addon-links', '@storybook/addon-essentials', '@storybook/addon-a11y', '@storybook/addon-docs'],
	framework: '@storybook/web-components',
	features: {
		previewMdx2: true
	},
	core: {
		builder: '@storybook/builder-vite',
	},
};
