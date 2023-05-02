import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UUIBooleanInputElement } from '@umbraco-ui/uui';

@customElement('umb-insert-section-checkbox')
export class UmbInsertSectionCheckboxElement extends UUIBooleanInputElement {
	renderCheckbox() {
		return html`
			<h3>${this.checked ? html`<uui-icon name="umb:check"></uui-icon>` : ''}${this.label}</h3>
			<slot><p>here goes some description</p></slot>
			${this.checked ? html`<slot name="if-checked"></slot>` : ''}
		`;
	}

	static styles = [
		...UUIBooleanInputElement.styles,
		UUITextStyles,
		css`
			:host {
				display: block;
				border-style: dashed;
				background-color: transparent;
				color: var(--uui-color-default-standalone, rgb(28, 35, 59));
				border-color: var(--uui-color-border-standalone, #c2c2c2);
				border-radius: var(--uui-border-radius, 3px);
				border-width: 1px;
				line-height: normal;
			}

			:host(:hover) {
				background-color: var(--uui-button-background-color-hover, transparent);
				color: var(--uui-color-default-emphasis, #3544b1);
				border-color: var(--uui-color-default-emphasis, #3544b1);
			}

			label {
				padding: 6px 18px;
				display: block;
			}

			uui-icon {
				background-color: var(--uui-color-positive-emphasis);
				border-radius: 50%;
				padding: 0.2em;
				margin-right: 1ch;
				color: var(--uui-color-positive-contrast);
				font-size: 0.7em;
			}

			::slotted(*) {
				line-height: normal;
			}

			.label {
				display: none;
			}
		`,
	];
}

export default UmbInsertSectionCheckboxElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-insert-section-input': UmbInsertSectionCheckboxElement;
	}
}
