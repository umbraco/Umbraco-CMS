import type { UmbBlockDataType, UmbBlockLayoutBaseModel } from "@umbraco-cms/backoffice/extension-registry";
export type { UmbBlockViewUrlsPropType, UmbBlockDataType, UmbBlockLayoutBaseModel } from "@umbraco-cms/backoffice/extension-registry";

export interface UmbBlockValueType<BlockLayoutType extends UmbBlockLayoutBaseModel> {
	layout: { [key: string]: Array<BlockLayoutType> | undefined };
	contentData: Array<UmbBlockDataType>;
	settingsData: Array<UmbBlockDataType>;
}
