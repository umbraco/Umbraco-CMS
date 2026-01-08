import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ENTITY_ACTION_DELETE_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.DeleteWithRelation',
	matchKind: 'deleteWithRelation',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DELETE_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'deleteWithRelation',
		api: () => import('./delete-with-relation.action.js'),
	},
};
