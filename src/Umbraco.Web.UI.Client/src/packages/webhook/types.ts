import type { UmbWebhookEntityType } from './entity.js';

export interface UmbWebhookDetailModel {
	entityType: UmbWebhookEntityType;
	headers: Record<string, string>;
	unique: string;
	enabled: boolean;
	url: string;
	events: Array<UmbWebhookEventModel>;
	contentTypes: Array<string>;
}

export interface UmbWebhookEventModel {
	eventName: string;
	eventType: string;
	alias: string;
}
