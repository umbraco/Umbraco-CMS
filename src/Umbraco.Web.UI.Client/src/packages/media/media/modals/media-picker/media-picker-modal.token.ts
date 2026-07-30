import type { UmbMediaTreeItemModel } from '../../tree/types.js';
import type { UmbMediaClipboardPasteConfig } from '../../clipboard/types.js';
import type { UmbTreePickerModalData } from '@umbraco-cms/backoffice/tree';
import { UmbModalToken, type UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';

export type UmbMediaPickerModalData = UmbTreePickerModalData<UmbMediaTreeItemModel> & {
	/**
	 * The clipboard tab, letting the user paste an entry instead of browsing for media. Supply this only when the
	 * opener can act on a picked entry.
	 */
	clipboard?: UmbMediaClipboardPasteConfig;
};

export type UmbMediaPickerModalValue = UmbPickerModalValue & {
	/**
	 * Entries picked from the clipboard tab. These are clipboard entry uniques, not media uniques — the opener
	 * translates them into its own property value shape.
	 */
	clipboard?: {
		selection: Array<string>;
	};
};

export const UMB_MEDIA_PICKER_MODAL = new UmbModalToken<UmbMediaPickerModalData, UmbMediaPickerModalValue>(
	'Umb.Modal.MediaPicker',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
	},
);
