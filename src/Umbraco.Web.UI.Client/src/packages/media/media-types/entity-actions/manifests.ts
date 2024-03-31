import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS, UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS } from '../repository/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { UMB_MEDIA_TREE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'moveTo',
		alias: 'Umb.EntityAction.MediaType.MoveTo',
		name: 'Move Media Type Entity Action',
		forEntityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE],
		meta: {
			moveRepositoryAlias: UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS,
			pickerModal: UMB_MEDIA_TREE_PICKER_MODAL,
		},
	},
	{
		type: 'entityAction',
		kind: 'duplicate',
		alias: 'Umb.EntityAction.MediaType.Duplicate',
		name: 'Duplicate Media Type Entity Action',
		forEntityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE],
		meta: {
			duplicateRepositoryAlias: UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS,
			pickerModal: UMB_MEDIA_TREE_PICKER_MODAL,
		},
	},
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.MediaType.Delete',
		name: 'Delete Media Type Entity Action',
		forEntityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS,
		},
	},
];

export const manifests = [...entityActions, ...createManifests];
