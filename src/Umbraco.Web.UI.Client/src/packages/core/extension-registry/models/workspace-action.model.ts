import type { ConditionTypes } from '../conditions/types.js';
import type { UUIInterfaceColor, UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { UmbWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface ManifestWorkspaceAction
	extends ManifestElementAndApi<UmbControllerHostElement, UmbWorkspaceAction>,
		ManifestWithDynamicConditions<ConditionTypes> {
	type: 'workspaceAction';
	meta: MetaWorkspaceAction;
}

export interface MetaWorkspaceAction {
	label?: string; //TODO: Use or implement additional label-key
	look?: UUIInterfaceLook;
	color?: UUIInterfaceColor;
}
