import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUITextareaElement } from '@umbraco-cms/backoffice/external/uui';
import { ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';

@customElement('umb-property-editor-ui-textarea')
export class UmbPropertyEditorUITextareaElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

	@state()
	private _maxChars?: number;

	@state()
	private _rows?: number;

	@state()
	private _maxHeight?: number;

	@state()
	private _minHeight?: number;

	@state()
	private _css?: any;

	@property({ type: Array, attribute: false })
	public set config(config: UmbDataTypePropertyCollection) {
		this._maxChars = config.getValueByAlias('maxChars');
		this._rows = config.getValueByAlias('rows');
		this._minHeight = config.getValueByAlias('minHeight');
		this._maxHeight = config.getValueByAlias('maxHeight');

		this._css = {
			'--uui-textarea-min-height': `${this._minHeight ? `${this._minHeight}px` : 'reset'}`,
			'--uui-textarea-max-height': `${this._maxHeight ? `${this._maxHeight}px` : 'reset'}`,
		};
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
			style="${styleMap(this._css)}"
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
