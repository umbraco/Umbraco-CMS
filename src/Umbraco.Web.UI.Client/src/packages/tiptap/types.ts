import type { UmbBlockValueType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockRteLayoutModel } from '@umbraco-cms/backoffice/block-rte';

export type * from './extensions/types.js';

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
export interface UmbPropertyEditorUiValueType {
	markup: string;
	blocks: UmbBlockValueType<UmbBlockRteLayoutModel>;
}
