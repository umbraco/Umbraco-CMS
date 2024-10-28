import { UMB_TEMPLATE_ALLOW_DELETE_ACTION_CONDITION_ALIAS } from './const.js';

export const manifest: UmbExtensionManifest = {
	type: 'condition',
	name: 'Template Allow Delete Action Condition',
	alias: UMB_TEMPLATE_ALLOW_DELETE_ACTION_CONDITION_ALIAS,
	api: () => import('./template-allow-delete-action.condition.js'),
};
