import { getUmbracoFieldSnippet } from '../utils.js';
import { UUITextStyles , UUIComboboxElement, UUIInputElement } from '@umbraco-cms/backoffice/external/uui';
import { css, html , customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';

@customElement('umb-insert-value-sidebar')
export default class UmbInsertValueSidebarElement extends UmbModalBaseElement<object, string> {
	private _close() {
		this.modalHandler?.submit();
	}

	private _submit() {
		this.modalHandler?.submit(this.output);
	}

	@state()
	showDefaultValueInput = false;

	@state()
	recursive = false;

	@state()
	defaultValue: string | null = null;

	@state()
	field: string | null = null;

	@state()
	output = '';

	protected willUpdate(): void {
		this.output = this.field ? getUmbracoFieldSnippet(this.field, this.defaultValue, this.recursive) : '';
	}

	#setField(event: Event) {
		const target = event.target as UUIComboboxElement;
		this.field = target.value as string;
	}

	#setDefaultValue(event: Event) {
		const target = event.target as UUIInputElement;
		this.defaultValue = target.value === '' ? null : (target.value as string);
	}

	render() {
		return html`
			<umb-body-layout headline="Insert">
				<div id="main">
					<uui-box>
						<uui-form-layout-item>
							<uui-label slot="label" for="field-selector">Choose field</uui-label>
							<uui-combobox id="field-selector" @change=${this.#setField}>
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
									<uui-input
										id="default-value"
										type="text"
										name="default-value"
										label="default value"
										@input=${this.#setDefaultValue}>
									</uui-input>
							  </uui-form-layout-item>`
							: html` <uui-button
									@click=${() => (this.showDefaultValueInput = true)}
									look="placeholder"
									label="Add default value "
									>Add default value</uui-button
							  >`}
						<uui-form-layout-item>
							<uui-label slot="label" for="default-value">Fallback</uui-label>
							<uui-checkbox @change=${() => (this.recursive = !this.recursive)}>From ancestors</uui-checkbox>
						</uui-form-layout-item>
						<uui-form-layout-item>
							<uui-label slot="label">Output sample</uui-label>
							<uui-code-block>${this.output}</uui-code-block>
						</uui-form-layout-item>
					</uui-box>
				</div>
				<div slot="actions">
					<uui-button @click=${this._close} look="secondary">Close</uui-button>
					<uui-button @click=${this._submit} look="primary">Submit</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				color: var(--uui-color-text);
				--umb-header-layout-height: 70px;
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-insert-value-sidebar': UmbInsertValueSidebarElement;
	}
}
