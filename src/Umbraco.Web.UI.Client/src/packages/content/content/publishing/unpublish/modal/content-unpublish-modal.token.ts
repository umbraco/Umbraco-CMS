import type { UmbContentUnpublishModalData, UmbContentUnpublishModalValue } from './types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_CONTENT_UNPUBLISH_MODAL_ALIAS = 'Umb.Modal.Content.Unpublish';

export const UMB_CONTENT_UNPUBLISH_MODAL = new UmbModalToken<
	UmbContentUnpublishModalData,
	UmbContentUnpublishModalValue
>(UMB_CONTENT_UNPUBLISH_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
	},
});
