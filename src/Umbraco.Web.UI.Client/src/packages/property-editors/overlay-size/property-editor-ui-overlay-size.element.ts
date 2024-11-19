import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import {
	type UmbPropertyEditorUiElement,
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIModalSidebarSize, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-property-editor-ui-overlay-size
 */
@customElement('umb-property-editor-ui-overlay-size')
export class UmbPropertyEditorUIOverlaySizeElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value: UUIModalSidebarSize | string = '';

	// TODO: Stop having global type of the UUI-SELECT element. And make it possible to have undefined as an option.
	@state()
	private _list: Array<Option> = [
		{ value: undefined as any, name: 'Default', selected: true },
		{ value: 'small', name: 'Small' },
		{ value: 'medium', name: 'Medium' },
		{ value: 'large', name: 'Large' },
		{ value: 'full', name: 'Full' },
	];

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	override firstUpdated() {
		if (!this.value) return;
		this._list = this._list.map((option) => ({
			...option,
			selected: option.value === this.value,
		}));
	}

	#onChange(event: UUISelectEvent) {
		this.value = event.target.value as string;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`<uui-select .options=${this._list} @change=${this.#onChange}></uui-select>`;
	}
}

export default UmbPropertyEditorUIOverlaySizeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-overlay-size': UmbPropertyEditorUIOverlaySizeElement;
	}
}
