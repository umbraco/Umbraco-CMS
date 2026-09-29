import UmbRoutableWorkspaceElement from './routable-workspace.element.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

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
