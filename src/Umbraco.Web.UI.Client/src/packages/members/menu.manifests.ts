import { ManifestMenu } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MEMBER_MENU_ALIAS = 'Umb.Menu.Member';

const menu: ManifestMenu = {
	type: 'menu',
	alias: UMB_MEMBER_MENU_ALIAS,
	name: 'Members Menu',
	meta: {
		label: 'Members',
	},
};

export const manifests = [menu];
