import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';

// TODO: maybe move this to UI Library.
@customElement('umb-dropdown')
export class UmbDropdownElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			#container {
				display: inline-block;
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-dropdown': UmbDropdownElement;
	}
}
