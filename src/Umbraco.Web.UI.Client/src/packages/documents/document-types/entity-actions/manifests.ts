import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../entity.js';
import { DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS, DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS } from '../repository/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as moveManifests } from './move-to/manifests.js';
import { UMB_DOCUMENT_TYPE_PICKER_MODAL } from '@umbraco-cms/backoffice/document-type';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.DocumentType.Delete',
		name: 'Delete Document-Type Entity Action',
		forEntityTypes: [UMB_DOCUMENT_TYPE_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'entityAction',
		kind: 'duplicate',
		alias: 'Umb.EntityAction.DocumentType.Duplicate',
		name: 'Duplicate Document Type Entity Action',
		forEntityTypes: [UMB_DOCUMENT_TYPE_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
			duplicateRepositoryAlias: DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
			pickerModal: UMB_DOCUMENT_TYPE_PICKER_MODAL,
		},
	},
];

export const manifests = [...entityActions, ...createManifests, ...moveManifests];
