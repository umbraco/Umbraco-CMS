import type { UmbSearchResultModel } from '../../../types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbExamineFieldsViewerModalData = {
	name: string;
	searchResult: UmbSearchResultModel;
};

export type UmbExamineFieldsViewerModalValue = never;

export const UMB_EXAMINE_FIELDS_VIEWER_MODAL = new UmbModalToken<
	UmbExamineFieldsViewerModalData,
	UmbExamineFieldsViewerModalValue
>('Umb.Modal.Examine.FieldsViewer', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
