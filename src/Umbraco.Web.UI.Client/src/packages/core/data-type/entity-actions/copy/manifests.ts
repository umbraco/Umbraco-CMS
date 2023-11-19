import { UMB_DATA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { COPY_DATA_TYPE_REPOSITORY_ALIAS } from '../../repository/copy/manifests.js';
import { UmbCopyDataTypeEntityAction } from './copy.action.js';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.Copy',
		name: 'Copy Data Type Entity Action',
		weight: 900,
		api: UmbCopyDataTypeEntityAction,
		meta: {
			icon: 'icon-documents',
			label: 'Copy to...',
			repositoryAlias: COPY_DATA_TYPE_REPOSITORY_ALIAS,
			entityTypes: [UMB_DATA_TYPE_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
