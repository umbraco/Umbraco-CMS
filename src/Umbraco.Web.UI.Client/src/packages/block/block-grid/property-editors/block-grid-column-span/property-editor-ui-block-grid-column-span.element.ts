import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-property-editor-ui-block-grid-column-span')
export class UmbPropertyEditorUIBlockGridColumnSpanElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	private _value: Array<string> = [];

	@property({ type: Array })
	public get value(): Array<string> {
		return this._value;
	}
	public set value(value: Array<string>) {
		this._value = value || [];
	}

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {}

	private _onChange(event: CustomEvent) {
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`Column span TODO `;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIBlockGridColumnSpanElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-column-span': UmbPropertyEditorUIBlockGridColumnSpanElement;
	}
}
