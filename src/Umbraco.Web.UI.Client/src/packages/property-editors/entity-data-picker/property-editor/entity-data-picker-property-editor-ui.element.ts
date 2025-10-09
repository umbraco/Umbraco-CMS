import type { UmbInputEntityDataElement } from '../input/input-entity-data.element.js';
import type { UmbEntityDataPickerPropertyEditorValue } from './types.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type {
	ManifestPropertyEditorDataSource,
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

// import of local component
import '../input/input-entity-data.element.js';
import type { UmbConfigCollectionModel } from '@umbraco-cms/backoffice/utils';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

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
		if (!config) {
			this.#propertyEditorConfigCollection = undefined;
			return;
		}

		this.#propertyEditorConfigCollection = config;

		this._min = this.#parseInt(config.getValueByAlias('minNumber'), 0);
		this._max = this.#parseInt(config.getValueByAlias('maxNumber'), Infinity);

		this._minMessage = `${this.localize.term('validation_minCount')} ${this._min} ${this.localize.term('validation_items')}`;
		this._maxMessage = `${this.localize.term('validation_maxCount')} ${this._max} ${this.localize.term('validation_itemsSelected')}`;

		// NOTE: Run validation immediately, to notify if the value is outside of min/max range. [LK]
		if (this._min > 0 || this._max < Infinity) {
			this.checkValidity();
		}

		this._dataSourceConfig = this.#extractDataSourceConfig();
	}

	#propertyEditorConfigCollection?: UmbPropertyEditorConfigCollection;

	#parseInt(value: unknown, fallback: number): number {
		const num = Number(value);
		return !isNaN(num) && num > 0 ? num : fallback;
	}

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

	#onChange(event: CustomEvent & { target: UmbInputEntityDataElement }) {
		this.value = event.target.selection;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`<umb-input-entity-data
			.selection=${this.value ?? []}
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
