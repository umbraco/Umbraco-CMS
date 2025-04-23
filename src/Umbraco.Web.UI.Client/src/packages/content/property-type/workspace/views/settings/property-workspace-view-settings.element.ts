import { UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT } from '../../../index.js';
import { css, html, customElement, state, nothing, query } from '@umbraco-cms/backoffice/external/lit';
import { generateAlias } from '@umbraco-cms/backoffice/utils';
import { umbBindToValidation } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_CONTENT_TYPE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content-type';
import type { UmbPropertyTypeScaffoldModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import type {
	UUIBooleanInputEvent,
	UUIInputEvent,
	UUIInputLockElement,
	UUISelectEvent,
} from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-property-type-workspace-view-settings')
export class UmbPropertyTypeWorkspaceViewSettingsElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#context?: typeof UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _customValidationOptions: Array<Option> = [
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
	private _data?: UmbPropertyTypeScaffoldModel;

	@state()
	private _aliasLocked = true;

	@state()
	private _autoGenerateAlias = true;

	@state()
	private _contentTypeVariesByCulture?: boolean;

	@state()
	private _contentTypeVariesBySegment?: boolean;

	@query('#alias-input')
	private _aliasInput!: UUIInputLockElement;

	@state()
	private _entityType?: string;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#context = instance;
			this.observe(
				instance.data,
				(data) => {
					if (!this._data && data?.alias) {
						// Initial. Loading existing property
						this._autoGenerateAlias = false;
					}
					this._data = data;
				},
				'observeData',
			);
		});

		this.consumeContext(UMB_CONTENT_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.observe(instance.variesByCulture, (variesByCulture) => (this._contentTypeVariesByCulture = variesByCulture));
			this.observe(instance.variesBySegment, (variesBySegment) => (this._contentTypeVariesBySegment = variesBySegment));
			this._entityType = instance.getEntityType();
		}).passContextAliasMatches();
	}

	updateValue(partialValue: Partial<UmbPropertyTypeScaffoldModel>) {
		this.#context?.updateData(partialValue);
	}

	#onNameChange(event: UUIInputEvent) {
		this.updateValue({ name: event.target.value.toString() });
		if (this._aliasLocked && this._autoGenerateAlias) {
			this.updateValue({ alias: generateAlias(this._data?.name ?? '') });
		}
	}

	#onAliasChange() {
		// TODO: Why can I not get the correct value via event? Is it an issue in uui library too?
		const alias = generateAlias(this._aliasInput.value.toString());
		this.updateValue({ alias });
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

	#onToggleShowOnMemberProfile(e: UUIBooleanInputEvent) {
		const memberCanEdit = this._data?.visibility?.memberCanEdit ?? false;
		this.updateValue({ visibility: { memberCanView: e.target.checked, memberCanEdit } });
	}

	#onToggleMemberCanEdit(e: UUIBooleanInputEvent) {
		const memberCanView = this._data?.visibility?.memberCanView ?? false;
		this.updateValue({ visibility: { memberCanEdit: e.target.checked, memberCanView } });
	}

	#onToggleIsSensitiveData(e: UUIBooleanInputEvent) {
		this.updateValue({ isSensitive: e.target.checked });
	}

	#onToggleAliasLock() {
		this._aliasLocked = !this._aliasLocked;
		if (this._aliasLocked && !this._data?.alias) {
			// Reenable auto-generate if alias is empty and locked.
			this._autoGenerateAlias = true;
		} else {
			this._autoGenerateAlias = false;
		}
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

	#onShareAcrossCulturesChange(event: UUIBooleanInputEvent) {
		const sharedAcrossCultures = event.target.checked;
		this.updateValue({ variesByCulture: !sharedAcrossCultures });
	}

	#onShareAcrossSegmentsChange(event: UUIBooleanInputEvent) {
		const sharedAcrossSegments = event.target.checked;
		this.updateValue({ variesBySegment: !sharedAcrossSegments });
	}

	override render() {
		if (!this._data) return;
		return html`
			<uui-box class="uui-text">
				<div class="container">
					<umb-form-validation-message>
						<uui-input
							id="name-input"
							data-mark="input:entity-name"
							name="name"
							label=${this.localize.term('placeholders_entername')}
							placeholder=${this.localize.term('placeholders_entername')}
							.value=${this._data?.name}
							@input=${this.#onNameChange}
							required
							${umbBindToValidation(this, '$.name')}
							${umbFocus()}>
							<!-- TODO: validation for bad characters -->
						</uui-input>
					</umb-form-validation-message>
					<umb-form-validation-message>
						<uui-input-lock
							id="alias-input"
							name="alias"
							label=${this.localize.term('placeholders_enterAlias')}
							placeholder=${this.localize.term('placeholders_enterAlias')}
							.value=${this._data?.alias}
							?locked=${this._aliasLocked}
							required
							${umbBindToValidation(this, '$.alias')}
							@input=${this.#onAliasChange}
							@lock-change=${this.#onToggleAliasLock}>
						</uui-input-lock>
					</umb-form-validation-message>
					<uui-textarea
						id="description-input"
						name="description"
						@input=${this.#onDescriptionChange}
						label=${this.localize.term('placeholders_enterDescription')}
						placeholder=${this.localize.term('placeholders_enterDescription')}
						.value=${this._data?.description}
						auto-height></uui-textarea>
				</div>
				<umb-form-validation-message>
					<umb-data-type-flow-input
						.value=${this._data?.dataType?.unique ?? ''}
						@change=${this.#onDataTypeIdChange}
						required
						${umbBindToValidation(this, '$.dataType.unique')}></umb-data-type-flow-input>
				</umb-form-validation-message>
				<hr />
				<div class="container">
					<b><umb-localize key="validation_validation">Validation</umb-localize></b>
					${this.#renderMandatory()}
					<p style="margin-bottom: 0">
						<umb-localize key="validation_customValidation">Custom validation</umb-localize>
					</p>
					${this.#renderCustomValidation()}
				</div>
				${this.#renderVariationControls()}
				<umb-property-layout label="#contentTypeEditor_displaySettingsHeadline" orientation="vertical">
					<div id="appearances" slot="editor">${this.#renderAlignLeftIcon()} ${this.#renderAlignTopIcon()}</div>
				</umb-property-layout>

				${this.#renderMemberTypeOptions()}
			</uui-box>
		`;
	}

	#renderMemberTypeOptions() {
		if (this._entityType !== 'member-type') return nothing;
		return html` <hr />
			<div class="container">
				<b style="margin-bottom: var(--uui-size-space-3)">
					<umb-localize key="general_options">Options</umb-localize>
				</b>
				<div class="options">
					<uui-toggle
						?checked=${this._data?.visibility?.memberCanView}
						@change=${this.#onToggleShowOnMemberProfile}
						label=${this.localize.term('contentTypeEditor_showOnMemberProfile')}></uui-toggle>
					<small>
						<umb-localize key="contentTypeEditor_showOnMemberProfileDescription">
							Allow this property value to be displayed on the member profile page
						</umb-localize>
					</small>

					<uui-toggle
						?checked=${this._data?.visibility?.memberCanEdit}
						@change=${this.#onToggleMemberCanEdit}
						label=${this.localize.term('contentTypeEditor_memberCanEdit')}></uui-toggle>
					<small>
						<umb-localize key="contentTypeEditor_memberCanEditDescription">
							Allow this property value to be edited by the member on their profile page
						</umb-localize>
					</small>

					<uui-toggle
						?checked=${this._data?.isSensitive}
						@change=${this.#onToggleIsSensitiveData}
						label=${this.localize.term('contentTypeEditor_isSensitiveData')}></uui-toggle>
					<small>
						<umb-localize key="contentTypeEditor_isSensitiveDataDescription">
							Hide this property value from content editors that don't have access to view sensitive information
						</umb-localize>
					</small>
				</div>
			</div>`;
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
			? html` <umb-property-layout label="#contentTypeEditor_variantsHeading" orientation="vertical">
					<umb-stack slot="editor" look="compact">
						${this._contentTypeVariesByCulture ? this.#renderVaryByCulture() : nothing}
						${this._contentTypeVariesBySegment ? this.#renderVaryBySegment() : nothing}
					</umb-stack>
				</umb-property-layout>`
			: '';
	}

	#renderVaryByCulture() {
		return html`
			<div>
				<uui-toggle
					@change=${this.#onShareAcrossCulturesChange}
					.checked=${!(this._data?.variesByCulture ?? false)}
					label="Shared across cultures"></uui-toggle>
			</div>
		`;
	}

	#renderVaryBySegment() {
		return html`
			<div>
				<uui-toggle
					@change=${this.#onShareAcrossSegmentsChange}
					.checked=${!(this._data?.variesBySegment ?? false)}
					label="Shared across segments"></uui-toggle>
			</div>
		`;
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
			uui-input:focus-within {
				z-index: 1;
			}
			uui-input-lock:focus-within {
				z-index: 1;
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
