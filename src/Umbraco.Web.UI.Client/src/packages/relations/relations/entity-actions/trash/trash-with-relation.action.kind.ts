import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ENTITY_ACTION_TRASH_KIND_MANIFEST } from '@umbraco-cms/backoffice/recycle-bin';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.TrashWithRelation',
	matchKind: 'trashWithRelation',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_TRASH_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'trashWithRelation',
		api: () => import('./trash-with-relation.action.js'),
	},
};
