import { customElement, html, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-label
 */
@customElement('umb-property-editor-ui-label')
export class UmbPropertyEditorUILabelElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@state()
	private _labelTemplate?: string;

	@property()
	value = '';

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._labelTemplate = config?.getValueByAlias('labelTemplate');
	}

	override render() {
		return when(
			this._labelTemplate?.length,
			() => html`<umb-ufm-render inline .markdown=${this._labelTemplate} .value=${this.value}></umb-ufm-render>`,
			() => html`${this.value ?? ''}`,
		);
	}
}

export default UmbPropertyEditorUILabelElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-label': UmbPropertyEditorUILabelElement;
	}
}
