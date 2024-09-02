import type { UmbBlockDataModel, UmbBlockLayoutBaseModel } from '@umbraco-cms/backoffice/extension-registry';
export type {
	UmbBlockDataModel as UmbBlockDataType,
	UmbBlockLayoutBaseModel,
} from '@umbraco-cms/backoffice/extension-registry';

export interface UmbBlockValueType<BlockLayoutType extends UmbBlockLayoutBaseModel> {
	layout: { [key: string]: Array<BlockLayoutType> | undefined };
	contentData: Array<UmbBlockDataModel>;
	settingsData: Array<UmbBlockDataModel>;
}
