import type { UmbPreviewOptionActionBase } from './preview-option-action-base.controller.js';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UUIInterfaceLook, UUIInterfaceColor } from '@umbraco-cms/backoffice/external/uui';

export interface ManifestPreviewOption<MetaType extends MetaPreviewOption = MetaPreviewOption>
	extends ManifestElementAndApi<UmbControllerHostElement, UmbPreviewOptionActionBase<MetaType>>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'previewOption';
	meta: MetaPreviewOption;
}

export interface MetaPreviewOption {
	icon?: string;
	label?: string;
	look?: UUIInterfaceLook;
	color?: UUIInterfaceColor;
}

export interface ManifestPreviewOptionDefaultKind extends ManifestPreviewOption {
	type: 'previewOption';
	kind: 'default';
}

export interface ManifestPreviewOptionUrlProviderKind extends ManifestPreviewOption {
	type: 'previewOption';
	kind: 'urlProvider';
	providerAlias: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbPreviewOption: ManifestPreviewOption | ManifestPreviewOptionDefaultKind | ManifestPreviewOptionUrlProviderKind;
	}
}
