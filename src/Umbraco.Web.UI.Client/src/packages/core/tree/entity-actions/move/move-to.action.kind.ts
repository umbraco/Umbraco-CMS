import UmbMoveToEntityAction from './move-to.action.js';
import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_GROUP_STRUCTURE } from '@umbraco-cms/backoffice/action';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.MoveTo',
	matchKind: 'moveTo',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'moveTo',
		group: UMB_ACTION_GROUP_STRUCTURE,
		api: UmbMoveToEntityAction,
		weight: 700,
		forEntityTypes: [],
		meta: {
			icon: 'icon-enter',
			label: '#actions_move',
			additionalOptions: true,
			treeRepositoryAlias: '',
			moveRepositoryAlias: '',
			treeAlias: '',
		},
	},
};
