import { UmbModalToken } from '../../token/index.js';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbErrorViewerModalData extends Record<string, unknown> {}

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
