import type { UmbInputEntityDataElement } from '../input/input-entity-data.element.js';
import type { UmbEntityDataPickerPropertyEditorValue } from './types.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

// import of local component
import '../input/input-entity-data.element.js';

@customElement('umb-entity-data-picker-property-editor-ui')
export class UmbEntityDataPickerPropertyEditorUIElement
	extends UmbFormControlMixin<UmbEntityDataPickerPropertyEditorValue | undefined, typeof UmbLitElement>(
		UmbLitElement,
		undefined,
	)
	implements UmbPropertyEditorUiElement
{
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

	#dataSourceAlias: string | null = null;
	#dataSourceManifest: UmbPropertyEditorDataSourceManifest | null = null;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this.#dataSourceAlias = config.getValueByAlias<string | null>('editorDataSourceAlias') ?? null;
		this.#getDataSourceManifest();

		this._min = this.#parseInt(config.getValueByAlias('minNumber'), 0);
		this._max = this.#parseInt(config.getValueByAlias('maxNumber'), Infinity);

		this._minMessage = `${this.localize.term('validation_minCount')} ${this._min} ${this.localize.term('validation_items')}`;
		this._maxMessage = `${this.localize.term('validation_maxCount')} ${this._max} ${this.localize.term('validation_itemsSelected')}`;

		// NOTE: Run validation immediately, to notify if the value is outside of min/max range. [LK]
		if (this._min > 0 || this._max < Infinity) {
			this.checkValidity();
		}
	}

	#getDataSourceManifest() {
		const manifest = umbExtensionsRegistry.getByAlias<UmbPropertyEditorDataSourceManifest>(this.#dataSourceAlias ?? '');
		debugger;
	}

	#parseInt(value: unknown, fallback: number): number {
		const num = Number(value);
		return !isNaN(num) && num > 0 ? num : fallback;
	}

	override focus() {
		return this.shadowRoot?.querySelector<UmbInputEntityDataElement>('umb-input-entity-data')?.focus();
	}

	override render() {
		return html`<umb-input-entity-data></umb-input-entity-data>`;
	}
}

export { UmbEntityDataPickerPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-data-property-editor-ui': UmbEntityDataPickerPropertyEditorUIElement;
	}
}
