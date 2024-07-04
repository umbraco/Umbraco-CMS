import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_CONTENT_TYPE_WORKSPACE_CONTEXT, UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UUIBooleanInputEvent, UUIInputEvent, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import { generateAlias } from '@umbraco-cms/backoffice/utils';
import { UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT } from '../../../index.js';

@customElement('umb-property-type-workspace-view-settings')
export class UmbPropertyTypeWorkspaceViewSettingsElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#context?: typeof UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT.TYPE;

	@state() private _customValidationOptions: Array<Option> = [
		{
			name: this.localize.term('validation_validateNothing'),
			value: '!NOVALIDATION!',
			selected: true,
		},
		{
			name: this.localize.term('validation_validateAsEmail'),
			value: '[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\\.[a-zA-Z0-9-.]+',
		},
		{
			name: this.localize.term('validation_validateAsNumber'),
			value: '^[0-9]*$',
		},
		{
			name: this.localize.term('validation_validateAsUrl'),
			value: 'https?://[a-zA-Z0-9-.]+\\.[a-zA-Z]{2,}',
		},
		{
			name: this.localize.term('validation_enterCustomValidation'),
			value: '.+',
		},
	];

	@state()
	private _data?: UmbPropertyTypeModel;

	@state()
	private _aliasLocked = true;

	@state()
	private _contentTypeVariesByCulture?: boolean;

	@state()
	private _contentTypeVariesBySegment?: boolean;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#context = instance;
			this.observe(instance.data, (data) => {
				this._data = data;
			});
		});

		this.consumeContext(UMB_CONTENT_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.observe(instance.variesByCulture, (variesByCulture) => (this._contentTypeVariesByCulture = variesByCulture));
			this.observe(instance.variesBySegment, (variesBySegment) => (this._contentTypeVariesBySegment = variesBySegment));
		}).passContextAliasMatches();
	}

	updateValue(partialValue: Partial<UmbPropertyTypeModel>) {
		this.#context?.updateData(partialValue);
	}

	#onNameChange(event: UUIInputEvent) {
		const oldName = this._data?.name;
		const oldAlias = this._data?.alias;
		this.updateValue({ name: event.target.value.toString() });
		if (this._aliasLocked) {
			const expectedOldAlias = generateAlias(oldName ?? '');
			// Only update the alias if the alias matches a generated alias of the old name (otherwise the alias is considered one written by the user.) [NL]
			if (expectedOldAlias === oldAlias) {
				this.updateValue({ alias: generateAlias(this._data?.name ?? '') });
			}
		}
	}

	#onAliasChange(event: UUIInputEvent) {
		const alias = generateAlias(event.target.value.toString());
		if (this._aliasLocked) {
			this.updateValue({ alias });
		}
	}

	#onDescriptionChange(event: UUIInputEvent) {
		this.updateValue({ description: event.target.value.toString() });
	}

	#onDataTypeIdChange(event: UUIInputEvent) {
		const dataTypeUnique = event.target.value.toString();
		this.updateValue({ dataType: { unique: dataTypeUnique } });
	}

	#onMandatoryChange(event: UUIBooleanInputEvent) {
		const mandatory = event.target.checked;
		this.updateValue({
			validation: { ...this._data?.validation, mandatory },
		});
	}

	#onMandatoryMessageChange(event: UUIInputEvent) {
		const mandatoryMessage = event.target.value.toString();
		this.updateValue({
			validation: { ...this._data?.validation, mandatory: this._data?.validation.mandatory ?? false, mandatoryMessage },
		});
	}

	#setAppearanceNormal() {
		const currentValue = this._data?.appearance?.labelOnTop;
		if (currentValue !== true) return;

		this.updateValue({
			appearance: { ...this._data?.appearance, labelOnTop: false },
		});
	}
	#setAppearanceTop() {
		const currentValue = this._data?.appearance?.labelOnTop;
		if (currentValue === true) return;

		this.updateValue({
			appearance: { ...this._data?.appearance, labelOnTop: true },
		});
	}

	#onToggleAliasLock() {
		this._aliasLocked = !this._aliasLocked;
	}

	#onCustomValidationChange(event: UUISelectEvent) {
		const value = event.target.value.toString();
		const regEx = value !== '!NOVALIDATION!' ? value : null;

		this.updateValue({
			validation: { ...this._data?.validation, mandatory: this._data?.validation.mandatory ?? false, regEx },
		});
	}

	#onValidationRegExChange(event: UUIInputEvent) {
		const value = event.target.value.toString();
		const regEx = value !== '!NOVALIDATION!' ? value : null;
		const betterChoice = this._customValidationOptions.find((option) => {
			option.selected = option.value === value;
			return option.selected;
		});
		if (betterChoice === undefined) {
			this._customValidationOptions[4].selected = true;
			this.requestUpdate('_customValidationOptions');
		}
		this.updateValue({
			validation: { ...this._data?.validation, mandatory: this._data?.validation.mandatory ?? false, regEx },
		});
	}
	#onValidationMessageChange(event: UUIInputEvent) {
		const regExMessage = event.target.value.toString();
		this.updateValue({
			validation: { ...this._data?.validation, mandatory: this._data?.validation.mandatory ?? false, regExMessage },
		});
	}

	#onVaryByCultureChange(event: UUIBooleanInputEvent) {
		const variesByCulture = event.target.checked;
		this.updateValue({
			variesByCulture,
		});
	}

	override render() {
		return this._data
			? html`
					<uui-box class="uui-text">
						<div class="container">
							<!-- TODO: Align styling across this and the property of document type workspace editor, or consider if this can go away for a different UX flow -->
							<uui-input
								id="name-input"
								name="name"
								label=${this.localize.term('placeholders_entername')}
								@input=${this.#onNameChange}
								.value=${this._data?.name}
								placeholder=${this.localize.term('placeholders_entername')}
								${umbFocus()}>
								<!-- TODO: validation for bad characters -->
							</uui-input>
							<uui-input
								id="alias-input"
								name="alias"
								@input=${this.#onAliasChange}
								.value=${this._data?.alias}
								label=${this.localize.term('placeholders_enterAlias')}
								placeholder=${this.localize.term('placeholders_enterAlias')}
								?disabled=${this._aliasLocked}>
								<!-- TODO: validation for bad characters -->
								<div @click=${this.#onToggleAliasLock} @keydown=${() => ''} id="alias-lock" slot="prepend">
									<uui-icon name=${this._aliasLocked ? 'icon-lock' : 'icon-unlocked'}></uui-icon>
								</div>
							</uui-input>
							<uui-textarea
								id="description-input"
								name="description"
								@input=${this.#onDescriptionChange}
								label=${this.localize.term('placeholders_enterDescription')}
								placeholder=${this.localize.term('placeholders_enterDescription')}
								.value=${this._data?.description}></uui-textarea>
						</div>
						<umb-data-type-flow-input
							.value=${this._data?.dataType?.unique ?? ''}
							@change=${this.#onDataTypeIdChange}></umb-data-type-flow-input>
						<hr />
						<div class="container">
							<b><umb-localize key="validation_validation">Validation</umb-localize></b>
							${this.#renderMandatory()}
							<p style="margin-bottom: 0">
								<umb-localize key="validation_customValidation">Custom validation</umb-localize>
							</p>
							${this.#renderCustomValidation()}
						</div>
						<hr />
						${this.#renderVariationControls()}
						<div class="container">
							<b style="margin-bottom: var(--uui-size-space-3)">
								<umb-localize key="contentTypeEditor_displaySettingsHeadline">Appearance</umb-localize>
							</b>
							<div id="appearances">${this.#renderAlignLeftIcon()} ${this.#renderAlignTopIcon()}</div>
						</div>
					</uui-box>
				`
			: '';
	}

	#renderAlignLeftIcon() {
		return html`<button
			type="button"
			@click=${this.#setAppearanceNormal}
			class="appearance left ${this._data?.appearance?.labelOnTop ? '' : 'selected'}">
			<svg width="200" height="48" viewBox="0 0 200 60" fill="none" xmlns="http://www.w3.org/2000/svg">
				<rect width="94" height="14" rx="6" fill="currentColor" />
				<rect y="22" width="64" height="9" rx="4" fill="currentColor" fill-opacity="0.4" />
				<rect x="106" width="94" height="60" rx="5" fill="currentColor" fill-opacity="0.4" />
			</svg>
			<label class="appearance-label">
				<umb-localize key="contentTypeEditor_displaySettingsLabelOnLeft">Label to the left</umb-localize>
			</label>
		</button>`;
	}

	#renderAlignTopIcon() {
		return html`
			<button
				type="button"
				@click=${this.#setAppearanceTop}
				class="appearance top ${this._data?.appearance?.labelOnTop ? 'selected' : ''}">
				<svg width="140" height="48" viewBox="0 0 140 60" fill="none" xmlns="http://www.w3.org/2000/svg">
					<rect width="90" height="14" rx="6" fill="currentColor" />
					<rect y="22" width="64" height="9" rx="4" fill="currentColor" fill-opacity="0.4" />
					<rect y="42" width="140" height="36" rx="5" fill="currentColor" fill-opacity="0.4" />
				</svg>
				<label class="appearance-label">
					<umb-localize key="contentTypeEditor_displaySettingsLabelOnTop">Label above (full-width)</umb-localize>
				</label>
			</button>
		`;
	}

	#renderMandatory() {
		return html`<div style="display: flex; justify-content: space-between">
				<label for="mandatory">
					<umb-localize key="validation_fieldIsMandatory">Field is mandatory</umb-localize>
				</label>
				<uui-toggle
					@change=${this.#onMandatoryChange}
					id="mandatory"
					.checked=${this._data?.validation?.mandatory ?? false}
					slot="editor"></uui-toggle>
			</div>
			${this._data?.validation?.mandatory
				? html`<uui-input
						name="mandatory-message"
						value=${this._data.validation?.mandatoryMessage ?? ''}
						@change=${this.#onMandatoryMessageChange}
						style="margin-top: var(--uui-size-space-1)"
						id="mandatory-message"
						placeholder=${this.localize.term('validation_mandatoryMessage')}
						label=${this.localize.term('validation_mandatoryMessage')}></uui-input>`
				: ''}`;
	}

	#renderCustomValidation() {
		return html`<uui-select
				style="margin-top: var(--uui-size-space-1)"
				@change=${this.#onCustomValidationChange}
				.options=${this._customValidationOptions}></uui-select>

			${this._data?.validation?.regEx !== null
				? html`
						<uui-input
							name="pattern"
							style="margin-bottom: var(--uui-size-space-1); margin-top: var(--uui-size-space-5);"
							@change=${this.#onValidationRegExChange}
							placeholder=${this.localize.term('validation_validationRegExp')}
							label=${this.localize.term('validation_validationRegExp')}
							.value=${this._data?.validation?.regEx ?? ''}></uui-input>
						<uui-textarea
							name="pattern-message"
							@change=${this.#onValidationMessageChange}
							placeholder=${this.localize.term('validation_validationRegExpMessage')}
							label=${this.localize.term('validation_validationRegExpMessage')}
							.value=${this._data?.validation?.regExMessage ?? ''}></uui-textarea>
					`
				: nothing} `;
	}

	#renderVariationControls() {
		return this._contentTypeVariesByCulture || this._contentTypeVariesBySegment
			? html` <div class="container">
						<b><umb-localize key="contentTypeEditor_variantsHeading">Allow variations</umb-localize></b>
						${this._contentTypeVariesByCulture ? this.#renderVaryByCulture() : ''}
					</div>
					<hr />`
			: '';
	}
	#renderVaryByCulture() {
		return html`<uui-toggle
			@change=${this.#onVaryByCultureChange}
			.checked=${this._data?.variesByCulture ?? false}
			label=${this.localize.term('contentTypeEditor_cultureVariantLabel')}></uui-toggle> `;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
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

export default UmbPropertyTypeWorkspaceViewSettingsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-type-workspace-view-settings': UmbPropertyTypeWorkspaceViewSettingsElement;
	}
}
