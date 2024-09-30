import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '@umbraco-cms/backoffice/block';

export const UMB_BLOCK_RTE_TYPE = 'block-rte-type';
export const UMB_BLOCK_RTE = 'block-rte';
/**
 * The attribute where the block content key is stored.
 * @deprecated Use {@link UMB_DATA_CONTENT_KEY} instead
 */
export const UMB_DATA_CONTENT_UDI = 'data-content-udi';
/**
 * The attribute where the block content key is stored.
 */
export const UMB_DATA_CONTENT_KEY = 'data-content-key';

export interface UmbBlockRteTypeModel extends UmbBlockTypeBaseModel {
	displayInline: boolean;
}
export interface UmbBlockRteLayoutModel extends UmbBlockLayoutBaseModel {
	displayInline?: boolean;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockRteValueModel extends UmbBlockValueType<UmbBlockRteLayoutModel> {}
