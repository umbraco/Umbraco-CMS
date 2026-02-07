import type { UmbInputEntityDataElement } from '../input/input-entity-data.element.js';
import type { UmbEntityDataPickerPropertyEditorValue } from './types.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestPropertyEditorDataSource } from '@umbraco-cms/backoffice/property-editor-data-source';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';

@customElement('umb-entity-data-picker-property-editor-ui')
export class UmbEntityDataPickerPropertyEditorUIElement
	extends UmbFormControlMixin<UmbEntityDataPickerPropertyEditorValue | undefined, typeof UmbLitElement>(
		UmbLitElement,
		undefined,
	)
	implements UmbPropertyEditorUiElement
{
	/**
	 * The data source alias to use for this property editor.
	 * @type {string}
	 * @memberof UmbEntityDataPickerPropertyEditorUIElement
	 */
	@property({ type: String })
	private _dataSourceAlias?: string | undefined;
	public get dataSourceAlias(): string | undefined {
		return this._dataSourceAlias;
	}
	public set dataSourceAlias(value: string | undefined) {
		this._dataSourceAlias = value;
		this.#extractDataSourceConfig();
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _min = 0;

	@state()
	private _minMessage = '';

	@state()
	private _max = Infinity;

	@state()
	private _maxMessage = '';

	@state()
	private _dataSourceConfig?: UmbConfigCollectionModel;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.#propertyEditorConfigCollection = config;

		const minMax = config?.getValueByAlias<UmbNumberRangeValueType>('validationLimit');
		this._min = minMax?.min ?? 0;
		this._max = minMax?.max ?? Infinity;

		this._minMessage = `${this.localize.term('validation_minCount')} ${this._min} ${this.localize.term('validation_items')}`;
		this._maxMessage = `${this.localize.term('validation_maxCount')} ${this._max} ${this.localize.term('validation_itemsSelected')}`;

		this._dataSourceConfig = this.#extractDataSourceConfig();
	}

	#propertyEditorConfigCollection?: UmbPropertyEditorConfigCollection;

	#extractDataSourceConfig() {
		if (!this._dataSourceAlias || !this.#propertyEditorConfigCollection) {
			this._dataSourceConfig = undefined;
			return;
		}

		const dataSourceExtension = umbExtensionsRegistry.getByAlias<ManifestPropertyEditorDataSource>(
			this._dataSourceAlias,
		);

		if (!dataSourceExtension) {
			throw new Error(`Data source with alias ${this._dataSourceAlias} not found`);
		}

		const aliases = dataSourceExtension.meta?.settings?.properties.map((property) => property.alias);
		const configAliasMatch = this.#propertyEditorConfigCollection.filter((configEntry) =>
			aliases?.includes(configEntry.alias),
		);

		const dataSourceConfig: UmbConfigCollectionModel | undefined = configAliasMatch?.map((configEntry) => {
			return {
				alias: configEntry.alias,
				value: configEntry.value,
			};
		});

		return dataSourceConfig;
	}

	override focus() {
		return this.shadowRoot?.querySelector<UmbInputEntityDataElement>('umb-input-entity-data')?.focus();
	}

	override firstUpdated(changedProperties: Map<string | number | symbol, unknown>) {
		super.firstUpdated(changedProperties);
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-entity-data')!);

		if (this._min && this._max && this._min > this._max) {
			console.warn(
				`Property (Entity Data Picker) has been misconfigured, 'min' is greater than 'max'. Please correct your data type configuration.`,
				this,
			);
		}
	}

	#onChange(event: CustomEvent & { target: UmbInputEntityDataElement }) {
		const selection = event.target.selection;

		// Ensure the value is of the correct type before setting it.
		if (Array.isArray(selection)) {
			this.value = { ids: selection };
			this.dispatchEvent(new UmbChangeEvent());
		} else {
			throw new Error('Selection is not of type array. Cannot set property value.');
		}
	}

	override render() {
		return html`<umb-input-entity-data
			.selection=${this.value?.ids ?? []}
			.dataSourceAlias="${this._dataSourceAlias}"
			.dataSourceConfig=${this._dataSourceConfig}
			.min=${this._min}
			.min-message=${this._minMessage}
			.max=${this._max}
			.max-message=${this._maxMessage}
			?readonly=${this.readonly}
			@change=${this.#onChange}></umb-input-entity-data>`;
	}
}

export { UmbEntityDataPickerPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-data-property-editor-ui': UmbEntityDataPickerPropertyEditorUIElement;
	}
}
