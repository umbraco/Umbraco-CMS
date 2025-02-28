import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestContextualUserPermission<
	MetaType extends MetaContextualUserPermission = MetaContextualUserPermission,
> extends ManifestElement {
	type: 'contextualUserPermission';
	meta: MetaType;
}

export interface MetaContextualUserPermission {
	label: string;
	description?: string;
	group: string;
	permission: {
		context: string;
		verbs: Array<string>;
	};
}

declare global {
	interface UmbExtensionManifestMap {
		umbContextualUserPermission: ManifestContextualUserPermission;
	}
}
