import type { UmbBlockTypeBaseModel } from '../block-type/index.js';
import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '@umbraco-cms/backoffice/block';

export const UMB_BLOCK_RTE_TYPE = 'block-rte-type';

export interface UmbBlockRteTypeModel extends UmbBlockTypeBaseModel {
	displayInline: boolean;
}
export interface UmbBlockRteLayoutModel extends UmbBlockLayoutBaseModel {
	displayInline?: boolean;
}

export interface UmbBlockRteValueModel extends UmbBlockValueType<UmbBlockRteLayoutModel> {}
