import type { UmbTreeItemModel, UmbTreeStartNode } from '../types.js';
import { UMB_TREE_PICKER_MODAL_ALIAS } from './constants.js';
import type { UmbPickerModalData, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';
import type { UmbWorkspaceModalData } from '@umbraco-cms/backoffice/workspace';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbPathPattern, UmbPathPatternParamsType } from '@umbraco-cms/backoffice/router';

export interface UmbTreePickerModalCreateActionData<PathPatternParamsType extends UmbPathPatternParamsType> {
	label: string;
	modalData: UmbWorkspaceModalData;
	modalToken?: UmbModalToken;
	extendWithPathPattern: UmbPathPattern;
	extendWithPathParams: PathPatternParamsType;
}

export interface UmbTreePickerModalData<
	TreeItemType = UmbTreeItemModel,
	PathPatternParamsType extends UmbPathPatternParamsType = UmbPathPatternParamsType,
> extends UmbPickerModalData<TreeItemType> {
	hideTreeRoot?: boolean;
	treeAlias?: string;
	// Consider if it makes sense to move this into the UmbPickerModalData interface, but for now this is a TreePicker feature. [NL]
	createAction?: UmbTreePickerModalCreateActionData<PathPatternParamsType>;
	startNode?: UmbTreeStartNode;
	foldersOnly?: boolean;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
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
