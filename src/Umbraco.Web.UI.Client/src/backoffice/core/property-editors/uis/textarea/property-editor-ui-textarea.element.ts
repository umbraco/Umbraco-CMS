import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, query, state } from 'lit/decorators.js';
import { UUITextareaElement } from '@umbraco-ui/uui';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extensions-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DataTypePropertyPresentationModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-property-editor-ui-textarea')
export class UmbPropertyEditorUITextareaElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

	@state()
	private _maxChars?: number;

	@state()
	private _rows?: number;

	@property({ type: Array, attribute: false })
	public set config(config: Array<DataTypePropertyPresentationModel>) {
		const maxChars = config.find((x) => x.alias === 'maxChars');
		if (maxChars) this._maxChars = maxChars.value;

		const rows = config.find((x) => x.alias === 'rows');
		if (!rows) return;
		this._rows = rows.value;
	}

	private onInput(e: InputEvent) {
		this.value = (e.target as UUITextareaElement).value as string;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	//TODO: uui-textarea needs a rows property

	render() {
		return html` <uui-textarea
			label="Textarea"
			.value=${this.value}
			maxlength="${ifDefined(this._maxChars)}"
			rows="${ifDefined(this._rows)}"
			@input=${this.onInput}
			autoheight="${this._rows ? false : true}"></uui-textarea>`;
	}

	static styles = [
		UUITextStyles,
		css`
			uui-textarea {
				width: 100%;
			}
		`,
	];
}

export default UmbPropertyEditorUITextareaElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-textarea': UmbPropertyEditorUITextareaElement;
	}
}
