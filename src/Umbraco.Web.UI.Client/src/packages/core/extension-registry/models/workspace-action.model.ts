import type { ConditionTypes } from '../conditions/types.js';
import type { UUIInterfaceColor, UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { UmbWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface ManifestWorkspaceAction<MetaType extends MetaWorkspaceAction = MetaWorkspaceAction>
	extends ManifestElementAndApi<UmbControllerHostElement, UmbWorkspaceAction<MetaType>>,
		ManifestWithDynamicConditions<ConditionTypes> {
	type: 'workspaceAction';
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaWorkspaceAction {}

export interface ManifestWorkspaceActionDefaultKind extends ManifestWorkspaceAction<MetaWorkspaceActionDefaultKind> {
	type: 'workspaceAction';
	kind: 'default';
}

export interface MetaWorkspaceActionDefaultKind extends MetaWorkspaceAction {
	label?: string;
	look?: UUIInterfaceLook;
	color?: UUIInterfaceColor;
}
