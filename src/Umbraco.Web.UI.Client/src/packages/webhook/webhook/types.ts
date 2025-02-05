import type { UmbWebhookEntityType } from '../entity.js';
import type { UmbWebhookEventModel } from '../webhook-event/types.js';

export type * from './collection/types.js';

export interface UmbWebhookDetailModel {
	entityType: UmbWebhookEntityType;
	headers: Record<string, string>;
	unique: string;
	enabled: boolean;
	url: string;
	events: Array<UmbWebhookEventModel>;
	contentTypes: Array<string>;
}
