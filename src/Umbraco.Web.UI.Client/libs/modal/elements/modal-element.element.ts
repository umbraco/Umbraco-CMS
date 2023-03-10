import { customElement, property } from 'lit/decorators.js';
import { UmbModalHandler } from '..';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-modal-element')
export class UmbModalBaseElement<UmbModalData = void, UmbModalResult = void> extends UmbLitElement {
	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbModalData, UmbModalResult>;

	@property({ type: Object, attribute: false })
	data?: UmbModalData;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-element': UmbModalBaseElement<unknown>;
	}
}
