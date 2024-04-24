import { UMB_TREE_PICKER_MODAL_ALIAS } from './constants.js';
import type { UmbPickerModalData, UmbPickerModalValue, UmbWorkspaceModalData } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbPathGeneratorType = (params: any) => string;
export interface UmbTreePickerModalCreateActionData<PathGeneratorType extends UmbPathGeneratorType> {
	modalData: UmbWorkspaceModalData;
	modalToken?: UmbModalToken;
	additionalPathGenerator: PathGeneratorType;
	additionalPathParams: Parameters<PathGeneratorType>[0];
}

export interface UmbTreePickerModalData<
	TreeItemType = any,
	PathGeneratorType extends UmbPathGeneratorType = UmbPathGeneratorType,
> extends UmbPickerModalData<TreeItemType> {
	treeAlias?: string;
	// Consider if it makes sense to move this into the UmbPickerModalData interface, but for now this is a TreePicker feature. [NL]
	createAction?: UmbTreePickerModalCreateActionData<PathGeneratorType>;
}

export interface UmbTreePickerModalValue extends UmbPickerModalValue {}

export const UMB_TREE_PICKER_MODAL = new UmbModalToken<UmbTreePickerModalData, UmbTreePickerModalValue>(
	UMB_TREE_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
