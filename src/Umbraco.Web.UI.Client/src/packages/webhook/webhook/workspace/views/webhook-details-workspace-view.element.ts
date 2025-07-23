import { UMB_WEBHOOK_WORKSPACE_CONTEXT } from '../webhook-workspace.context-token.js';
import type { UmbInputWebhookHeadersElement } from '../../../components/input-webhook-headers.element.js';
import type { UmbInputWebhookEventsElement } from '../../../webhook-event/input-webhook-events.element.js';
import type { UmbWebhookDetailModel } from '../../types.js';
import { css, customElement, html, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbInputDocumentTypeElement } from '@umbraco-cms/backoffice/document-type';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import type { UUIBooleanInputEvent, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

import '@umbraco-cms/backoffice/culture';
import '../../../components/input-webhook-headers.element.js';
import '../../../webhook-event/input-webhook-events.element.js';

@customElement('umb-webhook-details-workspace-view')
export class UmbWebhookDetailsWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _webhook?: UmbWebhookDetailModel;

	@state()
	private _contentType?: string;

	#webhookWorkspaceContext?: typeof UMB_WEBHOOK_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_WEBHOOK_WORKSPACE_CONTEXT, (instance) => {
			this.#webhookWorkspaceContext = instance;
			this.observe(this.#webhookWorkspaceContext?.data, (webhook) => {
				this._webhook = webhook;
				this._contentType = this._webhook?.events[0]?.eventType ?? undefined;
			});
		});
	}

	#onEventsChange(event: UmbChangeEvent & { target: UmbInputWebhookEventsElement }) {
		const events = event.target.events ?? [];
		if (events.length && events[0].eventType !== this._contentType) {
			this.#webhookWorkspaceContext?.setTypes([]);
		}
		this.#webhookWorkspaceContext?.setEvents(events);
	}

	#onHeadersChange(event: UmbChangeEvent) {
		const headers = (event.target as UmbInputWebhookHeadersElement).headers;
		this.#webhookWorkspaceContext?.setHeaders(headers);
	}

	#onTypesChange(event: UmbChangeEvent) {
		const types = (event.target as UmbInputDocumentTypeElement).selection;
		this.#webhookWorkspaceContext?.setTypes(types);
	}

	#onUrlChange(event: UUIInputEvent) {
		const value = event.target.value;
		if (typeof value !== 'string') return;

		this.#webhookWorkspaceContext?.setUrl(value);
	}

	#onEnabledChange(event: UUIBooleanInputEvent) {
		this.#webhookWorkspaceContext?.setEnabled(event.target.checked);
	}
	#renderContentTypePicker() {
		if (this._contentType !== 'Content' && this._contentType !== 'Media') return nothing;

		return html`
			<umb-property-layout
				label=${this.localize.term('webhooks_contentType')}
				description=${this.localize.term('webhooks_contentTypeDescription')}>
				${this.#renderContentTypePickerEditor()}
			</umb-property-layout>
		`;
	}

	#renderContentTypePickerEditor() {
		switch (this._contentType) {
			case 'Content':
				return html`
					<umb-input-document-type
						slot="editor"
						@change=${this.#onTypesChange}
						.selection=${this._webhook?.contentTypes ?? []}
						.documentTypesOnly=${true}></umb-input-document-type>
				`;
			case 'Media':
				return html`
					<umb-input-media-type
						slot="editor"
						@change=${this.#onTypesChange}
						.selection=${this._webhook?.contentTypes ?? []}></umb-input-media-type>
				`;
			default:
				return nothing;
		}
	}

	override render() {
		if (!this._webhook) return nothing;

		return html`
			<uui-box>
				<umb-property-layout
					mandatory
					label=${this.localize.term('webhooks_url')}
					description=${this.localize.term('webhooks_urlDescription')}>
					<uui-input @input=${this.#onUrlChange} .value=${this._webhook.url} slot="editor" required="true"></uui-input>
				</umb-property-layout>
				<umb-property-layout
					label=${this.localize.term('webhooks_events')}
					description=${this.localize.term('webhooks_eventDescription')}>
					<umb-input-webhook-events
						@change=${this.#onEventsChange}
						.events=${this._webhook.events ?? []}
						slot="editor"></umb-input-webhook-events>
				</umb-property-layout>
				${this.#renderContentTypePicker()}
				<umb-property-layout
					label=${this.localize.term('webhooks_enabled')}
					description=${this.localize.term('webhooks_enabledDescription')}>
					<uui-toggle slot="editor" .checked=${this._webhook.enabled} @change=${this.#onEnabledChange}></uui-toggle>
				</umb-property-layout>
				<umb-property-layout
					label=${this.localize.term('webhooks_headers')}
					description=${this.localize.term('webhooks_headersDescription')}>
					<umb-input-webhook-headers
						@change=${this.#onHeadersChange}
						.headers=${this._webhook.headers}
						slot="editor"></umb-input-webhook-headers>
				</umb-property-layout>
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-space-6);
			}

			uui-input {
				width: 100%;
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
