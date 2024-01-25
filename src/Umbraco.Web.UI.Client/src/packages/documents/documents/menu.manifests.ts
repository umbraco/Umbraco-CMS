import type { ManifestMenu } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_CONTENT_MENU_ALIAS = 'Umb.Menu.Content';

const menu: ManifestMenu = {
	type: 'menu',
	alias: UMB_CONTENT_MENU_ALIAS,
	name: 'Content Menu',
	meta: {
		label: 'Content',
	},
};

export const manifests = [menu];
