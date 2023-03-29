import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbModalHandler } from '@umbraco-cms/backoffice/modal';

@customElement('umb-modal-element')
export class UmbModalBaseElement<UmbModalData extends object = object, UmbModalResult = unknown> extends UmbLitElement {
	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbModalData, UmbModalResult>;

	@property({ type: Object, attribute: false })
	data?: UmbModalData;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-element': UmbModalBaseElement;
	}
}
