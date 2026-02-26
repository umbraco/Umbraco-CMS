import { UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT } from '../../../index.js';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY, umbBindToValidation } from '@umbraco-cms/backoffice/validation';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_CONTENT_TYPE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content-type';
import type { UmbPropertyTypeScaffoldModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import type { UUIBooleanInputEvent, UUIInputEvent, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbInputWithAliasElement } from '@umbraco-cms/backoffice/components';

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
	private _contentTypeVariesByCulture?: boolean;

	@state()
	private _contentTypeVariesBySegment?: boolean;

	@state()
	private _entityType?: string;

	@state()
	private _isNew?: boolean;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.#context = instance;
			this.observe(instance?.data, (data) => (this._data = data), 'observeData');
			this.observe(instance?.isNew, (isNew) => (this._isNew = isNew), '_observeIsNew');
		});

		this.consumeContext(UMB_CONTENT_TYPE_WORKSPACE_CONTEXT, (instance) => {
			this.observe(
				instance?.variesByCulture,
				(variesByCulture) => (this._contentTypeVariesByCulture = variesByCulture),
				'observeVariesByCulture',
			);
			this.observe(
				instance?.variesBySegment,
				(variesBySegment) => (this._contentTypeVariesBySegment = variesBySegment),
				'observeVariesBySegment',
			);
			this._entityType = instance?.getEntityType();
		}).passContextAliasMatches();
	}

	updateValue(partialValue: Partial<UmbPropertyTypeScaffoldModel>) {
		this.#context?.updateData(partialValue);
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

	#onNameAndAliasChange(event: InputEvent & { target: UmbInputWithAliasElement }) {
		const name = event.target.value ?? '';
		const alias = event.target.alias ?? '';
		this.updateValue({ name, alias });
	}

	override render() {
		if (!this._data) return;
		return html`
			<uui-box class="uui-text">
				<umb-property-layout label=${this.localize.term('general_name')} orientation="vertical" mandatory>
					<umb-input-with-alias
						id="name-input"
						data-mark="input:entity-name"
						name="name"
						slot="editor"
						required
						.value=${this._data?.name ?? ''}
						.alias=${this._data?.alias ?? ''}
						?auto-generate-alias=${this._isNew}
						@change=${this.#onNameAndAliasChange}
						${umbBindToValidation(this, '$.name', this._data?.name)}
						${umbFocus()}>
					</umb-input-with-alias>
				</umb-property-layout>

				<umb-property-layout label=${this.localize.term('general_description')} orientation="vertical">
					<uui-textarea
						id="description-input"
						data-mark="input:entity-description"
						slot="editor"
						name="description"
						@input=${this.#onDescriptionChange}
						.value=${this._data?.description ?? ''}
						auto-height></uui-textarea>
				</umb-property-layout>

				<umb-property-layout label=${this.localize.term('general_propertyEditor')} orientation="vertical" mandatory>
					<umb-data-type-flow-input
						slot="editor"
						id="data-type-input"
						.value=${this._data?.dataType?.unique ?? ''}
						@change=${this.#onDataTypeIdChange}
						required
						${umbBindToValidation(this, '$.dataType.unique')}></umb-data-type-flow-input>
				</umb-property-layout>
			</uui-box>

			<uui-box class="uui-text">
				<umb-localize key="validation_validation" slot="headline">Validation</umb-localize>
				${this.#renderMandatory()} ${this.#renderCustomValidation()}</uui-box
			>

			${this.#renderVariationControls()}

			<uui-box class="uui-text">
				<umb-localize key="contentTypeEditor_displaySettingsHeadline" slot="headline">Appearance</umb-localize>
				<umb-property-layout orientation="vertical">
					<div id="appearances" slot="editor">${this.#renderAlignLeftIcon()} ${this.#renderAlignTopIcon()}</div>
				</umb-property-layout>
			</uui-box>

			${this.#renderMemberTypeOptions()}
		`;
	}

	#renderMemberTypeOptions() {
		if (this._entityType !== 'member-type') return nothing;
		return html` <uui-box class="uui-text">
			<div class="options">
				<umb-property-layout orientation="vertical">
					<uui-toggle
						slot="editor"
						?checked=${this._data?.visibility?.memberCanView}
						@change=${this.#onToggleShowOnMemberProfile}
						label=${this.localize.term('contentTypeEditor_showOnMemberProfile')}></uui-toggle>
					<small slot="description">
						<umb-localize key="contentTypeEditor_showOnMemberProfileDescription">
							Allow this property value to be displayed on the member profile page
						</umb-localize>
					</small>
				</umb-property-layout>

				<umb-property-layout orientation="vertical">
					<uui-toggle
						slot="editor"
						?checked=${this._data?.visibility?.memberCanEdit}
						@change=${this.#onToggleMemberCanEdit}
						label=${this.localize.term('contentTypeEditor_memberCanEdit')}></uui-toggle>
					<small slot="description">
						<umb-localize key="contentTypeEditor_memberCanEditDescription">
							Allow this property value to be edited by the member on their profile page
						</umb-localize>
					</small>
				</umb-property-layout>

				<umb-property-layout orientation="vertical">
					<uui-toggle
						slot="editor"
						?checked=${this._data?.isSensitive}
						@change=${this.#onToggleIsSensitiveData}
						label=${this.localize.term('contentTypeEditor_isSensitiveData')}></uui-toggle>
					<small slot="description">
						<umb-localize key="contentTypeEditor_isSensitiveDataDescription">
							Hide this property value from content editors that don't have access to view sensitive information
						</umb-localize>
					</small>
				</umb-property-layout>
			</div>
		</uui-box>`;
	}

	#renderAlignLeftIcon() {
		return html`<div class="appearance-option">
			<button
				type="button"
				@click=${this.#setAppearanceNormal}
				class="appearance left ${this._data?.appearance?.labelOnTop ? '' : 'selected'}">
				<svg width="200" height="48" viewBox="0 0 200 60" fill="none" xmlns="http://www.w3.org/2000/svg">
					<rect width="94" height="14" rx="6" fill="currentColor" />
					<rect y="22" width="64" height="9" rx="4" fill="currentColor" fill-opacity="0.4" />
					<rect x="106" width="94" height="60" rx="5" stroke="currentColor" fill-opacity="0.4" />
				</svg></button
			><label class="appearance-label">
				<umb-localize key="contentTypeEditor_displaySettingsLabelOnLeft">Label to the left</umb-localize>
			</label>
		</div>`;
	}

	#renderAlignTopIcon() {
		return html`<div class="appearance-option">
			<button
				type="button"
				@click=${this.#setAppearanceTop}
				class="appearance top ${this._data?.appearance?.labelOnTop ? 'selected' : ''}">
				<svg width="140" height="48" viewBox="0 0 140 60" fill="none" xmlns="http://www.w3.org/2000/svg">
					<rect width="90" height="14" rx="6" fill="currentColor" />
					<rect y="22" width="64" height="9" rx="4" fill="currentColor" fill-opacity="0.4" />
					<rect y="42" width="140" height="36" rx="5" stroke="currentColor" fill-opacity="0.4" />
				</svg></button
			><label class="appearance-label">
				<umb-localize key="contentTypeEditor_displaySettingsLabelOnTop">Label above (full-width)</umb-localize>
			</label>
		</div> `;
	}

	#renderMandatory() {
		return html`<umb-property-layout orientation="vertical">
				<uui-toggle
					@change=${this.#onMandatoryChange}
					id="mandatory"
					.checked=${this._data?.validation?.mandatory ?? false}
					slot="editor"
					><umb-localize key="validation_fieldIsMandatory">Field is mandatory</umb-localize></uui-toggle
				></umb-property-layout
			>

			${this._data?.validation?.mandatory
				? html`<umb-property-layout label="#validation_mandatoryMessageLabel" orientation="vertical"
						><uui-input
							name="mandatory-message"
							slot="editor"
							value=${this._data.validation?.mandatoryMessage ?? ''}
							@change=${this.#onMandatoryMessageChange}
							style="margin-top: var(--uui-size-space-1)"
							id="mandatory-message"
							placeholder=${this.localize.string(UMB_VALIDATION_EMPTY_LOCALIZATION_KEY)}
							label=${this.localize.term('validation_mandatoryMessage')}></uui-input
					></umb-property-layout>`
				: ''} `;
	}

	#renderCustomValidation() {
		return html`<umb-property-layout orientation="vertical" label=${this.localize.term('validation_customValidation')}
				><uui-select
					slot="editor"
					@change=${this.#onCustomValidationChange}
					.options=${this._customValidationOptions}></uui-select
			></umb-property-layout>

			${this._data?.validation?.regEx !== null
				? html`<umb-property-layout orientation="vertical" label=${this.localize.term('validation_validationPattern')}>
							<uui-input
								name="pattern"
								slot="editor"
								@change=${this.#onValidationRegExChange}
								placeholder=${this.localize.term('validation_validationRegExp')}
								label=${this.localize.term('validation_validationRegExp')}
								.value=${this._data?.validation?.regEx ?? ''}></uui-input
						></umb-property-layout>
						<umb-property-layout label="#validation_mandatoryMessageLabel" orientation="vertical"
							><uui-textarea
								name="pattern-message"
								slot="editor"
								@change=${this.#onValidationMessageChange}
								placeholder=${this.localize.term('validation_invalidPattern')}
								label=${this.localize.term('validation_validationRegExpMessage')}
								.value=${this._data?.validation?.regExMessage ?? ''}></uui-textarea
						></umb-property-layout> `
				: nothing}`;
	}

	#renderVariationControls() {
		return this._contentTypeVariesByCulture || this._contentTypeVariesBySegment
			? html` <uui-box class="uui-text">
					<umb-localize key="contentTypeEditor_variantsHeading" slot="headline">Variation</umb-localize
					><umb-property-layout orientation="vertical">
						<umb-stack slot="editor" look="compact">
							${this._contentTypeVariesByCulture ? this.#renderVaryByCulture() : nothing}
							${this._contentTypeVariesBySegment ? this.#renderVaryBySegment() : nothing}
						</umb-stack>
					</umb-property-layout></uui-box
				>`
			: '';
	}

	#renderVaryByCulture() {
		return html`
			<div>
				<uui-toggle
					@change=${this.#onShareAcrossCulturesChange}
					.checked=${!(this._data?.variesByCulture ?? false)}
					label=${this.localize.term('contentTypeEditor_cultureInvariantLabel')}></uui-toggle>
			</div>
		`;
	}

	#renderVaryBySegment() {
		return html`
			<div>
				<uui-toggle
					@change=${this.#onShareAcrossSegmentsChange}
					.checked=${!(this._data?.variesBySegment ?? false)}
					label=${this.localize.term('contentTypeEditor_segmentInvariantLabel')}></uui-toggle>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-layout-1);
				padding: var(--uui-size-layout-1);
			}
			umb-property-layout[orientation='vertical'] {
				padding: var(--uui-size-space-2) 0;
			}

			umb-property-layout:first-of-type {
				padding-top: 0;
			}
			umb-property-layout:last-of-type {
				padding-bottom: 0;
			}

			uui-select {
				width: 100%;
			}

			#appearances {
				display: flex;
				gap: var(--uui-size-space-4);
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

			.appearance-option {
				display: flex;
				width: 100%;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}

			.appearance-label {
				font-size: 0.8rem;
				line-height: 1;
				text-align: center;
				pointer-events: none;
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

			uui-input {
				width: 100%;
			}

			uui-input:focus-within {
				z-index: 1;
			}

			.container {
				display: flex;
				flex-direction: column;
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
