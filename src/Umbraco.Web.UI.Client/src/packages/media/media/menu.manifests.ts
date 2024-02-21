import type { ManifestMenu } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEDIA_MENU_ALIAS = 'Umb.Menu.Media';

const menu: ManifestMenu = {
	type: 'menu',
	alias: UMB_MEDIA_MENU_ALIAS,
	name: 'Media Menu',
	meta: {
		label: 'Media',
	},
};

export const manifests = [menu];
