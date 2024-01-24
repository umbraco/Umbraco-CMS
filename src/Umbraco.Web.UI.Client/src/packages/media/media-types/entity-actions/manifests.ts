import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { UmbDeleteEntityAction, UmbMoveEntityAction, UmbCopyEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestEntityAction> = [
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
		api: UmbCopyEntityAction,
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
		weight: 200,
		api: UmbDeleteEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete',
			repositoryAlias: UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions, ...createManifests];
