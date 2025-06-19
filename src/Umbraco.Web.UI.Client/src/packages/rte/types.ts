import type { UmbBlockValueType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockRteLayoutModel } from '@umbraco-cms/backoffice/block-rte';

export interface UmbPropertyEditorRteValueType {
	markup: string;
	blocks?: UmbBlockValueType<UmbBlockRteLayoutModel>;
}

/**
 * @deprecated Use `UmbPropertyEditorRteValueType` instead, will be removed in v.17.0.0
 */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbPropertyEditorUiValueType extends UmbPropertyEditorRteValueType {}
