import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbPeekErrorArgs } from '../../types.js';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbErrorViewerModalData extends UmbPeekErrorArgs {}

export type UmbErrorViewerModalValue = undefined;

export const UMB_ERROR_VIEWER_MODAL = new UmbModalToken<UmbErrorViewerModalData, UmbErrorViewerModalValue>(
	'Umb.Modal.ErrorViewer',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
	},
);
