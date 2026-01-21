import type { UmbTreeItemModel, UmbTreeStartNode } from '../types.js';
import type { UmbPathPattern, UmbPathPatternParamsType } from '@umbraco-cms/backoffice/router';
import type { UmbModalToken, UmbPickerModalData, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';
import type { UmbWorkspaceModalData } from '@umbraco-cms/backoffice/workspace';

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
	expandTreeRoot?: boolean;
	treeAlias?: string;
	// TODO: create action should be replaces by entity actions in the pickers. Then we also open up for creating folders, choosing where to place items etc. [MR]
	createAction?: UmbTreePickerModalCreateActionData<PathPatternParamsType>;
	startNode?: UmbTreeStartNode;
	foldersOnly?: boolean;
	isVariant?: boolean;
}

export interface UmbTreePickerModalValue extends UmbPickerModalValue {
	culture?: string;
}
