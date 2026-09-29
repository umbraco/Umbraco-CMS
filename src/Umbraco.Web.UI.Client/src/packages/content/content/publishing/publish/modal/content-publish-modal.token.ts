import type { UmbContentPublishModalData, UmbContentPublishModalValue } from './types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_CONTENT_PUBLISH_MODAL_ALIAS = 'Umb.Modal.Content.Publish';

export const UMB_CONTENT_PUBLISH_MODAL = new UmbModalToken<UmbContentPublishModalData, UmbContentPublishModalValue>(
	UMB_CONTENT_PUBLISH_MODAL_ALIAS,
	{
		modal: {
			type: 'dialog',
		},
	},
);
