import {
	DOCUMENT_TYPE_ENTITY_TYPE,
	DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
	DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
} from '../../index.js';
import { DOCUMENT_TYPE_REPOSITORY_ALIAS } from '../../repository/manifests.js';
import { UmbCreateDataTypeEntityAction } from './create.action.js';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DocumentType.Create',
		name: 'Create Document Type Entity Action',
		weight: 1000,
		meta: {
			icon: 'umb:add',
			label: 'Create...',
			repositoryAlias: DOCUMENT_TYPE_REPOSITORY_ALIAS,
			api: UmbCreateDataTypeEntityAction,
			entityTypes: [DOCUMENT_TYPE_ENTITY_TYPE, DOCUMENT_TYPE_ROOT_ENTITY_TYPE, DOCUMENT_TYPE_FOLDER_ENTITY_TYPE],
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.DocumentTypeCreateOptions',
		name: 'Document Type Create Options Modal',
		loader: () => import('./modal/document-type-create-options-modal.element.js'),
	},
];

export const manifests = [...entityActions];
