import { StorybookConfig } from '@storybook/web-components-vite';
import remarkGfm from 'remark-gfm';

const config: StorybookConfig = {
	stories: ['../@(src|libs|apps|storybook)/**/*.mdx', '../@(src|libs|apps|storybook)/**/*.stories.@(js|jsx|ts|tsx)'],
	addons: [
		'@storybook/addon-links',
		'@storybook/addon-essentials',
		'@storybook/addon-a11y',
		{
			name: '@storybook/addon-docs',
			options: {
				mdxPluginOptions: {
					mdxCompileOptions: {
						remarkPlugins: [remarkGfm],
					},
				},
			},
		},
	],
	framework: {
		name: '@storybook/web-components-vite',
		options: {},
	},
	staticDirs: ['../public-assets', '../public'],
	typescript: {
		check: true,
	},
	docs: {
		autodocs: true,
	},
	managerHead(head, { configType }) {
		const base = process.env.VITE_BASE_PATH || '/';
		const injections = [
			`<base href="${base}" />`, // This decide how storybook's main frame visit stories
		];
		return configType === 'PRODUCTION' ? `${injections.join('')}${head}` : head;
	},
	refs: {
		uui: {
			title: 'Umbraco UI Library (1.3.0)',
			url: 'https://e662ac3--62189360eeb21b003ab2f4ad.chromatic.com/',
		},
	},
};
export default config;
