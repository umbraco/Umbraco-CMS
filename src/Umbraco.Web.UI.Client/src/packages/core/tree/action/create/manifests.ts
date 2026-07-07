import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTreeCreateActionButtonElement } from './tree-create-action.element.js';
import { UmbTreeCreateActionApi } from './tree-create-action.api.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.TreeAction.Create',
		matchKind: 'create',
		matchType: 'treeAction',
		manifest: {
			type: 'treeAction',
			kind: 'create',
			api: UmbTreeCreateActionApi,
			element: UmbTreeCreateActionButtonElement,
			weight: 1200,
			meta: {
				label: '#actions_createFor',
			},
		},
	},
];
