import type { UmbPeekErrorArgs } from '../../../notification/types.js';
import { UmbModalToken } from '../../token/index.js';

export type UmbErrorViewerModalData = UmbPeekErrorArgs | Record<string, string[]> | string;

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
