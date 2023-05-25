import { DOCUMENT_REPOSITORY_ALIAS } from '../repository/manifests.js';
import { UmbCreateDocumentEntityAction } from './create/create.action.js';
import { UmbPublishDocumentEntityAction } from './publish.action.js';
import { UmbDocumentCultureAndHostnamesEntityAction } from './culture-and-hostnames.action.js';
import { UmbCreateDocumentBlueprintEntityAction } from './create-blueprint.action.js';
import { UmbDocumentPublicAccessEntityAction } from './public-access.action.js';
import { UmbDocumentPermissionsEntityAction } from './permissions.action.js';
import { UmbUnpublishDocumentEntityAction } from './unpublish.action.js';
import { UmbRollbackDocumentEntityAction } from './rollback.action.js';
import {
	UmbCopyEntityAction,
	UmbMoveEntityAction,
	UmbTrashEntityAction,
	UmbSortChildrenOfEntityAction,
} from '@umbraco-cms/backoffice/entity-action';
import { ManifestEntityAction, ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Create',
		name: 'Create Document Entity Action',
		weight: 1000,
		meta: {
			icon: 'umb:add',
			label: 'Create',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbCreateDocumentEntityAction,
		},
		conditions: {
			entityTypes: [DOCUMENT_ROOT_ENTITY_TYPE, DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Trash',
		name: 'Trash Document Entity Action',
		weight: 900,
		meta: {
			icon: 'umb:trash',
			label: 'Trash',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbTrashEntityAction,
		},
		conditions: {
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.CreateBlueprint',
		name: 'Create Document Blueprint Entity Action',
		weight: 800,
		meta: {
			icon: 'umb:blueprint',
			label: 'Create Content Template',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbCreateDocumentBlueprintEntityAction,
		},
		conditions: {
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Move',
		name: 'Move Document Entity Action',
		weight: 700,
		meta: {
			icon: 'umb:enter',
			label: 'Move',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbMoveEntityAction,
		},
		conditions: {
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Copy',
		name: 'Copy Document Entity Action',
		weight: 600,
		meta: {
			icon: 'umb:documents',
			label: 'Copy',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbCopyEntityAction,
		},
		conditions: {
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Sort',
		name: 'Sort Document Entity Action',
		weight: 500,
		meta: {
			icon: 'umb:navigation-vertical',
			label: 'Sort',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbSortChildrenOfEntityAction,
		},
		conditions: {
			entityTypes: [DOCUMENT_ROOT_ENTITY_TYPE, DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.CultureAndHostnames',
		name: 'Culture And Hostnames Document Entity Action',
		weight: 400,
		meta: {
			icon: 'umb:home',
			label: 'Culture And Hostnames',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbDocumentCultureAndHostnamesEntityAction,
		},
		conditions: {
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Permissions',
		name: 'Document Permissions Entity Action',
		meta: {
			icon: 'umb:vcard',
			label: 'Permissions',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbDocumentPermissionsEntityAction,
		},
		conditions: {
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.PublicAccess',
		name: 'Document Permissions Entity Action',
		meta: {
			icon: 'umb:lock',
			label: 'Public Access',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbDocumentPublicAccessEntityAction,
		},
		conditions: {
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Publish',
		name: 'Publish Document Entity Action',
		meta: {
			icon: 'umb:globe',
			label: 'Publish',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbPublishDocumentEntityAction,
		},
		conditions: {
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Unpublish',
		name: 'Unpublish Document Entity Action',
		meta: {
			icon: 'umb:globe',
			label: 'Unpublish',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbUnpublishDocumentEntityAction,
		},
		conditions: {
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Rollback',
		name: 'Rollback Document Entity Action',
		meta: {
			icon: 'umb:undo',
			label: 'Rollback',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbRollbackDocumentEntityAction,
		},
		conditions: {
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
];

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CreateDocument',
		name: 'Create Document Modal',
		loader: () => import('../../document-types/modals/allowed-document-types/allowed-document-types-modal.element.js'),
	},
];

export const manifests = [...entityActions, ...modals];
