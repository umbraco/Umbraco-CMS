import { ManifestMenu } from '@umbraco-cms/extensions-registry';

const menu: ManifestMenu = {
	type: 'menu',
	alias: 'Umb.Menu.Content',
	name: 'Content Menu',
	meta: {
		label: 'Content',
	},
};

export const manifests = [menu];
