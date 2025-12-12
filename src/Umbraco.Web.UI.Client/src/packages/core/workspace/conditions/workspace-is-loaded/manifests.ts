import { UMB_ENTITY_DETAIL_WORKSPACE_IS_LOADED_CONDITION_ALIAS } from './constants.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Entity Detail Workspace Is Loaded Condition',
	alias: UMB_ENTITY_DETAIL_WORKSPACE_IS_LOADED_CONDITION_ALIAS,
	api: () => import('./workspace-is-loaded.condition.js'),
};
