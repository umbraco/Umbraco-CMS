import type { UmbInputRadioButtonListElement } from '@umbraco-cms/backoffice/components';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

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
		}
	}

	@state()
	private _list: UmbInputRadioButtonListElement['list'] = [];

	#onChange(event: CustomEvent & { target: UmbInputRadioButtonListElement }) {
		this.value = event.target.value;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
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
