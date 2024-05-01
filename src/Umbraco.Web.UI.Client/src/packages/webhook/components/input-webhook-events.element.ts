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
	public events: Array<string> = [];

	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (_instance) => {
			this._modalContext = _instance;
		});
	}

	async #openModal() {
		const modal = this._modalContext?.open(this, UMB_WEBHOOK_EVENTS_MODAL, { modal: { type: 'sidebar' } });
		if (!modal) return;

		await modal.onSubmit();
		this.events = modal.getValue() as Array<string>;

		modal.destroy();
		this.dispatchEvent(new UmbChangeEvent());
	}

	#removeEvent(index: number) {
		this.events = this.events.filter((_, i) => i !== index);
		this.dispatchEvent(new UmbChangeEvent());
	}

	#renderEvents() {
		if (!this.events.length) return nothing;

		return html`
			${repeat(
				this.events,
				(item) => item,
				(item, index) => html`
					<span>${item}</span>
					<uui-button @click=${() => this.#removeEvent(index)} label="remove"></uui-button>
				`,
			)}
		`;
	}

	render() {
		return html`${this.#renderEvents()}
			<uui-button id="add" look="placeholder" label="Add" @click=${this.#openModal}></uui-button>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				grid-template-columns: 1fr auto;
				gap: var(--uui-size-space-2) var(--uui-size-space-2);
				align-items: center;
			}

			#add {
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
