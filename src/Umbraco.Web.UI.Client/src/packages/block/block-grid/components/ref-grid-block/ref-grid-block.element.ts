import { UUIRefNodeElement } from '@umbraco-cms/backoffice/external/uui';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-ref-grid-block
 */
@customElement('umb-ref-grid-block')
export class UmbRefGridBlockElement extends UUIRefNodeElement {
	render() {
		return html`
			${super.render()}
			<div class="break"></div>
			<slot name="areas"></slot>
		`;
	}

	static styles = [
		...UUIRefNodeElement.styles,
		css`
			:host {
				min-height: var(--uui-size-16);
				flex-flow: row wrap;
			}

			.break {
				flex-basis: 100%;
				height: 0;
			}
		`,
	];
}

export default UmbRefGridBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-grid-block': UmbRefGridBlockElement;
	}
}
