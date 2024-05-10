import type { SearchResultResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbExamineFieldsViewerModalData = {
	name: string;
	searchResult: SearchResultResponseModel;
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
