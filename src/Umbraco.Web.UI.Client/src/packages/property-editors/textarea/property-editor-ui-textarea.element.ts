import { css, customElement, html, property, state, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { StyleInfo } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UUITextareaElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-property-editor-ui-textarea')
export class UmbPropertyEditorUITextareaElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = '';

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	private _maxChars?: number;

	@state()
	private _rows?: number;

	@state()
	private _maxHeight?: number;

	@state()
	private _minHeight?: number;

	@state()
	private _css: StyleInfo = {};

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._maxChars = Number(config?.getValueByAlias('maxChars')) || undefined;
		this._rows = Number(config?.getValueByAlias('rows')) || undefined;
		this._minHeight = Number(config?.getValueByAlias('minHeight')) || undefined;
		this._maxHeight = Number(config?.getValueByAlias('maxHeight')) || undefined;

		this._css = {
			'--uui-textarea-min-height': this._minHeight ? `${this._minHeight}px` : 'reset',
			'--uui-textarea-max-height': this._maxHeight ? `${this._maxHeight}px` : 'reset',
		};
	}

	#onInput(event: InputEvent & { target: UUITextareaElement }) {
		this.value = event.target.value as string;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			<uui-textarea
				label="Textarea"
				style=${styleMap(this._css)}
				.autoHeight=${this._rows ? false : true}
				.maxlength=${this._maxChars}
				.rows=${this._rows}
				.value=${this.value ?? ''}
				@input=${this.#onInput}
				?readonly=${this.readonly}></uui-textarea>
		`;
	}

	static styles = [
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
