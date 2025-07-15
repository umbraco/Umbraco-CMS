import { UUIRefNodeElement } from '@umbraco-cms/backoffice/external/uui';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-ref-grid-block
 */
@customElement('umb-ref-grid-block')
export class UmbRefGridBlockElement extends UUIRefNodeElement {
	override render() {
		return html`
			${super.render()}
			<div class="break"></div>
			<slot name="areas"></slot>
		`;
	}

	static override styles = [
		...UUIRefNodeElement.styles,
		css`
			:host {
				min-width: 20px; /* Set to something, to overwrite UUI min width. */
				height: 100%; /* Help to fill out the whole layout entry. */
				min-height: var(--uui-size-16);
				flex-flow: row wrap;
				background-color: var(--uui-color-surface);
			}

			.break {
				flex-basis: 100%;
				height: 0;
			}

			#open-part {
				display: flex;
				min-height: var(--uui-size-16);
				padding: calc(var(--uui-size-2) + 1px);
			}

			:host([unpublished]) #open-part {
				opacity: 0.6;
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
