import type { ConditionTypes } from '../conditions/types.js';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceFooterApp
	extends ManifestElementAndApi<HTMLElement, any>,
		ManifestWithDynamicConditions<ConditionTypes> {
	type: 'workspaceFooterApp';
}
