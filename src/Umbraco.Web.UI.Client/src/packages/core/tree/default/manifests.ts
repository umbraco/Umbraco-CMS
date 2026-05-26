import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDefaultTreeContext } from './default-tree.context.js';
import UmbDefaultTreeElement from './default-tree.element.js';

export const manifests: Array<UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.Tree.Default',
		matchKind: 'default',
		matchType: 'tree',
		manifest: {
			type: 'tree',
			api: UmbDefaultTreeContext,
			element: UmbDefaultTreeElement,
		},
	},
];
