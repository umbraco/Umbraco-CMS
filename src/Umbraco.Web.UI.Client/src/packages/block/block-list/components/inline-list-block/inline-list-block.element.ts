import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-inline-list-block
 */
@customElement('umb-inline-list-block')
export class UmbInlineListBlockElement extends UmbLitElement {
	render() {
		return 'inline mode..';
	}
}

export default UmbInlineListBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-inline-list-block': UmbInlineListBlockElement;
	}
}
