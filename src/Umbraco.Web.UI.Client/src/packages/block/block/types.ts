import type { ManifestBlockEditorCustomView } from './block-editor-custom-view.model.js';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
export type { ManifestBlockEditorCustomView } from './block-editor-custom-view.model.js';

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
	editContentPath?: string;
	editSettingsPath?: string;
	showContentEdit: boolean;
	showSettingsEdit: boolean;
}

export interface UmbBlockEditorCustomViewProperties<
	LayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
	BlockType extends UmbBlockTypeBaseModel = UmbBlockTypeBaseModel,
> {
	manifest?: ManifestBlockEditorCustomView;
	config?: Partial<UmbBlockEditorCustomViewConfiguration>;
	blockType?: BlockType;
	contentUdi?: string;
	label?: string;
	icon?: string;
	index?: number;
	layout?: LayoutType;
	content?: UmbBlockDataType;
	settings?: UmbBlockDataType;
	contentInvalid?: boolean;
	settingsInvalid?: boolean;
}

export interface UmbBlockEditorCustomViewElement<
	LayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
	BlockType extends UmbBlockTypeBaseModel = UmbBlockTypeBaseModel,
> extends UmbBlockEditorCustomViewProperties<LayoutType, BlockType>,
		HTMLElement {}

export interface UmbBlockValueType<BlockLayoutType extends UmbBlockLayoutBaseModel> {
	layout: { [key: string]: Array<BlockLayoutType> | undefined };
	contentData: Array<UmbBlockDataType>;
	settingsData: Array<UmbBlockDataType>;
}
