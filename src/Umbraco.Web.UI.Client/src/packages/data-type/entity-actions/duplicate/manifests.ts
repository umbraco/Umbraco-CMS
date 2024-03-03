import { UMB_DATA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UMB_COPY_DATA_TYPE_REPOSITORY_ALIAS } from '../../repository/copy/manifests.js';
import { UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UMB_DATA_TYPE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.Duplicate',
		name: 'Duplicate Data Type Entity Action',
		kind: 'duplicate',
		meta: {
			entityTypes: [UMB_DATA_TYPE_ENTITY_TYPE],
			duplicateRepositoryAlias: UMB_COPY_DATA_TYPE_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS,
			pickerModalAlias: UMB_DATA_TYPE_PICKER_MODAL,
		},
	},
];

export const manifests = [...entityActions];
