import type { UmbWebhookEventEntityType } from './entity.js';

export interface UmbWebhookEventModel {
	entityType: UmbWebhookEventEntityType;
	unique: string;
	eventName: string;
	eventType: string;
	alias: string;
}
