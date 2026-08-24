import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ENTITY_ACTION_SORT_CHILDREN_OF_KIND_MANIFEST } from '@umbraco-cms/backoffice/tree';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.SortChildrenOfDocument',
	matchKind: 'sortChildrenOfDocument',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_SORT_CHILDREN_OF_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'sortChildrenOfDocument',
		api: () => import('./sort-children-of-document.action.js'),
	},
};
