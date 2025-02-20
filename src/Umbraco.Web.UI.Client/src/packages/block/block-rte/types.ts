import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '@umbraco-cms/backoffice/block';

export interface UmbBlockRteTypeModel extends UmbBlockTypeBaseModel {
	displayInline: boolean;
}
export interface UmbBlockRteLayoutModel extends UmbBlockLayoutBaseModel {
	displayInline?: boolean;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockRteValueModel extends UmbBlockValueType<UmbBlockRteLayoutModel> {}
