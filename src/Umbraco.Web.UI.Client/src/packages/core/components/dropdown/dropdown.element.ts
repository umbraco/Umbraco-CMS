import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

// TODO: maybe move this to UI Library.
@customElement('umb-dropdown')
export class UmbDropdownElement extends UmbLitElement {
	@property({ type: Boolean, reflect: true })
	open = false;

	render() {
		return html`
			<uui-popover id="container" .open=${this.open}>
				<slot name="trigger" slot="trigger"></slot>
				${this.open ? this.#renderDropdown() : nothing}
			</uui-popover>
		`;
	}

	#renderDropdown() {
		return html`
			<div id="dropdown" slot="popover">
				<slot name="dropdown"></slot>
			</div>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			#container {
				display: inline-block;
				width: unset;
			}

			#dropdown {
				overflow: hidden;
				z-index: -1;
				background-color: var(--uui-combobox-popover-background-color, var(--uui-color-surface));
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				width: 100%;
				height: 100%;
				box-sizing: border-box;
				box-shadow: var(--uui-shadow-depth-3);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-dropdown': UmbDropdownElement;
	}
}
