import type { UmbWebhookEventModel } from '../types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbWebhookPickerModalData {
	events: Array<UmbWebhookEventModel>;
}

export interface UmbWebhookPickerModalValue {
	events: Array<UmbWebhookEventModel>;
}

export const UMB_WEBHOOK_EVENTS_MODAL = new UmbModalToken<UmbWebhookPickerModalData, UmbWebhookPickerModalValue>(
	'Umb.Modal.Webhook.Events',
);
