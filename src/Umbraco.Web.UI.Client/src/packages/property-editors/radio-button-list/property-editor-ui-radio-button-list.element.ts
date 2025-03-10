import type { UmbInputRadioButtonListElement, UmbRadioButtonItem } from '@umbraco-cms/backoffice/components';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-radio-button-list
 */
@customElement('umb-property-editor-ui-radio-button-list')
export class UmbPropertyEditorUIRadioButtonListElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value?: string = '';

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const items = config.getValueByAlias('items');

		if (Array.isArray(items) && items.length > 0) {
			this._list =
				typeof items[0] === 'string'
					? items.map((item) => ({ label: item, value: item }))
					: items.map((item) => ({ label: item.name, value: item.value }));

			// If selection includes a value that is not in the list, add it to the list
			if (this.value && !this._list.find((item) => item.value === this.value)) {
				this._list.push({ label: this.value, value: this.value, invalid: true });
			}
		}
	}

	@state()
	private _list: Array<UmbRadioButtonItem> = [];

	#onChange(event: CustomEvent & { target: UmbInputRadioButtonListElement }) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-radio-button-list
				.list=${this._list}
				.value=${this.value ?? ''}
				@change=${this.#onChange}
				?readonly=${this.readonly}></umb-input-radio-button-list>
		`;
	}
}

export default UmbPropertyEditorUIRadioButtonListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-radio-button-list': UmbPropertyEditorUIRadioButtonListElement;
	}
}
