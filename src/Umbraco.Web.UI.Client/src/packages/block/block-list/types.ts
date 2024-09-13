import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';

export const UMB_BLOCK_LIST_TYPE = 'block-list-type';
export const UMB_BLOCK_LIST = 'block-list';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockListTypeModel extends UmbBlockTypeBaseModel {}
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockListLayoutModel extends UmbBlockLayoutBaseModel {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockListValueModel extends UmbBlockValueType<UmbBlockListLayoutModel> {}
