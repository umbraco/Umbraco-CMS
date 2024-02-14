import { css, html, customElement, property, state, ifDefined, styleMap } from '@umbraco-cms/backoffice/external/lit';
//import type { UUICodeEditorElement } from '@umbraco-cms/backoffice/external/uui';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-code-editor')
export class UmbPropertyEditorUICodeEditorElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = '';

	@state()
	private _language?: string;

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._language = config?.getValueByAlias('language');
	}
	
	#onInput(event: Event) {
		//this.value = (event.target as UmbCodeEditorElement).code as string;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-code-editor
			language="${this._language}"
			id="content"
			.code=${this.value ?? ''}
			@input=${this.#onInput}></umb-code-editor>`;
	}
}

export default UmbPropertyEditorUICodeEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-code-editor': UmbPropertyEditorUICodeEditorElement;
	}
}
