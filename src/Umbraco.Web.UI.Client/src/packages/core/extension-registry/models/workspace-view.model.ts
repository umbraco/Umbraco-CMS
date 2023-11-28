import { ConditionTypes } from '../conditions/types.js';
import type { UmbWorkspaceViewElement } from '../interfaces/workspace-view-element.interface.js';
import type { ManifestWithDynamicConditions, ManifestWithView } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceView
	extends ManifestWithView<UmbWorkspaceViewElement>,
		ManifestWithDynamicConditions<ConditionTypes> {
	type: 'workspaceView';
}
