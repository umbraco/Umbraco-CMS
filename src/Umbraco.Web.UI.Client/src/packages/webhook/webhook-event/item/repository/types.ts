import type { UmbWebhookEventEntityType } from '../../entity.js';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';

export interface UmbWebhookEventItemModel extends UmbItemModel {
	entityType: UmbWebhookEventEntityType;
	name: string;
	eventType: string;
	alias: string;
}
