import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, property, query } from '@umbraco-cms/backoffice/external/lit';
import { UUIBooleanInputElement, UUIInputElement } from '@umbraco-ui/uui';
import { getAddSectionSnippet, getRenderBodySnippet, getRenderSectionSnippet } from '../../utils.js';

@customElement('umb-insert-section-checkbox')
export class UmbInsertSectionCheckboxElement extends UUIBooleanInputElement {
	renderCheckbox() {
		return html``;
	}

	@property({ type: Boolean, attribute: 'show-mandatory' })
	showMandatory = false;

	@property({ type: Boolean, attribute: 'show-input' })
	showInput = false;

	@query('uui-input')
	input?: UUIInputElement;

	@query('form')
	form?: HTMLFormElement;

	@query('uui-checkbox')
	checkbox?: HTMLFormElement;

	validate() {
		if (!this.form) return true;

		this.form.requestSubmit();
		return this.form.checkValidity();
	}

	#preventDefault(event: Event) {
		event.preventDefault();
	}

	get inputValue() {
		return this.input?.value;
	}

	get isMandatory() {
		return this.checkbox?.checked;
	}

	/* eslint-disable lit-a11y/click-events-have-key-events */
	render() {
		return html`
			${super.render()}
			<h3 @click=${this.click}>${this.checked ? html`<uui-icon name="umb:check"></uui-icon>` : ''}${this.label}</h3>
			<div @click=${this.click}>
				<slot name="description"><p>here goes some description</p></slot>
			</div>
			${this.checked && this.showInput
				? html`<uui-form>
						<form @submit=${this.#preventDefault}>
							<uui-form-layout-item>
								<uui-label slot="label" for="section-name-input" required>Section name</uui-label>
								<uui-input
									required
									placeholder="Enter section name"
									id="section-name-input"></uui-input> </uui-form-layout-item
							>${this.showMandatory
								? html`<p slot="if-checked">
										<uui-checkbox label="Section is mandatory">Section is mandatory </uui-checkbox><br />
										<small
											>If mandatory, the child template must contain a <code>@section</code> definition, otherwise an
											error is shown.</small
										>
								  </p>`
								: ''}
						</form>
				  </uui-form>`
				: ''}
		`;
	}
	/* eslint-enable lit-a11y/click-events-have-key-events */

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
				padding: 6px 18px;
			}

			:host(:hover),
			:host(:focus),
			:host(:focus-within) {
				background-color: var(--uui-button-background-color-hover, transparent);
				color: var(--uui-color-default-emphasis, #3544b1);
				border-color: var(--uui-color-default-emphasis, #3544b1);
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

			h3,
			p {
				text-align: left;
			}

			uui-input {
				width: 100%;
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
