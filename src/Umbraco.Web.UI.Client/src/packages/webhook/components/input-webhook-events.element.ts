import { UMB_WEBHOOK_EVENTS_MODAL } from './webhook-events-modal/webhook-events-modal.token.js';
import { css, html, customElement, state, property, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import '@umbraco-cms/backoffice/culture';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

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

		console.log('Search modal closed', modal.getValue());
		modal.destroy();
	}

	render() {
		return html`<uui-button id="add" look="placeholder" label="Add" @click=${this.#openModal}></uui-button>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				grid-template-columns: 1fr 1fr auto;
				gap: var(--uui-size-space-2) var(--uui-size-space-2);
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
