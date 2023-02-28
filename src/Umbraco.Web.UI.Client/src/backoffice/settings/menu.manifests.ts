import { ManifestMenu } from '@umbraco-cms/extensions-registry';

const menu: ManifestMenu = {
	type: 'menu',
	alias: 'Umb.Menu.Settings',
	name: 'Settings Menu',
	meta: {
		label: 'Settings',
	},
};

export const manifests = [menu];
