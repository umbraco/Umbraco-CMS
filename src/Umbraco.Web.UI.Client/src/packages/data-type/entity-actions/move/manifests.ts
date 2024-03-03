import { UMB_DATA_TYPE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { UMB_DATA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UMB_MOVE_DATA_TYPE_REPOSITORY_ALIAS } from '../../repository/move/manifests.js';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.Move',
		name: 'Move Data Type Entity Action',
		kind: 'move',
		meta: {
			entityTypes: [UMB_DATA_TYPE_ENTITY_TYPE],
			itemRepositoryAlias: UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS,
			moveRepositoryAlias: UMB_MOVE_DATA_TYPE_REPOSITORY_ALIAS,
			pickerModalAlias: UMB_DATA_TYPE_PICKER_MODAL.toString(),
		},
	},
];

export const manifests = [...entityActions];
