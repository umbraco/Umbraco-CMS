import type { UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';
import type { UmbWebhookEventModel } from '../../webhook-event/types.js';
import type { UmbWebhookEntityType } from '../../entity.js';

export interface UmbWebhookCollectionItemModel extends UmbCollectionItemModel {
	entityType: UmbWebhookEntityType;
	name: string;
	enabled: boolean;
	url: string;
	description: string | null | undefined;
	headers: Record<string, string>;
	events: Array<UmbWebhookEventModel>;
	contentTypes: Array<string>;
}

export interface UmbWebhookCollectionFilterModel {
	skip?: number;
	take?: number;
}
