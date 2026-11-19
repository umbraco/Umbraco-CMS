import { css, customElement, LitElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-separator
 * @description A horizontal line between items in a list or menu. It only draws the line; whatever renders it decides where it goes.
 * @augments {LitElement}
 */
@customElement('umb-separator')
export class UmbSeparatorElement extends LitElement {
	override connectedCallback() {
		super.connectedCallback();
		if (!this.hasAttribute('role')) {
			this.setAttribute('role', 'separator');
		}
	}

	static override styles = [
		css`
			:host {
				display: block;
				border-top: 1px solid var(--uui-color-divider-standalone);
				margin: var(--uui-size-space-2) var(--uui-size-space-3);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-separator': UmbSeparatorElement;
	}
}
