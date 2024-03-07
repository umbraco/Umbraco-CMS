import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const contentTypeDesignEditorManifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceView.ContentTypeDesignEditor',
	matchKind: 'contentTypeDesign',
	matchType: 'workspaceView',
	manifest: {
		type: 'workspaceView',
		kind: 'contentTypeDesign',
		element: () => import('./content-type-workspace-view-edit.element.js'),
		weight: 1000,
		meta: {
			label: 'Design',
			pathname: 'design',
			icon: 'icon-document-dashed-line',
		},
	},
};
