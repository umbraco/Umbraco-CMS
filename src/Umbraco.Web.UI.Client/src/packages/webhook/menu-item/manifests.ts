import { UMB_WEBHOOK_ENTITY_TYPE } from '../entity.js';
import type { ManifestMenuItem } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Webhook',
	name: 'Webhook Menu Item',
	weight: 100,
	meta: {
		label: 'Webhooks',
		icon: 'icon-webhook',
		entityType: UMB_WEBHOOK_ENTITY_TYPE,
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
