import { css, html, customElement, property, state, ifDefined, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { monaco } from '@umbraco-cms/backoffice/external/monaco-editor';
import type { UmbCodeEditorController, UmbCodeEditorElement } from '@umbraco-cms/backoffice/code-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

@customElement('umb-property-editor-ui-code-editor')
export class UmbPropertyEditorUICodeEditorElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = '';

	@state()
	private _language?: string = 'HTML';

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._language = config?.getValueByAlias('language');
	}
	
	#onChange(e: Event) {
		this.value = (e.target as UmbInputCodeEditorElement).value as string;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-code-editor
			.language="${this._language}"
			.value=${this.value ?? ''}
			@change=${this.#onChange}></umb-input-code-editor>`;
	}
}

export default UmbPropertyEditorUICodeEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-code-editor': UmbPropertyEditorUICodeEditorElement;
	}
}
