import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { classMap, customElement, css, html, property } from '@umbraco-cms/backoffice/external/lit';

/**
 *  @element umb-stack
 *  @description - Element for displaying items in a stack with even spacing between
 *  @augments LitElement
 */
@customElement('umb-stack')
export class UmbStackElement extends UmbLitElement {
	/**
	 * Look
	 * @type {string}
	 * @memberof UmbStackElement
	 */
	@property({ type: String })
	look: 'compact' | 'default' = 'default';

	/**
	 * Divide
	 * @type {boolean}
	 * @memberof UmbStackElement
	 */
	@property({ type: Boolean })
	divide: boolean = false;

	override render() {
		return html`
			<div class=${classMap({ divide: this.divide, compact: this.look === 'compact' })}>
				<slot></slot>
			</div>
		`;
	}

	static override styles = [
		css`
			div {
				display: block;
				position: relative;
			}

			::slotted(*) {
				position: relative;
				margin-top: var(--uui-size-space-6);
			}

			.divide ::slotted(*)::before {
				content: '';
				position: absolute;
				top: calc((var(--uui-size-space-6) / 2) * -1);
				height: 0;
				width: 100%;
				border-top: solid 1px var(--uui-color-divider-standalone);
			}

			::slotted(*:first-child) {
				margin-top: 0;
			}

			.divide ::slotted(*:first-child)::before {
				display: none;
			}

			.compact ::slotted(*) {
				margin-top: var(--uui-size-space-3);
			}

			.compact ::slotted(*:first-child) {
				margin-top: 0;
			}

			.compact.divide ::slotted(*)::before {
				display: none;
			}
		`,
	];
}

export default UmbStackElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stack': UmbStackElement;
	}
}
