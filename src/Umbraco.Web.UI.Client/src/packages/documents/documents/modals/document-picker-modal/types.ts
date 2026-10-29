import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import type { UmbTreePickerModalData, UmbTreePickerModalValue } from '@umbraco-cms/backoffice/tree';

// TODO (V21): Remove the deprecated members below and extend
// `Omit<UmbTreePickerModalData<UmbDocumentTreeItemModel>, 'treeAlias' | 'foldersOnly' | 'createAction'>` instead.
export interface UmbDocumentPickerModalData extends UmbTreePickerModalData<UmbDocumentTreeItemModel> {
	/**
	 * The tree to browse in place of the document tree.
	 * @deprecated The document picker always browses the document tree. Scheduled for removal in Umbraco 21.
	 */
	treeAlias?: string;

	/**
	 * Not applied by the document picker.
	 * @deprecated Documents have no folders. Scheduled for removal in Umbraco 21.
	 */
	foldersOnly?: boolean;

	/**
	 * Not applied by the document picker.
	 * @deprecated The document picker does not offer a create action. Scheduled for removal in Umbraco 21.
	 */
	createAction?: UmbTreePickerModalData<UmbDocumentTreeItemModel>['createAction'];
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentPickerModalValue extends UmbTreePickerModalValue {}
