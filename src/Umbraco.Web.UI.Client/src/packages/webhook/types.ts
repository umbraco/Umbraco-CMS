import type { UmbWebhookEntityType } from './entity.js';

export interface UmbWebhookDetailModel {
	entityType: UmbWebhookEntityType;
	unique: string;
	name: string;
	enabled: boolean;
	url: string;
	events: string[] | null;
	types: string[] | null;
}
