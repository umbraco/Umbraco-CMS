import { DOCUMENT_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { DOCUMENT_ENTITY_TYPE, DOCUMENT_ROOT_ENTITY_TYPE } from '../index.js';
import { UmbPublishDocumentEntityAction } from './publish.action.js';
import { UmbDocumentCultureAndHostnamesEntityAction } from './culture-and-hostnames.action.js';
import { UmbCreateDocumentBlueprintEntityAction } from './create-blueprint.action.js';
import { UmbDocumentPublicAccessEntityAction } from './public-access.action.js';
import { UmbUnpublishDocumentEntityAction } from './unpublish.action.js';
import { UmbRollbackDocumentEntityAction } from './rollback.action.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as permissionManifests } from './permissions/manifests.js';
import {
	UmbCopyEntityAction,
	UmbMoveEntityAction,
	UmbSortChildrenOfEntityAction,
} from '@umbraco-cms/backoffice/entity-action';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	...createManifests,
	...permissionManifests,
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.CreateBlueprint',
		name: 'Create Document Blueprint Entity Action',
		weight: 800,
		api: UmbCreateDocumentBlueprintEntityAction,
		meta: {
			icon: 'umb:blueprint',
			label: 'Create Document Blueprint (TBD)',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Move',
		name: 'Move Document Entity Action ',
		weight: 700,
		api: UmbMoveEntityAction,
		meta: {
			icon: 'umb:enter',
			label: 'Move (TBD)',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Copy',
		name: 'Copy Document Entity Action',
		weight: 600,
		api: UmbCopyEntityAction,
		meta: {
			icon: 'umb:documents',
			label: 'Copy (TBD)',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Sort',
		name: 'Sort Document Entity Action',
		weight: 500,
		api: UmbSortChildrenOfEntityAction,
		meta: {
			icon: 'umb:navigation-vertical',
			label: 'Sort (TBD)',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			entityTypes: [DOCUMENT_ROOT_ENTITY_TYPE, DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.CultureAndHostnames',
		name: 'Culture And Hostnames Document Entity Action',
		weight: 400,
		api: UmbDocumentCultureAndHostnamesEntityAction,
		meta: {
			icon: 'umb:home',
			label: 'Culture And Hostnames (TBD)',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.PublicAccess',
		name: 'Document Permissions Entity Action',
		api: UmbDocumentPublicAccessEntityAction,
		meta: {
			icon: 'umb:lock',
			label: 'Public Access (TBD)',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Publish',
		name: 'Publish Document Entity Action',
		api: UmbPublishDocumentEntityAction,
		meta: {
			icon: 'umb:globe',
			label: 'Publish (TBD)',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Unpublish',
		name: 'Unpublish Document Entity Action',
		api: UmbUnpublishDocumentEntityAction,
		meta: {
			icon: 'umb:globe',
			label: 'Unpublish (TBD)',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Rollback',
		name: 'Rollback Document Entity Action',
		api: UmbRollbackDocumentEntityAction,
		meta: {
			icon: 'umb:undo',
			label: 'Rollback (TBD)',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
