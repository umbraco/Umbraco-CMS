import type { ConditionTypes } from '../conditions/types.js';
import type { UUIInterfaceColor, UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { UmbWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export interface ManifestWorkspaceAction
	extends ManifestElementAndApi<HTMLElement, UmbWorkspaceAction>,
		ManifestWithDynamicConditions<ConditionTypes> {
	type: 'workspaceAction';
	meta: MetaWorkspaceAction;
}

export interface MetaWorkspaceAction {
	label?: string; //TODO: Use or implement additional label-key
	look?: UUIInterfaceLook;
	color?: UUIInterfaceColor;
}
