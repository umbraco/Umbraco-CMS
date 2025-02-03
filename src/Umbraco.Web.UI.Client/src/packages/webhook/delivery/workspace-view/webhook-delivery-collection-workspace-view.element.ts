import { UMB_WEBHOOK_WORKSPACE_CONTEXT } from '../../webhook/constants.js';
import { UmbCollectionWorkspaceViewElement } from '@umbraco-cms/backoffice/collection';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-webhook-delivery-collection-workspace-view')
export class UmbWebhookDeliveryCollectionWorkspaceViewElement extends UmbCollectionWorkspaceViewElement {
	constructor() {
		super();

		this.consumeContext(UMB_WEBHOOK_WORKSPACE_CONTEXT, (instance) => {
			this.observe(instance.unique, (unique) => this.#setWebhookFilter(unique));
		});
	}

	#setWebhookFilter(unique: string | null) {
		this._filter = {
			webhook: unique ? { unique } : null,
		};
	}
}

export { UmbWebhookDeliveryCollectionWorkspaceViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-delivery-collection-workspace-view': UmbWebhookDeliveryCollectionWorkspaceViewElement;
	}
}
