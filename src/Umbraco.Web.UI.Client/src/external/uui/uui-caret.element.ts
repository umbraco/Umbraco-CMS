import { defineElement } from '@umbraco-ui/uui';
import { css, html, LitElement, property } from '../lit';

/**
 *  DEPRECATED: Please use uui-symbol-expand or uui-symbol-sort. A caret that rotates on click. Color will be `currentColor`
 *  @deprecated since version 0.0.8
 *  @element uui-caret
 */
@defineElement('uui-caret')
export class UUICaretElement extends LitElement {
	/**
	 * Turns the arrow around.
	 * @type {boolean}
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	public open = false;

	constructor() {
		super();
		console.error('´uui-caret´ is deprecated, please use ´uui-symbol-expand´ or ´uui-symbol-sort´');
	}

	override render() {
		return html`<svg
			xmlns="http://www.w3.org/2000/svg"
			viewBox="0 0 24 24"
			fill="none"
			stroke="currentColor"
			stroke-width="3"
			stroke-linecap="round"
			stroke-linejoin="round">
			<path d="m4 9 8 8 8-8"></path>
		</svg>`;
	}

	static override styles = [
		css`
			:host {
				display: inline-block;
				width: 12px;
				vertical-align: middle;
			}

			svg {
				transform-origin: 50% 50%;
				transition: transform 100ms cubic-bezier(0.1, 0, 0.9, 1);
			}

			:host([open]) svg {
				transform: rotate(180deg);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'uui-caret': UUICaretElement;
	}
}
