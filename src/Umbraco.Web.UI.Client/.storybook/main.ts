import { dirname, join } from 'path';
import { StorybookConfig } from '@storybook/web-components-vite';
import remarkGfm from 'remark-gfm';

const config: StorybookConfig = {
	stories: ['../@(src|libs|apps|storybook)/**/*.mdx', '../@(src|libs|apps|storybook)/**/*.stories.@(js|jsx|ts|tsx)'],
	addons: [
		getAbsolutePath('@storybook/addon-links'),
		getAbsolutePath('@storybook/addon-essentials'),
		getAbsolutePath('@storybook/addon-a11y'),
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
		name: getAbsolutePath('@storybook/web-components-vite'),
		options: {},
	},
	staticDirs: [
		'../public-assets',
		'../public',
		'../src/assets',
		{
			from: '../src/packages/core/icon-registry/icons',
			to: 'assets/icons',
		},
	],
	typescript: {
		check: true,
	},
	docs: {},
	managerHead(head, { configType }) {
		const base = process.env.VITE_BASE_PATH || '/';
		const injections = [
			`<base href="${base}" />`, // This decide how storybook's main frame visit stories
		];
		return configType === 'PRODUCTION' ? `${injections.join('')}${head}` : head;
	},
	refs: {
		uui: {
			title: 'Umbraco UI Library',
			url: 'https://62189360eeb21b003ab2f4ad-vfnpsanjps.chromatic.com/',
		},
	},
};
export default config;

function getAbsolutePath(value: string): any {
	return dirname(require.resolve(join(value, 'package.json')));
}
