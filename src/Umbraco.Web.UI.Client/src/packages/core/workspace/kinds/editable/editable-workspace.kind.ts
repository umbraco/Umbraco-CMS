import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Workspace.Editable',
	matchKind: 'editable',
	matchType: 'workspace',
	manifest: {
		type: 'workspace',
		kind: 'editable',
		element: () => import('./editable-workspace.element.js'),
	},
};
