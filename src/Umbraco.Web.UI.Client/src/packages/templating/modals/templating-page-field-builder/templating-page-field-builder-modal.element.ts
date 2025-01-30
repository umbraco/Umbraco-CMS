import { getUmbracoFieldSnippet } from '../../utils/index.js';
import type {
	UmbTemplatingPageFieldBuilderModalData,
	UmbTemplatingPageFieldBuilderModalValue,
} from './templating-page-field-builder-modal.token.js';
import type { UmbTemplateFieldDropdownListElement } from './components/template-field-dropdown-list/index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UUIBooleanInputEvent, UUIButtonState, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

// import of local components
import './components/template-field-dropdown-list/index.js';
import { UmbValidationContext, umbBindToValidation } from '@umbraco-cms/backoffice/validation';

@customElement('umb-templating-page-field-builder-modal')
export class UmbTemplatingPageFieldBuilderModalElement extends UmbModalBaseElement<
	UmbTemplatingPageFieldBuilderModalData,
	UmbTemplatingPageFieldBuilderModalValue
> {
	#validation = new UmbValidationContext(this);

	#close() {
		this.modalContext?.reject();
	}

	async #submit() {
		this._submitButtonState = 'waiting';

		try {
			await this.#validation.validate();
			this._submitButtonState = 'success';

			this.value = { output: getUmbracoFieldSnippet(this._field!, this._default, this._recursive) };
			this.modalContext?.submit();
		} catch {
			this._submitButtonState = 'failed';
		}
	}

	@state()
	private _field?: string;

	@state()
	private _haveDefault: boolean = false;

	@state()
	private _default?: string;

	@state()
	private _recursive: boolean = false;

	@state()
	private _submitButtonState: UUIButtonState;

	/** TODO: Implement "Choose field" */

	#onChangeFieldValue(e: Event) {
		this._field = (e.target as UmbTemplateFieldDropdownListElement).value?.alias;
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('template_insert')}>
				<uui-box>
					<div>
						<umb-property-layout orientation="vertical" label=${this.localize.term('templateEditor_chooseField')}>
							<umb-template-field-dropdown-list
								slot="editor"
								required
								${umbBindToValidation(this)}
								@change=${this.#onChangeFieldValue}
								exclude-media-type></umb-template-field-dropdown-list>
						</umb-property-layout>

						<uui-label for="page-field-default-value">
							<umb-localize key="templateEditor_defaultValue">Default value</umb-localize>
						</uui-label>
						${!this._haveDefault
							? html`<uui-button
									label=${this.localize.term('templateEditor_addDefaultValue')}
									look="placeholder"
									@click=${() => (this._haveDefault = true)}></uui-button>`
							: html`<uui-input
									id="page-field-default-value"
									@change=${(e: UUIInputEvent) => (this._default = e.target.value as string)}
									label=${this.localize.term('templateEditor_defaultValue')}></uui-input>`}

						<uui-label for="recursive"><umb-localize key="templateEditor_recursive">Recursive</umb-localize></uui-label>
						<uui-checkbox
							id="recursive"
							label=${this.localize.term('templateEditor_recursiveDescr')}
							@change=${(e: UUIBooleanInputEvent) => (this._recursive = e.target.checked)}
							?disabled=${this._field ? false : true}></uui-checkbox>

						<uui-label><umb-localize key="templateEditor_outputSample">Output sample</umb-localize></uui-label>
						<umb-code-block style="max-height:500px;" language="C#" copy
							>${this._field ? getUmbracoFieldSnippet(this._field, this._default, this._recursive) : ''}</umb-code-block
						>
					</div>
				</uui-box>
				<uui-button
					slot="actions"
					@click=${this.#close}
					look="secondary"
					label=${this.localize.term('general_close')}></uui-button>
				<uui-button
					slot="actions"
					@click=${this.#submit}
					color="positive"
					look="primary"
					.state=${this._submitButtonState}
					label=${this.localize.term('general_submit')}></uui-button>
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-box > div {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}

			uui-label:not(:first-child) {
				margin-top: var(--uui-size-space-6);
			}
		`,
	];
}

export default UmbTemplatingPageFieldBuilderModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-page-field-builder-modal': UmbTemplatingPageFieldBuilderModalElement;
	}
}
