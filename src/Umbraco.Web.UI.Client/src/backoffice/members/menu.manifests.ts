import { ManifestMenu } from '@umbraco-cms/backoffice/extensions-registry';

const menu: ManifestMenu = {
	type: 'menu',
	alias: 'Umb.Menu.Members',
	name: 'Members Menu',
	meta: {
		label: 'Members',
	},
};

export const manifests = [menu];
