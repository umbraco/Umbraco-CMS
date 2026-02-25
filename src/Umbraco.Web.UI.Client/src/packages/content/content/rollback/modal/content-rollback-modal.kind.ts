import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Modal.Rollback',
	matchKind: 'rollback',
	matchType: 'modal',
	manifest: {
		type: 'modal',
		kind: 'rollback',
		element: () => import('./content-rollback-modal.element.js'),
		meta: {
			rollbackRepositoryAlias: '',
			detailRepositoryAlias: '',
		},
	},
};
