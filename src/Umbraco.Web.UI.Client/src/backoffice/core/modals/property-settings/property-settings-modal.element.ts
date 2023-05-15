import { UUIBooleanInputEvent, UUIInputEvent, UUISelectEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { PropertyValueMap, css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import {
	UmbModalContext,
	UMB_MODAL_CONTEXT_TOKEN,
	UMB_PROPERTY_EDITOR_UI_PICKER_MODAL,
	UmbPropertySettingsModalResult,
	UmbPropertySettingsModalData,
} from '@umbraco-cms/backoffice/modal';
import { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extensions-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { generateAlias } from '@umbraco-cms/backoffice/utils';

@customElement('umb-property-settings-modal')
// TODO: Could base take a token to get its types?.
export class UmbPropertySettingsModalElement extends UmbModalBaseElement<
	UmbPropertySettingsModalData,
	UmbPropertySettingsModalResult
> {
	@state() private _selectedPropertyEditorUI?: ManifestPropertyEditorUI;

	//TODO: Should these options come from the server?
	@state() private _customValidationOptions = [
		{
			name: 'No validation',
			value: '',
			selected: true,
		},
		{
			name: 'Validate as an email address',
			value: '[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\\.[a-zA-Z0-9-.]+',
		},
		{
			name: 'Validate as a number',
			value: '^[0-9]*$',
		},
		{
			name: 'Validate as an URL',
			value: 'https?://[a-zA-Z0-9-.]+\\.[a-zA-Z]{2,}',
		},
		{
			name: '...or enter a custom validation',
			value: '',
		},
	];

	@state() private _aliasLocked = true;

	#modalContext?: UmbModalContext;

	@state()
	protected _returnData!: UmbPropertySettingsModalResult;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		this._returnData = JSON.parse(JSON.stringify(this.data));

		const regEx = this._returnData.validation?.regEx ?? '';
		const newlySelected = this._customValidationOptions.find((option) => {
			option.selected = option.value === regEx;
			return option.selected;
		});
		if (newlySelected === undefined) {
			this._customValidationOptions[4].selected = true;
		}

		this.#observePropertyEditorUI();
	}

	protected firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);

		// TODO: Make a general way to put focus on a input in a modal. (also make sure it only happens if its the top-most-modal.)
		requestAnimationFrame(() => {
			(this.shadowRoot!.querySelector('#nameInput') as HTMLElement).focus();
		});
	}

	#observePropertyEditorUI() {
		if (!this._returnData.dataTypeId) return;

		this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('propertyEditorUI', this._returnData.dataTypeId),
			(propertyEditorUI) => {
				if (!propertyEditorUI) return;

				this._selectedPropertyEditorUI = propertyEditorUI;
			},
			'observePropertyEditorUI'
		);
	}

	#onClose() {
		this.modalHandler?.reject();
	}

	#onSubmit(event: SubmitEvent) {
		event.preventDefault();

		const form = event.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);

		this._returnData.validation!.mandatoryMessage = formData.get('mandatory-message')?.toString() || '';

		this.modalHandler?.submit(this._returnData);
	}

	#onNameChange(event: UUIInputEvent) {
		//TODO: Generate alias
		const oldName = this._returnData.name;
		const oldAlias = this._returnData.alias;
		this._returnData.name = event.target.value.toString();
		if (this._aliasLocked) {
			const expectedOldAlias = generateAlias(oldName ?? '');
			// Only update the alias if the alias matches a generated alias of the old name (otherwise the alias is considered one written by the user.)
			if (expectedOldAlias === oldAlias) {
				this._returnData.alias = generateAlias(this._returnData.name);
				this.requestUpdate('_returnData');
			}
		}
	}

	#onAliasChange(event: UUIInputEvent) {
		const alias = generateAlias(event.target.value.toString());
		if (!this._aliasLocked) {
			this._returnData.alias = alias;
		} else {
			this._returnData.alias = this.data?.alias;
		}
		this.requestUpdate('_returnData');
	}

	#onCustomValidationChange(event: UUISelectEvent) {
		const regEx = event.target.value.toString();
		this._returnData.validation!.regEx = regEx;
		this.requestUpdate('_returnData');
	}

	#onMandatoryChange(event: UUIBooleanInputEvent) {
		const value = event.target.checked;
		this._returnData.validation!.mandatory = value;
		this.requestUpdate('_returnData');
	}

	#onMandatoryMessageChange(event: UUIInputEvent) {
		const value = event.target.value.toString();
		this._returnData.validation!.mandatoryMessage = value;
		this.requestUpdate('_returnData');
	}

	#setAppearanceNormal() {
		const currentValue = this._returnData.appearance?.labelOnTop;
		if (currentValue !== true) return;

		this._returnData.appearance!.labelOnTop = false;
		this.requestUpdate('_returnData');
	}
	#setAppearanceTop() {
		const currentValue = this._returnData.appearance?.labelOnTop;
		if (currentValue === true) return;

		this._returnData.appearance!.labelOnTop = true;
		this.requestUpdate('_returnData');
	}

	#onOpenPropertyEditorUIPicker() {
		const modalHandler = this.#modalContext?.open(UMB_PROPERTY_EDITOR_UI_PICKER_MODAL, {
			selection: [],
		});

		if (!modalHandler) return;

		modalHandler?.onSubmit().then(({ selection }) => {
			if (selection.length === 0) return;
			// TODO: we might should set the alias to null or empty string, if no selection.
			this._returnData.dataTypeId = selection[0];
			this.requestUpdate('_returnData');
			this.#observePropertyEditorUI();
		});
	}

	#onToggleAliasLock() {
		this._aliasLocked = !this._aliasLocked;

		if (this._aliasLocked) {
			this._returnData.alias = generateAlias(this._returnData.alias ?? '');
			this.requestUpdate('_returnData');
		}
	}

	#onValidationRegExChange(event: UUIInputEvent) {
		const regEx = event.target.value.toString();
		const newlySelected = this._customValidationOptions.find((option) => {
			option.selected = option.value === regEx;
			return option.selected;
		});
		if (newlySelected === undefined) {
			this._customValidationOptions[4].selected = true;
		}
		this._returnData.validation!.regEx = regEx;
		this.requestUpdate('_returnData');
	}
	#onValidationMessageChange(event: UUIInputEvent) {
		this._returnData.validation!.regExMessage = event.target.value.toString();
		this.requestUpdate('_returnData');
	}

	render() {
		return html`
			<uui-form>
				<form @submit="${this.#onSubmit}">
					<umb-workspace-editor headline="Property settings">
						<div id="content">
							<uui-box>
								<div class="container">
									<uui-input
										id="nameInput"
										name="name"
										@input=${this.#onNameChange}
										.value=${this._returnData.name}
										placeholder="Enter a name...">
										<!-- TODO: validation for bad characters -->
									</uui-input>
									<uui-input
										name="alias"
										@input=${this.#onAliasChange}
										.value=${this._returnData.alias}
										placeholder="Enter alias..."
										?disabled=${this._aliasLocked}>
										<!-- TODO: validation for bad characters -->
										<div @click=${this.#onToggleAliasLock} @keydown=${() => ''} id="alias-lock" slot="prepend">
											<uui-icon name=${this._aliasLocked ? 'umb:lock' : 'umb:unlocked'}></uui-icon>
										</div>
									</uui-input>
									<uui-textarea
										name="description"
										placeholder="Enter description..."
										.value=${this._returnData.description}></uui-textarea>
								</div>
								${this.#renderPropertyUIPicker()}
								<hr />
								<div class="container">
									<b>Validation</b>
									${this.#renderMandatory()}
									<p style="margin-bottom: 0">Custom validation</p>
									${this.#renderCustomValidation()}
								</div>
								<hr />
								<div class="container">
									<b style="margin-bottom: var(--uui-size-space-3)">Appearance</b>
									<div id="appearances">${this.#renderAlignLeftIcon()} ${this.#renderAlignTopIcon()}</div>
								</div>
							</uui-box>
						</div>
						<div slot="actions">
							<uui-button label="Close" @click=${this.#onClose}></uui-button>
							<uui-button label="Submit" look="primary" color="positive" type="submit"></uui-button>
						</div>
					</umb-workspace-editor>
				</form>
			</uui-form>
		`;
	}

	#renderAlignLeftIcon() {
		return html`<div
			@click=${this.#setAppearanceNormal}
			@keydown=${() => ''}
			class="appearance left ${this._returnData.appearance?.labelOnTop ? '' : 'selected'}">
			<svg width="260" height="32" viewBox="0 0 260 60" fill="none" xmlns="http://www.w3.org/2000/svg">
				<rect width="89" height="14" rx="7" fill="currentColor" />
				<rect x="121" width="139" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
				<rect x="121" y="46" width="108" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
				<rect x="121" y="23" width="139" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
			</svg>
			<label class="appearance-label"> Label on the left </label>
		</div>`;
	}

	#renderAlignTopIcon() {
		return html`
			<div
				@click=${this.#setAppearanceTop}
				@keydown=${() => ''}
				class="appearance top ${this._returnData.appearance?.labelOnTop ? 'selected' : ''}">
				<svg width="139" height="48" viewBox="0 0 139 90" fill="none" xmlns="http://www.w3.org/2000/svg">
					<rect width="89" height="14" rx="7" fill="currentColor" />
					<rect y="30" width="139" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
					<rect y="76" width="108" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
					<rect y="53" width="139" height="10" rx="5" fill="currentColor" fill-opacity="0.4" />
				</svg>
				<label class="appearance-label"> Label on top </label>
			</div>
		`;
	}

	#renderMandatory() {
		return html`<div style="display: flex; justify-content: space-between">
				<label for="mandatory">Field is mandatory</label>
				<uui-toggle
					@change=${this.#onMandatoryChange}
					id="mandatory"
					value=${this._returnData.validation?.mandatory}
					slot="editor"></uui-toggle>
			</div>
			${this._returnData.validation?.mandatory
				? html`<uui-input
						name="mandatory-message"
						value=${this._returnData.validation?.mandatoryMessage}
						@change=${this.#onMandatoryMessageChange}
						style="margin-top: var(--uui-size-space-1)"
						id="mandatory-message"
						placeholder="Enter a custom validation error message (optional)"></uui-input>`
				: ''}`;
	}

	#renderPropertyUIPicker() {
		return this._selectedPropertyEditorUI
			? html`
					<umb-ref-property-editor-ui
						name=${this._selectedPropertyEditorUI.meta.label}
						alias=${this._selectedPropertyEditorUI.alias}
						property-editor-model-alias=${this._selectedPropertyEditorUI.meta.propertyEditorModel}
						border>
						<uui-icon name="${this._selectedPropertyEditorUI.meta.icon}" slot="icon"></uui-icon>
						<uui-action-bar slot="actions">
							<uui-button label="Change" @click=${this.#onOpenPropertyEditorUIPicker}></uui-button>
						</uui-action-bar>
					</umb-ref-property-editor-ui>
			  `
			: html`
					<uui-button
						id="property-editor-ui-picker"
						label="Select Property Editor"
						look="placeholder"
						color="default"
						@click=${this.#onOpenPropertyEditorUIPicker}></uui-button>
			  `;
	}

	#renderCustomValidation() {
		return html`<uui-select
				style="margin-top: var(--uui-size-space-1)"
				@change=${this.#onCustomValidationChange}
				.options=${this._customValidationOptions}></uui-select>

			${this._returnData.validation?.regEx !== ''
				? html`
						<uui-input
							name="pattern"
							style="margin-bottom: var(--uui-size-space-1); margin-top: var(--uui-size-space-5);"
							@change=${this.#onValidationRegExChange}
							.value=${this._returnData.validation?.regEx ?? ''}></uui-input>
						<uui-textarea
							name="pattern-message"
							@change=${this.#onValidationMessageChange}
							.value=${this._returnData.validation?.regExMessage ?? ''}></uui-textarea>
				  `
				: nothing} `;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				color: var(--uui-color-text);
			}
			#content {
				padding: var(--uui-size-layout-1);
			}
			#appearances {
				display: flex;
				gap: var(--uui-size-layout-1);
				max-width: 350px;
				margin: 0 auto;
			}
			.appearance {
				position: relative;
				display: flex;
				border: 2px solid var(--uui-color-border-standalone);
				padding: var(--uui-size-space-4) var(--uui-size-space-5);
				align-items: center;
				border-radius: 6px;
				opacity: 0.8;
				flex-direction: column;
				justify-content: space-between;
				gap: var(--uui-size-space-3);
			}
			.appearance-label {
				font-size: 0.8rem;
				line-height: 1;
			}
			.appearance.selected .appearance-label {
				font-weight: bold;
			}
			.appearance:not(.selected):hover {
				border-color: var(--uui-color-border-emphasis);
				cursor: pointer;
				opacity: 1;
			}
			.appearance.selected {
				border-color: var(--uui-color-selected);
				opacity: 1;
			}
			.appearance.selected::after {
				content: '';
				position: absolute;
				inset: 0;
				border-radius: 6px;
				opacity: 0.1;
				background-color: var(--uui-color-selected);
			}
			.appearance.left {
				flex-grow: 1;
			}
			.appearance.top {
				flex-shrink: 1;
			}
			.appearance svg {
				display: flex;
				width: 100%;
				color: var(--uui-color-text);
			}
			hr {
				border: none;
				border-top: 1px solid var(--uui-color-divider);
				margin-top: var(--uui-size-space-6);
				margin-bottom: var(--uui-size-space-5);
			}
			uui-input {
				width: 100%;
			}
			#alias-lock {
				display: flex;
				align-items: center;
				justify-content: center;
				cursor: pointer;
			}
			#alias-lock uui-icon {
				margin-bottom: 2px;
			}
			#property-editor-ui-picker {
				width: 100%;
				--uui-button-padding-top-factor: 4;
				--uui-button-padding-bottom-factor: 4;
			}
			.container {
				display: flex;
				flex-direction: column;
			}
			uui-form,
			form {
				display: block;
				height: 100%;
			}
		`,
	];
}

export default UmbPropertySettingsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-settings-modal': UmbPropertySettingsModalElement;
	}
}
