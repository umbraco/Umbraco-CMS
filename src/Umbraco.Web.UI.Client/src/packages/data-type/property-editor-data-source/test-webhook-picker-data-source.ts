import type { UmbPickerPropertyEditorCollectionDataSource } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbWebhookCollectionRepository, UmbWebhookItemRepository } from '@umbraco-cms/backoffice/webhook';

export class UmbWebhookPickerPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerPropertyEditorCollectionDataSource
{
	collection = new UmbWebhookCollectionRepository(this);
	item = new UmbWebhookItemRepository(this);
}

export { UmbWebhookPickerPropertyEditorDataSource as api };
