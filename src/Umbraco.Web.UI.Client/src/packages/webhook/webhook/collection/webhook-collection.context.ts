import type { UmbWebhookCollectionFilterModel, UmbWebhookCollectionItemModel } from './types.js';
import { UMB_EDIT_WEBHOOK_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';

export class UmbWebhookCollectionContext extends UmbDefaultCollectionContext<
	UmbWebhookCollectionItemModel,
	UmbWebhookCollectionFilterModel
> {
	override async requestItemHref(item: UmbWebhookCollectionItemModel): Promise<string | undefined> {
		return UMB_EDIT_WEBHOOK_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: item.unique });
	}
}

export { UmbWebhookCollectionContext as api };
