import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-block-list
 */
@customElement('umb-property-editor-ui-block-list')
export class UmbPropertyEditorUIBlockListElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	private _value: Array<string> = [];

	@property({ type: Array })
	public get value(): Array<string> {
		return this._value;
	}
	public set value(value: Array<string>) {
		this._value = value || [];
	}

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		const validationLimit = config['validationLimit'];

		this._limitMin = (validationLimit?.value as any)?.min;
		this._limitMax = (validationLimit?.value as any)?.max;

		//config.blocks
		//config.useSingleBlockMode
		//config.useLiveEditing
		//config.useInlineEditingAsDefault
		this._maxPropertyWidth = config.maxPropertyWidth;
	}

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;
	@state()
	private _maxPropertyWidth?: string;

	render() {
		return html`<div>umb-property-editor-ui-block-list</div>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIBlockListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-list': UmbPropertyEditorUIBlockListElement;
	}
}
