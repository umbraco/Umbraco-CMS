import type { UmbWorkspaceAction, UmbWorkspaceActionDefaultKind } from '../types.js';
import type { UUIInterfaceColor, UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface ManifestWorkspaceAction<
	MetaType extends MetaWorkspaceAction = MetaWorkspaceAction,
	ApiType extends UmbWorkspaceAction<MetaType> = UmbWorkspaceAction<MetaType>,
> extends ManifestElementAndApi<UmbControllerHostElement, ApiType>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'workspaceAction';
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaWorkspaceAction {}

export interface ManifestWorkspaceActionDefaultKind
	extends ManifestWorkspaceAction<MetaWorkspaceActionDefaultKind, UmbWorkspaceActionDefaultKind> {
	type: 'workspaceAction';
	kind: 'default';
}

export interface MetaWorkspaceActionDefaultKind extends MetaWorkspaceAction {
	label?: string;
	look?: UUIInterfaceLook;
	color?: UUIInterfaceColor;
	href?: string;
	additionalOptions?: boolean;
}

declare global {
	interface UmbExtensionManifestMap {
		ManifestWorkspaceAction: ManifestWorkspaceAction;
		ManifestWorkspaceActionDefaultKind: ManifestWorkspaceActionDefaultKind;
	}
}
