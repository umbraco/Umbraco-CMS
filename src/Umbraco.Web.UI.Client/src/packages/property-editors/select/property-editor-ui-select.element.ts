import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-property-editor-ui-select
 */
@customElement('umb-property-editor-ui-select')
export class UmbPropertyEditorUISelectElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value?: string = '';

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const items = config.getValueByAlias('items');

		if (Array.isArray(items) && items.length > 0) {
			this._options =
				typeof items[0] === 'string'
					? items.map((item) => ({ name: item, value: item, selected: item === this.value }))
					: items.map((item) => ({ name: item.name, value: item.value, selected: item.value === this.value }));
		}
	}

	@state()
	private _options: Array<Option> = [];

	#onChange(event: UUISelectEvent) {
		this.value = event.target.value as string;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`<uui-select .options=${this._options} @change=${this.#onChange}></uui-select>`;
	}
}

export default UmbPropertyEditorUISelectElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-select': UmbPropertyEditorUISelectElement;
	}
}
