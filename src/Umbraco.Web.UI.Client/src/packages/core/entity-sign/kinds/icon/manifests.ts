import { UmbIconEntitySignApi } from './entity-sign-icon.api.js';
import { UmbEntitySignIconElement } from './entity-sign-icon.element.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.EntitySign.Icon',
		matchKind: 'icon',
		matchType: 'entitySign',
		manifest: {
			type: 'entitySign',
			kind: 'icon',
			element: UmbEntitySignIconElement,
			api: UmbIconEntitySignApi,
		},
	},
];
