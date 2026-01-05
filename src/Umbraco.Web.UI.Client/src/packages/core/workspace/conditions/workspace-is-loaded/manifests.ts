import { UMB_CONTENT_WORKSPACE_IS_LOADED_CONDITION_ALIAS } from './constants.js';
import type { ManifestCondition } from '@umbraco-cms/backoffice/extension-api';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Content Workspace Is Loaded Condition',
	alias: UMB_CONTENT_WORKSPACE_IS_LOADED_CONDITION_ALIAS,
	api: () => import('./workspace-is-loaded.condition.js'),
};
