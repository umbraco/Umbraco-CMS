// Needed to disable the import/no-duplicates rule, cause otherwise we do not get the custom element registered:
// eslint-disable-next-line import/no-duplicates
import type { UmbInputStaticFileElement } from '@umbraco-cms/backoffice/static-file';
// eslint-disable-next-line import/no-duplicates
import '@umbraco-cms/backoffice/static-file';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-property-editor-ui-block-grid-layout-stylesheet')
export class UmbPropertyEditorUIBlockGridLayoutStylesheetElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	private _value: Array<string> = [];

	@property({ type: Array })
	public set value(value: Array<string>) {
		this._value = value || [];
	}
	public get value(): Array<string> {
		return this._value;
	}

	private _pickableFilter = (item: any) => item.unique.endsWith('css');

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	private _onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputStaticFileElement).selection;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	// TODO: Implement mandatory?
	render() {
		return html`
			<umb-input-static-file
				@change=${this._onChange}
				.pickableFilter=${this._pickableFilter}
				.selection=${this._value}
				.min=${0}
				.max=${1}></umb-input-static-file>
			<br />
			<a href="/umbraco/backoffice/assets/css/umbraco-blockgridlayout.css">Link to default layout stylesheet</a>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIBlockGridLayoutStylesheetElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-layout-stylesheet': UmbPropertyEditorUIBlockGridLayoutStylesheetElement;
	}
}
