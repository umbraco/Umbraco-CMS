import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifests: Array<ManifestCondition> = [
	{
		type: 'condition',
		name: 'Block Has Settings Condition',
		alias: 'Umb.Condition.BlockWorkspaceHasSettings',
		api: () => import('./block-workspace-has-settings.condition.js'),
	},
	{
		type: 'condition',
		name: 'Block Show Content Edit Condition',
		alias: 'Umb.Condition.BlockEntryShowContentEdit',
		api: () => import('./block-entry-show-content-edit.condition.js'),
	},
	{
		type: 'condition',
		name: 'Block Workspace Is Exposed Condition',
		alias: 'Umb.Condition.BlockWorkspaceIsExposed',
		api: () => import('./block-workspace-is-exposed.condition.js'),
	},
];
