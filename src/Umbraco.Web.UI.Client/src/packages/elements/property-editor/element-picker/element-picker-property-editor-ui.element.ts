import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin, UMB_VALIDATION_EMPTY_LOCALIZATION_KEY } from '@umbraco-cms/backoffice/validation';
import type { UmbInputEntityDataElement } from '@umbraco-cms/backoffice/entity-data-picker';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';

import '@umbraco-cms/backoffice/entity-data-picker';

@customElement('umb-element-picker-property-editor-ui')
export class UmbElementPickerPropertyEditorUIElement
	extends UmbFormControlMixin<Array<string> | undefined, typeof UmbLitElement>(UmbLitElement, undefined)
	implements UmbPropertyEditorUiElement
{
	#dataSourceAlias = 'Umb.PropertyEditorDataSource.Element';

	@property({ type: Boolean })
	mandatory?: boolean;

	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_EMPTY_LOCALIZATION_KEY;

	@property({ type: String })
	name?: string;

	@property({ type: Boolean, reflect: true })
	readonly = false;

	public set config(config: UmbPropertyEditorUiElement['config'] | undefined) {
		if (!config) return;

		const minMax = config?.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this._min = minMax?.min ?? 0;
		this._max = minMax?.max ?? Infinity;

		this._minMessage = `${this.localize.term('validation_minCount')} ${this._min} ${this.localize.term('validation_items')}`;
		this._maxMessage = `${this.localize.term('validation_maxCount')} ${this._max} ${this.localize.term('validation_itemsSelected')}`;
	}

	@state()
	private _min = 0;

	@state()
	private _minMessage = '';

	@state()
	private _max = Infinity;

	@state()
	private _maxMessage = '';

	override focus() {
		return this.shadowRoot?.querySelector('umb-input-entity-data')?.focus();
	}

	override firstUpdated(changedProperties: Map<string | number | symbol, unknown>) {
		super.firstUpdated(changedProperties);

		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-entity-data')!);

		if (this._min && this._max && this._min > this._max) {
			console.warn(
				`Property (Element Picker) has been misconfigured, 'min' is greater than 'max'. Please correct your data type configuration.`,
				this,
			);
		}
	}

	#onChange(event: CustomEvent & { target: UmbInputEntityDataElement }) {
		this.value = event.target.selection;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-entity-data
				.selection=${this.value ?? []}
				.dataSourceAlias=${this.#dataSourceAlias}
				.dataSourceConfig=${[]}
				.min=${this._min}
				.min-message=${this._minMessage}
				.max=${this._max}
				.max-message=${this._maxMessage}
				?readonly=${this.readonly}
				@change=${this.#onChange}>
			</umb-input-entity-data>
		`;
	}
}

export { UmbElementPickerPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-picker-property-editor-ui': UmbElementPickerPropertyEditorUIElement;
	}
}
