import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDefaultWorkspaceElement } from './default-workspace.element.js';
import { UmbDefaultWorkspaceContext } from './default-workspace.context.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.Workspace.Default',
		matchKind: 'default',
		matchType: 'workspace',
		manifest: {
			type: 'workspace',
			kind: 'default',
			element: UmbDefaultWorkspaceElement,
			api: UmbDefaultWorkspaceContext,
		},
	},
];
