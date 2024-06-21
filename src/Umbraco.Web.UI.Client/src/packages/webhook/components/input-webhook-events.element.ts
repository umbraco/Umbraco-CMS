import type { UmbWebhookEventModel } from '../types.js';
import { UMB_WEBHOOK_EVENTS_MODAL } from './webhook-events-modal/webhook-events-modal.token.js';
import { css, html, customElement, property, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import '@umbraco-cms/backoffice/culture';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-input-webhook-events')
export class UmbInputWebhookEventsElement extends UmbLitElement {
	@property({ attribute: false })
	public events: Array<UmbWebhookEventModel> = [];

	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (_instance) => {
			this._modalContext = _instance;
		});
	}

	async #openModal() {
		const modal = this._modalContext?.open(this, UMB_WEBHOOK_EVENTS_MODAL, {
			modal: { type: 'sidebar' },
			data: { events: this.events },
		});
		if (!modal) return;

		await modal.onSubmit();

		this.events = modal.getValue().events;

		modal.destroy();
		this.dispatchEvent(new UmbChangeEvent());
	}

	#removeEvent(alias: string) {
		this.events = this.events.filter((item) => item.alias !== alias);
		this.dispatchEvent(new UmbChangeEvent());
	}

	#renderEvents() {
		if (!this.events.length) return nothing;

		return html`
			${repeat(
				this.events,
				(item) => item.alias,
				(item) => html`
					<span>${item.eventName}</span>
					<uui-button
						label=${this.localize.term('general_remove')}
						@click=${() => this.#removeEvent(item.alias)}></uui-button>
				`,
			)}
		`;
	}

	override render() {
		return html`${this.#renderEvents()}
			<uui-button
				id="choose"
				look="placeholder"
				label=${this.localize.term('general_choose')}
				@click=${this.#openModal}></uui-button>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				grid-template-columns: 1fr auto;
				gap: var(--uui-size-space-2) var(--uui-size-space-2);
				align-items: center;
			}

			#choose {
				grid-column: -1 / 1;
			}
		`,
	];
}

export default UmbInputWebhookEventsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-webhook-events': UmbInputWebhookEventsElement;
	}
}
