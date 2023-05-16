import { ManifestMenu } from '@umbraco-cms/backoffice/extension-registry';

const menu: ManifestMenu = {
	type: 'menu',
	alias: 'Umb.Menu.Media',
	name: 'Media Menu',
	meta: {
		label: 'Media',
	},
};

export const manifests = [menu];
