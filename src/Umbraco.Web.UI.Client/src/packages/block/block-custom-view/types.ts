import type { ManifestBlockEditorCustomView } from './block-editor-custom-view.extension.js';
import type { UmbBlockLayoutBaseModel, UmbBlockDataType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';

export type * from './block-editor-custom-view.extension.js';

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
	config?: UmbBlockEditorCustomViewConfiguration;
	blockType?: BlockType;
	contentKey?: string;
	label?: string;
	icon?: string;
	index?: number;
	layout?: LayoutType;
	content?: UmbBlockDataType;
	settings?: UmbBlockDataType;
	contentInvalid?: boolean;
	settingsInvalid?: boolean;
	unsupported?: boolean;
	unpublished?: boolean;
}

export interface UmbBlockEditorCustomViewElement<
	LayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
	BlockType extends UmbBlockTypeBaseModel = UmbBlockTypeBaseModel,
> extends UmbBlockEditorCustomViewProperties<LayoutType, BlockType>,
		HTMLElement {}
