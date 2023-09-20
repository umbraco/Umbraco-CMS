import { DOCUMENT_REPOSITORY_ALIAS } from '../../repository/manifests.js';
import { UmbCreateDocumentEntityAction } from './create.action.js';
import { ManifestEntityAction, ManifestModal } from '@umbraco-cms/backoffice/extension-registry';
import { DOCUMENT_ENTITY_TYPE, DOCUMENT_ROOT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';

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
			entityTypes: [DOCUMENT_ROOT_ENTITY_TYPE, DOCUMENT_ENTITY_TYPE],
		},
	},
];

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CreateDocument',
		name: 'Create Document Modal',
		loader: () => import('./create-document-modal.element.js'),
	},
];

export const manifests = [...entityActions, ...modals];
