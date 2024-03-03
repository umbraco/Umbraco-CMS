import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS, UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS } from '../repository/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { UmbMoveEntityAction, UmbDuplicateEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Move',
		name: 'Move Media Type Entity Action',
		weight: 400,
		api: UmbMoveEntityAction,
		meta: {
			icon: 'icon-enter',
			label: 'Move',
			repositoryAlias: UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Copy',
		name: 'Copy Media Type Entity Action',
		weight: 300,
		api: UmbDuplicateEntityAction,
		meta: {
			icon: 'icon-documents',
			label: 'Copy',
			repositoryAlias: UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.MediaType.Delete',
		name: 'Delete Media Type Entity Action',
		kind: 'delete',
		meta: {
			detailRepositoryAlias: UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS,
			entityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions, ...createManifests];
