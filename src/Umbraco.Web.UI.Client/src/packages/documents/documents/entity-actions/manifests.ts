import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../entity.js';
import { UmbPublishDocumentEntityAction } from './publish.action.js';
import { UmbCreateDocumentBlueprintEntityAction } from './create-blueprint.action.js';
import { UmbUnpublishDocumentEntityAction } from './unpublish.action.js';
import { UmbRollbackDocumentEntityAction } from './rollback.action.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as permissionManifests } from './permissions/manifests.js';
import { manifests as publicAccessManifests } from './public-access/manifests.js';
import { manifests as cultureAndHostnamesManifests } from './culture-and-hostnames/manifests.js';
import {
	UmbCopyEntityAction,
	UmbMoveEntityAction,
	UmbSortChildrenOfEntityAction,
} from '@umbraco-cms/backoffice/entity-action';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	...createManifests,
	...permissionManifests,
	...publicAccessManifests,
	...cultureAndHostnamesManifests,
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.CreateBlueprint',
		name: 'Create Document Blueprint Entity Action',
		weight: 800,
		api: UmbCreateDocumentBlueprintEntityAction,
		meta: {
			icon: 'icon-blueprint',
			label: 'Create Document Blueprint (TBD)',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Move',
		name: 'Move Document Entity Action ',
		weight: 700,
		api: UmbMoveEntityAction,
		meta: {
			icon: 'icon-enter',
			label: 'Move (TBD)',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Copy',
		name: 'Copy Document Entity Action',
		weight: 600,
		api: UmbCopyEntityAction,
		meta: {
			icon: 'icon-documents',
			label: 'Copy (TBD)',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Sort',
		name: 'Sort Document Entity Action',
		weight: 500,
		api: UmbSortChildrenOfEntityAction,
		meta: {
			icon: 'icon-navigation-vertical',
			label: 'Sort (TBD)',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_DOCUMENT_ROOT_ENTITY_TYPE, UMB_DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Publish',
		name: 'Publish Document Entity Action',
		api: UmbPublishDocumentEntityAction,
		meta: {
			icon: 'icon-globe',
			label: 'Publish (TBD)',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Unpublish',
		name: 'Unpublish Document Entity Action',
		api: UmbUnpublishDocumentEntityAction,
		meta: {
			icon: 'icon-globe',
			label: 'Unpublish (TBD)',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Rollback',
		name: 'Rollback Document Entity Action',
		api: UmbRollbackDocumentEntityAction,
		meta: {
			icon: 'icon-undo',
			label: 'Rollback (TBD)',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
