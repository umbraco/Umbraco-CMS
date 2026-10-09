import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.WorkspaceContext.Content.LanguageAccess',
		matchKind: 'contentLanguageAccess',
		matchType: 'workspaceContext',
		manifest: {
			type: 'workspaceContext',
			kind: 'contentLanguageAccess',
			api: () => import('./content-language-access.workspace.controller.js'),
		},
	},
];
