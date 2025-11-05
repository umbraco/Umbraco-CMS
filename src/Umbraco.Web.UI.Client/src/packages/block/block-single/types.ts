import type { UmbBlockLayoutBaseModel, UmbBlockValueType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockSingleTypeModel extends UmbBlockTypeBaseModel {}
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockSingleLayoutModel extends UmbBlockLayoutBaseModel {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbBlockSingleValueModel extends UmbBlockValueType<UmbBlockSingleLayoutModel> {}
