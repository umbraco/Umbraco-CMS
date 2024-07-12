import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '@umbraco-cms/backoffice/block';

export const UMB_BLOCK_LIST_TYPE = 'block-list-type';
export const UMB_BLOCK_LIST = 'block-list';

export interface UmbBlockListTypeModel extends UmbBlockTypeBaseModel {}
export interface UmbBlockListLayoutModel extends UmbBlockLayoutBaseModel {}

export interface UmbBlockListValueModel extends UmbBlockValueType<UmbBlockListLayoutModel> {}
