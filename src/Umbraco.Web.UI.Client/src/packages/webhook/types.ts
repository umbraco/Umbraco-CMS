import type { UmbWebhookEntityType } from './entity.js';

export interface UmbWebhookDetailModel {
	entityType: UmbWebhookEntityType;
	headers: Record<string, string>;
	unique: string;
	enabled: boolean;
	url: string;
	events: Array<WebhookEventModel>;
	contentTypes: Array<string>;
}

export interface WebhookEventModel {
	eventName: string;
	eventType: string;
	alias: string;
}
