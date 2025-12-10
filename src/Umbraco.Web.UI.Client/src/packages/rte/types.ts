import type { UmbBlockValueType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockRteLayoutModel } from '@umbraco-cms/backoffice/block-rte';

export interface UmbPropertyEditorRteValueType {
	markup: string;
	blocks?: UmbBlockValueType<UmbBlockRteLayoutModel>;
}
