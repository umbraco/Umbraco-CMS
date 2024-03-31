import { DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS, DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS } from '../repository/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { UMB_DOCUMENT_TYPE_PICKER_MODAL } from '@umbraco-cms/backoffice/document-type';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityType = 'document-type';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.DocumentType.Delete',
		name: 'Delete Document-Type Entity Action',
		forEntityTypes: [entityType],
		meta: {
			itemRepositoryAlias: DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		kind: 'moveTo',
		alias: 'Umb.EntityAction.DocumentType.MoveTo',
		name: 'Move Document Type Entity Action',
		forEntityTypes: [entityType],
		meta: {
			itemRepositoryAlias: DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
			moveRepositoryAlias: DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
			pickerModal: UMB_DOCUMENT_TYPE_PICKER_MODAL,
		},
	},
	{
		type: 'entityAction',
		kind: 'duplicate',
		alias: 'Umb.EntityAction.DocumentType.Duplicate',
		name: 'Duplicate Document Type Entity Action',
		forEntityTypes: [entityType],
		meta: {
			itemRepositoryAlias: DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
			duplicateRepositoryAlias: DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
			pickerModal: UMB_DOCUMENT_TYPE_PICKER_MODAL,
		},
	},
];

export const manifests = [...entityActions, ...createManifests];
