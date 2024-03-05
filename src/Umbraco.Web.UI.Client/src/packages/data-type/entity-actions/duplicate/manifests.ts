import { UMB_DATA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UMB_DATA_TYPE_PICKER_MODAL } from '../../modals/data-type-picker-modal.token.js';
import { UMB_DUPLICATE_DATA_TYPE_REPOSITORY_ALIAS } from '../../repository/duplicate/manifests.js';
import { UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'duplicate',
		alias: 'Umb.EntityAction.DataType.Duplicate',
		name: 'Duplicate Data Type Entity Action',
		forEntityTypes: [UMB_DATA_TYPE_ENTITY_TYPE],
		meta: {
			duplicateRepositoryAlias: UMB_DUPLICATE_DATA_TYPE_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS,
			pickerModal: UMB_DATA_TYPE_PICKER_MODAL,
		},
	},
];

export const manifests = [...entityActions];
