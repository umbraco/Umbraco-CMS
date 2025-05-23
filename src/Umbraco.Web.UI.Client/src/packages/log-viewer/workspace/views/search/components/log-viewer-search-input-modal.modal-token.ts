import type { SavedLogSearchResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbContextSaveSearchModalData {
	query: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbContextSaveSearchModalValue extends SavedLogSearchResponseModel {}

export const UMB_LOG_VIEWER_SAVE_SEARCH_MODAL = new UmbModalToken<
	UmbContextSaveSearchModalData,
	UmbContextSaveSearchModalValue
>('Umb.Modal.LogViewer.SaveSearch', {
	modal: {
		type: 'dialog',
		size: 'small',
	},
});
