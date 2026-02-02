import UmbBlockEntryShowContentEditCondition from './block-entry-show-content-edit.condition.js';
import UmbBlockWorkspaceHasSettingsCondition from './block-workspace-has-settings.condition.js';
import UmbBlockEntryIsExposedCondition from './block-workspace-is-exposed.condition.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifests: Array<ManifestCondition> = [
	{
		type: 'condition',
		name: 'Block Has Settings Condition',
		alias: 'Umb.Condition.BlockWorkspaceHasSettings',
		api: UmbBlockWorkspaceHasSettingsCondition,
	},
	{
		type: 'condition',
		name: 'Block Show Content Edit Condition',
		alias: 'Umb.Condition.BlockEntryShowContentEdit',
		api: UmbBlockEntryShowContentEditCondition,
	},
	{
		type: 'condition',
		name: 'Block Workspace Is Exposed Condition',
		alias: 'Umb.Condition.BlockWorkspaceIsExposed',
		api: UmbBlockEntryIsExposedCondition,
	},
];
