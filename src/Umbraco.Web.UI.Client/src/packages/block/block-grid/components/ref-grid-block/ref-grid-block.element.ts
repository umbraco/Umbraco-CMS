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
				min-width: 20px; // Set to something, to overwrite UUI min width.
				height: 100%; // Help to fill out the whole layout entry.
				min-height: var(--uui-size-16);
				flex-flow: row wrap;
				background-color: var(--uui-color-surface);
			}

			.break {
				flex-basis: 100%;
				height: 0;
			}

			#open-part {
				min-height: var(
					--uui-size-layout-2
				); // We should not do this, but it is a quick fix for now to ensure that the top part of a block gets a minimum height.
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
