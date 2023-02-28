import { ManifestMenu } from '@umbraco-cms/extensions-registry';

const menu: ManifestMenu = {
	type: 'menu',
	alias: 'Umb.Menu.Dictionary',
	name: 'Dictionary Menu',
	meta: {
		label: 'Dictionary',
	},
};

export const manifests = [menu];
