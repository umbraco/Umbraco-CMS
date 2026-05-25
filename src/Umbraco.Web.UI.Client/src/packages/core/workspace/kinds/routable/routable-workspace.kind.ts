import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import UmbRoutableWorkspaceElement from './routable-workspace.element.js';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Workspace.Routable',
	matchKind: 'routable',
	matchType: 'workspace',
	manifest: {
		type: 'workspace',
		kind: 'routable',
		element: UmbRoutableWorkspaceElement,
	},
};
