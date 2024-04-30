import { UMB_WEBHOOK_WORKSPACE_CONTEXT } from '../webhook-workspace.context-token.js';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import '@umbraco-cms/backoffice/culture';
import type { UmbWebhookDetailModel } from '@umbraco-cms/backoffice/webhook';

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

	render() {
		return html`
			<uui-box>
				<umb-property-layout label="Url" description="The url to call when the webhook is triggered.">
					<uui-input slot="editor"></uui-input>
				</umb-property-layout>
				<umb-property-layout label="Events" description="The events for which the webhook should be triggered.">
					<div slot="editor">IMPLEMENT</div>
				</umb-property-layout>
				<umb-property
					label="Content Type"
					read-only
					alias="contentElementTypeKey"
					property-editor-ui-alias="Umb.PropertyEditorUi.DocumentTypePicker"
					.config=${[{ alias: 'onlyPickElementTypes', value: true }]}></umb-property>
				<umb-property-layout label="Enabled" description="Is the webhook enabled?">
					<uui-toggle slot="editor"></uui-toggle>
				</umb-property-layout>
				<umb-property-layout label="Headers" description="Custom headers to include in the webhook request.">
					<uui-input slot="editor"></uui-input>
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
		`,
	];
}

export default UmbWebhookDetailsWorkspaceViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-details-workspace-view': UmbWebhookDetailsWorkspaceViewElement;
	}
}
