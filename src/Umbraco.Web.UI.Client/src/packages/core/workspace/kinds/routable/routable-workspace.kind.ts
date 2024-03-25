import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Workspace.Routable',
	matchKind: 'routable',
	matchType: 'workspace',
	manifest: {
		type: 'workspace',
		kind: 'routable',
		element: () => import('./routable-workspace.element.js'),
	},
};
