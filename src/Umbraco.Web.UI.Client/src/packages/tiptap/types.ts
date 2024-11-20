import type { UmbBlockValueType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockRteLayoutModel } from '@umbraco-cms/backoffice/block-rte';

export type * from './extensions/types.js';

// TODO: Rename this type:
export interface UmbPropertyEditorUiValueType {
	markup: string;
	blocks: UmbBlockValueType<UmbBlockRteLayoutModel>;
}

export const UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS = 'Umbraco.RichText';

/**
 * The attribute where the block content key is stored.
 */
export const UMB_BLOCK_RTE_DATA_CONTENT_KEY = 'data-content-key';
