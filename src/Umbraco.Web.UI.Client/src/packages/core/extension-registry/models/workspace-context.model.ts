import type { ConditionTypes } from '../conditions/types.js';
import type { ManifestApi, ManifestWithDynamicConditions, UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceContext extends ManifestWithDynamicConditions<ConditionTypes>, ManifestApi<UmbApi> {
	type: 'workspaceContext';
}
