import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UUIBooleanInputEvent, UUIInputEvent, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { PropertyValueMap, css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { UmbPropertySettingsModalValue, UmbPropertySettingsModalData } from '@umbraco-cms/backoffice/modal';
import { generateAlias } from '@umbraco-cms/backoffice/utils';
import { UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/document-type';
// TODO: Could base take a token to get its types?.
// TODO: Missing a workspace context... unless this should not be a workspace any way.
@customElement('umb-property-settings-modal')
export class UmbPropertySettingsModalElement extends UmbModalBaseElement<
	UmbPropertySettingsModalData,
	UmbPropertySettingsModalValue
> {
	//TODO: Should these options come from the server?
	// TODO: Or should they come from a extension point?
	@state() private _customValidationOptions = [
		{
			name: 'No validation',
			value: null,
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

	@state()
	protected _ownerDocumentType?: UmbPropertySettingsModalValue;

	@state()
	protected _returnData!: UmbPropertySettingsModalValue;

	connectedCallback(): void {
		super.connectedCallback();

		// TODO: This is actually not good enough, we need to be able to get to the DOCUMENT_WORKSPACE_CONTEXT, so we can have a look at the draft/runtime version of the document. Otherwise 'Vary by culture' is first updated when saved.
		this.consumeContext(UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN, (instance) => {
			this.observe(
				instance.byId(this.data?.documentTypeId),
				(documentType) => {
					this._ownerDocumentType = documentType;
					this.requestUpdate('_ownerDocumentType');
				},
				'_observeDocumentType',
			);
		});

		this._returnData = JSON.parse(JSON.stringify(this.data?.propertyData ?? {}));

		this._returnData.validation ??= {};

		const regEx = this._returnData.validation.regEx ?? null;
		const newlySelected = this._customValidationOptions.find((option) => {
			option.selected = option.value === regEx;
			return option.selected;
		});
		if (newlySelected === undefined) {
			this._customValidationOptions[4].selected = true;
			this._returnData.validation.regEx = this._customValidationOptions[4].value;
		} else {
			this._returnData.validation.regEx = regEx;
		}
	}

	protected firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);

		// TODO: Make a general way to put focus on a input in a modal. (also make sure it only happens if its the top-most-modal.)
		requestAnimationFrame(() => {
			(this.shadowRoot!.querySelector('#name-input') as HTMLElement).focus();
		});
	}

	#onSubmit(event: SubmitEvent) {
		event.preventDefault();

		const form = event.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		this.modalContext?.submit(this._returnData);
	}

	#onNameChange(event: UUIInputEvent) {
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
			this._returnData.alias = this.data?.propertyData?.alias;
		}
		this.requestUpdate('_returnData');
	}

	#onDataTypeIdChange(event: UUIInputEvent) {
		const dataTypeId = event.target.value.toString();
		this._returnData.dataTypeId = dataTypeId;
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

	#onToggleAliasLock() {
		this._aliasLocked = !this._aliasLocked;
	}

	#onCustomValidationChange(event: UUISelectEvent) {
		const regEx = event.target.value || event.target.value === '' ? event.target.value.toString() : null;

		this._customValidationOptions.forEach((option) => {
			option.selected = option.value === regEx;
		});

		this._returnData.validation!.regEx = regEx ?? null;
		this.requestUpdate('_returnData');
		this.requestUpdate('_customValidationOptions');
	}

	#onValidationRegExChange(event: UUIInputEvent) {
		const regEx = event.target.value || event.target.value === '' ? event.target.value.toString() : null;
		const betterChoice = this._customValidationOptions.find((option) => {
			option.selected = option.value === regEx;
			return option.selected;
		});
		if (betterChoice === undefined) {
			this._customValidationOptions[4].selected = true;
		}
		this._returnData.validation!.regEx = regEx;
		this.requestUpdate('_returnData');
		this.requestUpdate('_customValidationOptions');
	}
	#onValidationMessageChange(event: UUIInputEvent) {
		this._returnData.validation!.regExMessage = event.target.value.toString();
		this.requestUpdate('_returnData');
	}

	#onVaryByCultureChange(event: UUIBooleanInputEvent) {
		this._returnData.variesByCulture = event.target.checked;
		this.requestUpdate('_returnData');
	}

	// TODO: This would conceptually be a Property Editor Workspace, should be changed at one point in the future.
	// For now this is hacky made available by giving the element an fixed alias.
	// This would allow for workspace views and workspace actions.
	render() {
		return html`
			<uui-form>
				<form @submit="${this.#onSubmit}">
					<umb-workspace-editor alias="Umb.Workspace.PropertyEditor" headline="Property settings">
						<div id="content">
							<uui-box>
								<div class="container">
									<!-- TODO: Align styling across this and the property of document type workspace editor, or consider if this can go away for a different UX flow -->
									<uui-input
										id="name-input"
										name="name"
										@input=${this.#onNameChange}
										.value=${this._returnData.name}
										placeholder="Enter a name...">
										<!-- TODO: validation for bad characters -->
									</uui-input>
									<uui-input
										id="alias-input"
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
										id="description-input"
										name="description"
										placeholder="Enter description..."
										.value=${this._returnData.description}></uui-textarea>
								</div>
								<umb-data-type-flow-input
									.value=${this._returnData.dataTypeId}
									@change=${this.#onDataTypeIdChange}></umb-data-type-flow-input>
								<hr />
								<div class="container">
									<b>Validation</b>
									${this.#renderMandatory()}
									<p style="margin-bottom: 0">Custom validation</p>
									${this.#renderCustomValidation()}
								</div>
								<hr />
								${this.#renderVariationControls()}
								<div class="container">
									<b style="margin-bottom: var(--uui-size-space-3)">Appearance</b>
									<div id="appearances">${this.#renderAlignLeftIcon()} ${this.#renderAlignTopIcon()}</div>
								</div>
							</uui-box>
						</div>
						<div slot="actions">
							<uui-button label="Submit" look="primary" color="positive" type="submit"></uui-button>
						</div>
					</umb-workspace-editor>
				</form>
			</uui-form>
		`;
	}

	#renderAlignLeftIcon() {
		return html`<button
			type="button"
			@click=${this.#setAppearanceNormal}
			class="appearance left ${this._returnData.appearance?.labelOnTop ? '' : 'selected'}">
			<svg width="200" height="48" viewBox="0 0 200 60" fill="none" xmlns="http://www.w3.org/2000/svg">
				<rect width="94" height="14" rx="6" fill="currentColor" />
				<rect y="22" width="64" height="9" rx="4" fill="currentColor" fill-opacity="0.4" />
				<rect x="106" width="94" height="60" rx="5" fill="currentColor" fill-opacity="0.4" />
			</svg>
			<label class="appearance-label"> Label on the left </label>
		</button>`;
	}

	#renderAlignTopIcon() {
		return html`
			<button
				type="button"
				@click=${this.#setAppearanceTop}
				class="appearance top ${this._returnData.appearance?.labelOnTop ? 'selected' : ''}">
				<svg width="140" height="48" viewBox="0 0 140 60" fill="none" xmlns="http://www.w3.org/2000/svg">
					<rect width="90" height="14" rx="6" fill="currentColor" />
					<rect y="22" width="64" height="9" rx="4" fill="currentColor" fill-opacity="0.4" />
					<rect y="42" width="140" height="36" rx="5" fill="currentColor" fill-opacity="0.4" />
				</svg>
				<label class="appearance-label"> Label on top </label>
			</button>
		`;
	}

	#renderMandatory() {
		return html`<div style="display: flex; justify-content: space-between">
				<label for="mandatory">Field is mandatory</label>
				<uui-toggle
					@change=${this.#onMandatoryChange}
					id="mandatory"
					.checked=${this._returnData.validation?.mandatory ?? false}
					slot="editor"></uui-toggle>
			</div>
			${this._returnData.validation?.mandatory
				? html`<uui-input
						name="mandatory-message"
						value=${this._returnData.validation?.mandatoryMessage ?? ''}
						@change=${this.#onMandatoryMessageChange}
						style="margin-top: var(--uui-size-space-1)"
						id="mandatory-message"
						placeholder="Enter a custom validation error message (optional)"></uui-input>`
				: ''}`;
	}

	#renderCustomValidation() {
		return html`<uui-select
				style="margin-top: var(--uui-size-space-1)"
				@change=${this.#onCustomValidationChange}
				.options=${this._customValidationOptions}></uui-select>

			${this._returnData.validation?.regEx !== null
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

	#renderVariationControls() {
		return this._ownerDocumentType?.variesByCulture || this._ownerDocumentType?.variesBySegment
			? html` <div class="container">
						<b>Variation</b>
						${this._ownerDocumentType?.variesByCulture ? this.#renderVaryByCulture() : ''}
					</div>
					<hr />`
			: '';
	}
	#renderVaryByCulture() {
		return html`<uui-toggle
			@change=${this.#onVaryByCultureChange}
			.checked=${this._returnData.variesByCulture ?? false}
			label="Vary by culture"></uui-toggle> `;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				color: var(--uui-color-text);
			}
			#content {
				padding: var(--uui-size-layout-1);
			}
			#alias-input,
			#label-input,
			#description-input {
				width: 100%;
			}

			#alias-input {
				border-color: transparent;
				background: var(--uui-color-surface);
			}

			#label-input {
				font-weight: bold; /* TODO: UUI Input does not support bold text yet */
				--uui-input-border-color: transparent;
			}
			#label-input input {
				font-weight: bold;
				--uui-input-border-color: transparent;
			}

			#alias-lock {
				display: flex;
				align-items: center;
				justify-content: center;
				cursor: pointer;
			}
			#alias-lock uui-icon {
				margin-bottom: 2px;
				/* margin: 0; */
			}
			#description-input {
				--uui-textarea-border-color: transparent;
				font-weight: 0.5rem; /* TODO: Cant change font size of UUI textarea yet */
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
				border: 1px solid var(--uui-color-border-standalone);
				background-color: transparent;
				padding: var(--uui-size-space-4) var(--uui-size-space-5);
				align-items: center;
				border-radius: var(--uui-border-radius);
				opacity: 0.8;
				flex-direction: column;
				justify-content: space-between;
				gap: var(--uui-size-space-3);
			}
			.appearance-label {
				font-size: 0.8rem;
				line-height: 1;
				font-weight: bold;
				pointer-events: none;
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
			.appearance:not(.selected):hover {
				border-color: var(--uui-color-border-emphasis);
				cursor: pointer;
				opacity: 1;
			}
			.appearance.selected {
				background-color: var(--uui-color-surface);
				border-color: var(--uui-color-selected);
				color: var(--uui-color-selected);
				opacity: 1;
			}
			.appearance.selected svg {
				color: var(--uui-color-selected);
			}
			.appearance.selected::after {
				content: '';
				position: absolute;
				inset: 0;
				border-radius: 6px;
				opacity: 0.1;
				background-color: var(--uui-color-selected);
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
