import { DOCUMENT_TYPE_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { manifests as createManifests } from './create/manifests.js';
import {
	UmbCopyEntityAction,
	UmbMoveEntityAction,
	UmbTrashEntityAction,
	UmbSortChildrenOfEntityAction,
} from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const entityType = 'document-type';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DocumentType.Trash',
		name: 'Trash Document-Type Entity Action',
		weight: 900,
		meta: {
			icon: 'umb:trash',
			label: 'Trash',
			repositoryAlias: DOCUMENT_TYPE_REPOSITORY_ALIAS,
			api: UmbTrashEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DocumentType.Move',
		name: 'Move Document-Type Entity Action',
		weight: 700,
		meta: {
			icon: 'umb:enter',
			label: 'Move',
			repositoryAlias: DOCUMENT_TYPE_REPOSITORY_ALIAS,
			api: UmbMoveEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DocumentType.Copy',
		name: 'Copy Document-Type Entity Action',
		weight: 600,
		meta: {
			icon: 'umb:documents',
			label: 'Copy',
			repositoryAlias: DOCUMENT_TYPE_REPOSITORY_ALIAS,
			api: UmbCopyEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DocumentType.Sort',
		name: 'Sort Document-Type Entity Action',
		weight: 500,
		meta: {
			icon: 'umb:navigation-vertical',
			label: 'Sort',
			repositoryAlias: DOCUMENT_TYPE_REPOSITORY_ALIAS,
			api: UmbSortChildrenOfEntityAction,
		},
		conditions: {
			entityTypes: [entityType],
		},
	},
];

export const manifests = [...entityActions, ...createManifests];
