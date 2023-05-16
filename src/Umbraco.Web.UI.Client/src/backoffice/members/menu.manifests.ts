import { ManifestMenu } from '@umbraco-cms/backoffice/extension-registry';

const menu: ManifestMenu = {
	type: 'menu',
	alias: 'Umb.Menu.Members',
	name: 'Members Menu',
	meta: {
		label: 'Members',
	},
};

export const manifests = [menu];
