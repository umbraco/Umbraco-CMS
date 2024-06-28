import type { ManifestBlockEditorCustomView } from "../index.js";

// Shared with the Property Editor
export interface UmbBlockLayoutBaseModel {
	contentUdi: string;
	settingsUdi?: string | null;
}

// Shared with the Property Editor
export interface UmbBlockDataType {
	udi: string;
	contentTypeKey: string;
	[key: string]: unknown;
}

export interface UmbBlockEditorCustomViewConfiguration {
	editContentPath: string;
	editSettingsPath: string;
	showEditBlock: boolean;
}

export interface UmbBlockViewUrlsPropType {
	editContent?: string;
	editSettings?: string;
}


export interface UmbBlockEditorCustomViewProperties<LayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel> {
	manifest?: ManifestBlockEditorCustomView;
	config?: UmbBlockEditorCustomViewConfiguration;
	urls?: UmbBlockViewUrlsPropType;
	contentUdi?: string;
	label?: string;
	icon?: string;
	index?: number;
	layout?: LayoutType;
	content?: UmbBlockDataType;
	settings?: UmbBlockDataType;
}

export interface UmbBlockEditorCustomViewElement<LayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel> extends UmbBlockEditorCustomViewProperties<LayoutType>, HTMLElement {

}
