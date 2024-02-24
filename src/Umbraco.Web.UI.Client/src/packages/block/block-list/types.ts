import type { UmbBlockTypeBaseModel } from '../block-type/index.js';
import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '@umbraco-cms/backoffice/block';

export const UMB_BLOCK_LIST_TYPE = 'block-list-type';

export interface UmbBlockListTypeModel extends UmbBlockTypeBaseModel {}
export interface UmbBlockListLayoutModel extends UmbBlockLayoutBaseModel {}

export interface UmbBlockListValueModel extends UmbBlockValueType<UmbBlockListLayoutModel> {}
