import type { ConditionTypes } from '../conditions/types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestWorkspaceFooterApp
	extends ManifestElementAndApi<UmbControllerHostElement, any>,
		ManifestWithDynamicConditions<ConditionTypes> {
	type: 'workspaceFooterApp';
}

export interface ManifestWorkspaceFooterAppMenuBreadcrumbKind extends ManifestWorkspaceFooterApp {
	type: 'workspaceFooterApp';
	kind: 'menuBreadcrumb';
}

export interface ManifestWorkspaceFooterAppVariantMenuBreadcrumbKind extends ManifestWorkspaceFooterApp {
	type: 'workspaceFooterApp';
	kind: 'variantMenuBreadcrumb';
}
