import {
	UMB_BLOCK_ENTRY_HAS_SETTINGS_CONDITION_ALIAS,
	UMB_BLOCK_ENTRY_IS_EXPOSED_CONDITION_ALIAS,
	UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS,
} from './constants.js';
import UmbBlockEntryHasSettingsCondition from './block-entry-has-settings.condition.js';
import { UmbBlockEntryIsExposedCondition as UmbBlockEntryIsExposedConditionEntry } from './block-entry-is-exposed.condition.js';
import UmbBlockEntryIsReadOnlyCondition from './block-entry-is-read-only.condition.js';
import UmbBlockEntryShowContentEditCondition from './block-entry-show-content-edit.condition.js';
import UmbBlockWorkspaceHasSettingsCondition from './block-workspace-has-settings.condition.js';
import UmbBlockEntryIsExposedCondition from './block-workspace-is-exposed.condition.js';
import UmbBlockWorkspaceIsReadOnlyCondition from './block-workspace-is-readonly.condition.js';
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
	{
		type: 'condition',
		name: 'Block Workspace Is ReadOnly Condition',
		alias: 'Umb.Condition.BlockWorkspaceIsReadOnly',
		api: UmbBlockWorkspaceIsReadOnlyCondition,
	},
	{
		type: 'condition',
		name: 'Block Entry Has Settings Condition',
		alias: UMB_BLOCK_ENTRY_HAS_SETTINGS_CONDITION_ALIAS,
		api: UmbBlockEntryHasSettingsCondition,
	},
	{
		type: 'condition',
		name: 'Block Entry Is Exposed Condition',
		alias: UMB_BLOCK_ENTRY_IS_EXPOSED_CONDITION_ALIAS,
		api: UmbBlockEntryIsExposedConditionEntry,
	},
	{
		type: 'condition',
		name: 'Block Entry Is ReadOnly Condition',
		alias: UMB_BLOCK_ENTRY_IS_READ_ONLY_CONDITION_ALIAS,
		api: UmbBlockEntryIsReadOnlyCondition,
	},
];
