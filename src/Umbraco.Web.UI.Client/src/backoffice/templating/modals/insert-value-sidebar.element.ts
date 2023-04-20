import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';

@customElement('umb-insert-value-sidebar')
export default class UmbInsertValueSidebarElement extends UmbModalBaseElement<object, string> {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				color: var(--uui-color-text);
			}

			#main {
				box-sizing: border-box;
				padding: var(--uui-size-space-5);
				height: calc(100vh - 124px);
			}

			#main uui-button {
				width: 100%;
			}

			h3,
			p {
				text-align: left;
			}

			uui-combobox,
			uui-input {
				width: 100%;
			}
		`,
	];

	private _close() {
		this.modalHandler?.submit();
	}

	private _submit() {
		this.modalHandler?.submit('I am some value to be inserted');
	}

	@state()
	showDefaultValueInput = false;

	render() {
		return html`
			<umb-workspace-layout headline="Insert">
				<div id="main">
					<uui-box>
						<uui-form-layout-item>
							<uui-label slot="label" for="field-selector">Choose field</uui-label>
							<uui-combobox id="field-selector">
								<uui-combobox-list>
									<uui-combobox-list-option style="padding: 8px"> apple </uui-combobox-list-option>
									<uui-combobox-list-option style="padding: 8px"> orange </uui-combobox-list-option>
									<uui-combobox-list-option style="padding: 8px"> lemon </uui-combobox-list-option>
								</uui-combobox-list>
							</uui-combobox>
						</uui-form-layout-item>
						${this.showDefaultValueInput
							? html` <uui-form-layout-item>
									<uui-label slot="label" for="default-value">Default value</uui-label>
									<uui-input id="default-value" type="text" name="default-value" label="default value"> </uui-input>
							  </uui-form-layout-item>`
							: html` <uui-button
									@click=${() => (this.showDefaultValueInput = true)}
									look="placeholder"
									label="Add default value "
									>Add default value</uui-button
							  >`}
						<uui-form-layout-item>
							<uui-label slot="label" for="default-value">Recursive</uui-label>
							<uui-checkbox>Yes, make it recursive</uui-checkbox>
						</uui-form-layout-item>
						<uui-form-layout-item>
							<uui-label slot="label">Output sample</uui-label>
							<uui-code-block> Some code that goes here... </uui-code-block>
						</uui-form-layout-item>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary">Close</uui-button>
					<uui-button @click=${this._submit} look="primary">Submit</uui-button>
				</div>
			</umb-workspace-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-insert-value-sidebar': UmbInsertValueSidebarElement;
	}
}
