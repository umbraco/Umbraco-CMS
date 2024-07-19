import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const contentTypeDesignEditorManifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.WorkspaceView.ContentTypeDesignEditor',
	matchKind: 'contentTypeDesignEditor',
	matchType: 'workspaceView',
	manifest: {
		type: 'workspaceView',
		kind: 'contentTypeDesignEditor',
		element: () => import('./content-type-design-editor.element.js'),
		weight: 1000,
		meta: {
			label: '#general_design',
			pathname: 'design',
			icon: 'icon-document-dashed-line',
		},
	},
};
