import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UUIRefNodeElement } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-ref-list-block
 */
@customElement('umb-ref-list-block')
export class UmbRefListBlockElement extends UUIRefNodeElement {
	//
	connectedCallback(): void {
		super.connectedCallback();
		this.setAttribute('border', '');
	}
}

export default UmbRefListBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-list-block': UmbRefListBlockElement;
	}
}
