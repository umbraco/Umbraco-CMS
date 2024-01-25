import { DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { manifests as createManifests } from './create/manifests.js';
import {
	UmbCopyEntityAction,
	UmbMoveEntityAction,
	UmbDeleteEntityAction,
	UmbSortChildrenOfEntityAction,
} from '@umbraco-cms/backoffice/entity-action';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const entityType = 'document-type';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DocumentType.Delete',
		name: 'Delete Document-Type Entity Action',
		weight: 900,
		api: UmbDeleteEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete',
			repositoryAlias: DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DocumentType.Move',
		name: 'Move Document-Type Entity Action',
		weight: 700,
		api: UmbMoveEntityAction,
		meta: {
			icon: 'icon-enter',
			label: 'Move',
			repositoryAlias: DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DocumentType.Copy',
		name: 'Copy Document-Type Entity Action',
		weight: 600,
		api: UmbCopyEntityAction,
		meta: {
			icon: 'icon-documents',
			label: 'Copy',
			repositoryAlias: DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DocumentType.Sort',
		name: 'Sort Document-Type Entity Action',
		weight: 500,
		api: UmbSortChildrenOfEntityAction,
		meta: {
			icon: 'icon-navigation-vertical',
			label: 'Sort',
			repositoryAlias: DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [entityType],
		},
	},
];

export const manifests = [...entityActions, ...createManifests];
