import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceInfoApp.EntityReferences',
	matchKind: 'entityReferences',
	matchType: 'workspaceInfoApp',
	manifest: {
		type: 'workspaceInfoApp',
		kind: 'entityReferences',
		element: () => import('./entity-references-workspace-view-info.element.js'),
		weight: 90,
	},
};
