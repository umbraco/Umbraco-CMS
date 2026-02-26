import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Modal.ContentRollback',
	matchKind: 'contentRollback',
	matchType: 'modal',
	manifest: {
		type: 'modal',
		kind: 'contentRollback',
		element: () => import('./content-rollback-modal.element.js'),
		meta: {
			rollbackRepositoryAlias: '',
			detailRepositoryAlias: '',
		},
	},
};
