import { UUIBooleanInputEvent, UUIInputEvent, UUISelectEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UMB_PROPERTY_EDITOR_UI_PICKER_MODAL_TOKEN } from '../../property-editors/modals/property-editor-ui-picker';
import { UmbPropertySettingsModalResult } from '.';
import { UmbModalContext, UmbModalBaseElement, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import { ManifestPropertyEditorUI } from '@umbraco-cms/extensions-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';

@customElement('umb-property-settings-modal')
export class UmbPropertySettingsModalElement extends UmbModalBaseElement<undefined, UmbPropertySettingsModalResult> {
	static styles = [
		UUITextStyles,
		css`
			:host {
				color: var(--uui-color-text);
			}
			#content {
				padding: var(--uui-size-space-6);
			}
			#appearances {
				display: flex;
				gap: var(--uui-size-space-6);
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

	@state() private _selectedPropertyEditorUI?: ManifestPropertyEditorUI;
	@state() private _selectedPropertyEditorUIAlias = '';

	@state() private _appearanceIsTop = false;
	@state() private _mandatory = false;

	//TODO: Should these options come from the server?
	@state() private _customValidationOptions = [
		{
			name: 'No validation',
			value: 'no-validation',
			selected: true,
		},
		{
			name: 'Validate as an email address',
			value: 'email',
			validation: '[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+',
		},
		{
			name: 'Validate as a number',
			value: 'number',
			validation: '^[0-9]*$',
		},
		{
			name: 'Validate as an URL',
			value: 'url',
			validation: 'https?://[a-zA-Z0-9-.]+.[a-zA-Z]{2,}',
		},
		{
			name: '...or enter a custom validation',
			value: 'custom',
		},
	];
	@state() private _customValidation = this._customValidationOptions[0];

	@state() private _aliasLocked = true;
	@state() private _name = '';
	@state() private _alias = '';

	#modalContext?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});

		this.#observePropertyEditorUI();
	}

	#observePropertyEditorUI() {
		if (!this._selectedPropertyEditorUIAlias) return;

		this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('propertyEditorUI', this._selectedPropertyEditorUIAlias),
			(propertyEditorUI) => {
				if (!propertyEditorUI) return;

				this._selectedPropertyEditorUI = propertyEditorUI;
			}
		);
	}

	#onCustomValidationChange(event: UUISelectEvent) {
		const value = event.target.value;

		this._customValidation =
			this._customValidationOptions.find((option) => option.value === value) ?? this._customValidationOptions[0];
	}

	#onMandatoryChange(event: UUIBooleanInputEvent) {
		const value = event.target.checked;
		this._mandatory = value;
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

		const label = this._name || '';
		const alias = this._alias || '';
		const description = formData.get('description')?.toString() || '';
		const propertyEditorUI = this._selectedPropertyEditorUIAlias || undefined;
		const labelOnTop = this._appearanceIsTop;
		const mandatory = this._mandatory;
		const mandatoryMessage = formData.get('mandatory-message')?.toString() || '';
		const pattern = formData.get('pattern')?.toString() || '';
		const patternMessage = formData.get('pattern-message')?.toString() || '';

		this.modalHandler?.submit({
			label,
			alias,
			description,
			propertyEditorUI,
			labelOnTop,
			validation: {
				mandatory,
				mandatoryMessage,
				pattern,
				patternMessage,
			},
		});
	}

	#onNameChange(event: UUIInputEvent) {
		//TODO: Generate alias
		this._name = event.target.value.toString();
		if (this._aliasLocked) {
			this._alias = this.#generateAlias(this._name);
		}
	}

	// TODO: move this to a helper so we can reuse it across the app
	#generateAlias(text: string) {
		//replace all spaces characters with a dash and remove all non-alphanumeric characters, except underscore. Allow a maximum of 1 dashes or underscores in a row.
		return text
			.replace(/\s+/g, '-')
			.replace(/[^a-zA-Z0-9_-]+/g, '')
			.replace(/[-_]{2,}/g, (match) => match[0])
			.toLowerCase();
	}

	#onAliasChange(event: UUIInputEvent) {
		const alias = this.#generateAlias(event.target.value.toString());
		if (!this._aliasLocked) {
			this._alias = alias;
		} else {
			event.target.value = this._alias;
		}
	}

	#onAppearanceChange(event: MouseEvent) {
		const target = event.target as HTMLElement;
		const alreadySelected = target.classList.contains(this._appearanceIsTop ? 'top' : 'left');

		if (alreadySelected) return;

		this._appearanceIsTop = !this._appearanceIsTop;
	}

	#onOpenPropertyEditorUIPicker() {
		const modalHandler = this.#modalContext?.open(UMB_PROPERTY_EDITOR_UI_PICKER_MODAL_TOKEN, {
			selection: [],
		});

		if (!modalHandler) return;

		modalHandler?.onSubmit().then(({ selection }) => {
			if (selection.length === 0) return;
			// TODO: we might should set the alias to null or empty string, if no selection.
			this._selectedPropertyEditorUIAlias = selection[0];
			this.#observePropertyEditorUI();
		});
	}

	/* TODO:
	From Github comment: We should not re-generate the alias when it gets locked again.
  Generally the auto generation is not determined by the lock, but wether it has been changed or saved.
	The experience in existing backoffice is: we only generate an alias when a property is new, once it has been saved it should never change unless the user actively does so.
	On new properties, the alias auto-generates until the user has made a change to it. */
	#onToggleAliasLock() {
		this._aliasLocked = !this._aliasLocked;

		if (this._aliasLocked) {
			this._alias = this.#generateAlias(this._name);
		}
	}

	render() {
		return html`
			<uui-form>
				<form @submit="${this.#onSubmit}">
					<umb-workspace-layout headline="Property settings">
						<div id="content">
							<uui-box>
								<div class="container">
									<uui-input
										name="name"
										@input=${this.#onNameChange}
										.value=${this._name}
										placeholder="Enter a name...">
									</uui-input>
									<uui-input
										name="alias"
										@input=${this.#onAliasChange}
										.value=${this._alias}
										placeholder="Enter alias..."
										?disabled=${this._aliasLocked}>
										<div @click=${this.#onToggleAliasLock} @keydown=${() => ''} id="alias-lock" slot="prepend">
											<uui-icon name=${this._aliasLocked ? 'umb:lock' : 'umb:unlocked'}></uui-icon>
										</div>
									</uui-input>
									<uui-textarea name="description" placeholder="Enter description..."></uui-textarea>
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
					</umb-workspace-layout>
				</form>
			</uui-form>
		`;
	}

	#renderAlignLeftIcon() {
		return html`<div
			@click=${this.#onAppearanceChange}
			@keydown=${() => ''}
			class="appearance left ${this._appearanceIsTop ? '' : 'selected'}">
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
				@click=${this.#onAppearanceChange}
				@keydown=${() => ''}
				class="appearance top ${this._appearanceIsTop ? 'selected' : ''}">
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
				<uui-toggle @change=${this.#onMandatoryChange} id="mandatory" slot="editor"></uui-toggle>
			</div>
			${this._mandatory
				? html`<uui-input
						name="mandatory-message"
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

			${this._customValidation.value !== 'no-validation'
				? html`
						<uui-input
							name="pattern"
							style="margin-bottom: var(--uui-size-space-1); margin-top: var(--uui-size-space-5);"
							value=${this._customValidation.validation ?? ''}></uui-input>
						<uui-textarea name="pattern-message"></uui-textarea>
				  `
				: nothing} `;
	}
}

export default UmbPropertySettingsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-settings-modal': UmbPropertySettingsModalElement;
	}
}
