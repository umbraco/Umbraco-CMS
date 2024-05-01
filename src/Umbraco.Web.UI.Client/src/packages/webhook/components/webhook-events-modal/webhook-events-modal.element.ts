import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbWebhookDetailRepository } from '../../repository/index.js';

@customElement('umb-webhook-events-modal')
export class UmbWebhookEventsModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext;

	#webhookRepository = new UmbWebhookDetailRepository(this);

	constructor() {
		super();

		const hello = this.#webhookRepository;
	}

	#onInput(event: InputEvent) {
		const input = event.target as HTMLInputElement;
		this.modalContext?.setValue(input.value);
	}

	render() {
		return html`<input type="text" @input=${this.#onInput} />
			<button @click=${() => this.modalContext?.submit()}>Submit</button> `;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbWebhookEventsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-events-modal': UmbWebhookEventsModalElement;
	}
}
