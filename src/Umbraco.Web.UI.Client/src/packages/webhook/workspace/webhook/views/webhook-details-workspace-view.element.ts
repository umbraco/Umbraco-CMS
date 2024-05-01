import { UMB_WEBHOOK_WORKSPACE_CONTEXT } from '../webhook-workspace.context-token.js';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import '@umbraco-cms/backoffice/culture';
import type { UmbWebhookDetailModel } from '@umbraco-cms/backoffice/webhook';

import '../../../components/input-webhook-headers-view.element.js';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import UmbInputWebhookHeadersElement from '../../../components/input-webhook-headers-view.element.js';

@customElement('umb-webhook-details-workspace-view')
export class UmbWebhookDetailsWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	_webhook?: UmbWebhookDetailModel;

	@state()
	_isNew?: boolean;

	#webhookWorkspaceContext?: typeof UMB_WEBHOOK_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_WEBHOOK_WORKSPACE_CONTEXT, (instance) => {
			this.#webhookWorkspaceContext = instance;
			this.observe(this.#webhookWorkspaceContext.data, (webhook) => {
				this._webhook = webhook;
			});
			this.observe(this.#webhookWorkspaceContext.isNew, (isNew) => {
				this._isNew = isNew;
			});
		});
	}

	#onHeadersChange(event: UmbChangeEvent) {
		if (!this._webhook) return;

		const headers = (event.target as UmbInputWebhookHeadersElement).headers;
		this.#webhookWorkspaceContext?.setHeaders(headers);
	}

	render() {
		if (!this._webhook) return nothing;

		return html`
			<uui-box>
				<umb-property-layout label="Url" description="The url to call when the webhook is triggered.">
					<uui-input slot="editor"></uui-input>
				</umb-property-layout>
				<umb-property-layout label="Events" description="The events for which the webhook should be triggered.">
					<div slot="editor">IMPLEMENT</div>
				</umb-property-layout>
				<umb-property-layout label="Content Type" description="Only trigger the webhook for a specific content type.">
					<umb-input-document-type slot="editor" ?elementTypesOnly=${true}></umb-input-document-type>
				</umb-property-layout>
				<umb-property-layout label="Enabled" description="Is the webhook enabled?">
					<uui-toggle slot="editor"></uui-toggle>
				</umb-property-layout>
				<umb-property-layout label="Headers" description="Custom headers to include in the webhook request.">
					<umb-input-webhook-headers
						@change=${this.#onHeadersChange}
						.headers=${this._webhook.headers}
						slot="editor"></umb-input-webhook-headers>
				</umb-property-layout>
			</uui-box>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-space-6);
			}

			umb-property-layout:first-child {
				padding-top: 0;
			}
			umb-property-layout:last-child {
				padding-bottom: 0;
			}
		`,
	];
}

export default UmbWebhookDetailsWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-details-workspace-view': UmbWebhookDetailsWorkspaceViewElement;
	}
}
