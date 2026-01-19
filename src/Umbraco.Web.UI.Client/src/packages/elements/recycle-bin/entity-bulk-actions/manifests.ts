import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_ITEM_REPOSITORY_ALIAS } from '../../item/constants.js';
import { UMB_ELEMENT_RECYCLE_BIN_REPOSITORY_ALIAS } from '../repository/constants.js';
import { UMB_ELEMENT_REFERENCE_REPOSITORY_ALIAS } from '../../reference/constants.js';
import { UMB_ELEMENT_COLLECTION_ALIAS } from '../../collection/constants.js';
import {
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_DELETE,
} from '../../user-permissions/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';
import { UMB_ENTITY_BULK_ACTION_TRASH_WITH_RELATION_KIND } from '@umbraco-cms/backoffice/relations';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import type { ManifestEntityBulkActionTrashWithRelationKind } from '@umbraco-cms/backoffice/relations';

const trash: ManifestEntityBulkActionTrashWithRelationKind = {
	type: 'entityBulkAction',
	kind: UMB_ENTITY_BULK_ACTION_TRASH_WITH_RELATION_KIND,
	alias: 'Umb.EntityBulkAction.Element.Trash',
	name: 'Trash Element Entity Bulk Action',
	weight: 10,
	forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
	meta: {
		itemRepositoryAlias: UMB_ELEMENT_ITEM_REPOSITORY_ALIAS,
		recycleBinRepositoryAlias: UMB_ELEMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
		referenceRepositoryAlias: UMB_ELEMENT_REFERENCE_REPOSITORY_ALIAS,
	},
	conditions: [
		{
			alias: UMB_COLLECTION_ALIAS_CONDITION,
			match: UMB_ELEMENT_COLLECTION_ALIAS,
		},
		{
			alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
			allOf: [UMB_USER_PERMISSION_ELEMENT_DELETE],
		},
		{
			alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [trash];
