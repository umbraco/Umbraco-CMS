import { LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbModalHandler } from '../../modal';

@customElement('umb-modal-layout')
export class UmbModalLayoutElement<UmbModalData = void> extends LitElement {
	@property({ attribute: false })
	modalHandler?: UmbModalHandler;

	@property({ type: Object })
	data?: UmbModalData;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout': UmbModalLayoutElement<unknown>;
	}
}
