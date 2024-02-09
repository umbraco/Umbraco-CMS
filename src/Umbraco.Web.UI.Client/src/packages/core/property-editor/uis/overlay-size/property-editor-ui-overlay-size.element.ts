import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UUIModalSidebarSize, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

/**
 * @element umb-property-editor-ui-overlay-size
 */
@customElement('umb-property-editor-ui-overlay-size')
export class UmbPropertyEditorUIOverlaySizeElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value: UUIModalSidebarSize | string = '';

	@state()
	private _list: Array<Option> = [
		{ value: 'small', name: 'Small', selected: true },
		{ value: 'medium', name: 'Medium' },
		{ value: 'large', name: 'Large' },
		{ value: 'full', name: 'Full' },
	];

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	firstUpdated() {
		if (!this.value) return;
		this._list = this._list.map((option) => ({
			...option,
			selected: option.value === this.value,
		}));
	}

	#onChange(event: UUISelectEvent) {
		this.value = event.target.value as string;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<uui-select .options=${this._list} @change=${this.#onChange}></uui-select>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIOverlaySizeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-overlay-size': UmbPropertyEditorUIOverlaySizeElement;
	}
}
