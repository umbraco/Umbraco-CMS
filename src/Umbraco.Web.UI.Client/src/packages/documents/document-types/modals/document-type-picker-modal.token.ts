import { umbCreateDocumentTypeWorkspacePathGenerator } from '../paths.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbDocumentTypeTreeItemModel } from '@umbraco-cms/backoffice/document-type';
import {
	type UmbTreePickerModalValue,
	type UmbTreePickerModalData,
	UMB_TREE_PICKER_MODAL_ALIAS,
} from '@umbraco-cms/backoffice/tree';

/*export interface UmbDocumentTypePickerModalData
	extends UmbTreePickerModalData<UmbDocumentTypeTreeItemModel, typeof umbCreateDocumentTypeWorkspacePathGenerator> {}
*/
export type UmbDocumentTypePickerModalData = UmbTreePickerModalData<
	UmbDocumentTypeTreeItemModel,
	typeof umbCreateDocumentTypeWorkspacePathGenerator
>;

export interface UmbDocumentTypePickerModalValue extends UmbTreePickerModalValue {}

export const UMB_DOCUMENT_TYPE_PICKER_MODAL = new UmbModalToken<
	UmbDocumentTypePickerModalData,
	UmbDocumentTypePickerModalValue
>(UMB_TREE_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		treeAlias: 'Umb.Tree.DocumentType',
		createAction: {
			modalData: {
				entityType: 'document-type',
				preset: {},
			},
			additionalPathGenerator: umbCreateDocumentTypeWorkspacePathGenerator,
			additionalPathParams: {
				entityType: 'document-type',
				parentUnique: null,
				presetAlias: null,
			},
		},
	},
});
