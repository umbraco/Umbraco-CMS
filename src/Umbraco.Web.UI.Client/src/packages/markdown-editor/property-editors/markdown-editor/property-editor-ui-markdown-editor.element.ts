import type { UmbInputMarkdownElement } from '../../components/input-markdown-editor/index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';

import '../../components/input-markdown-editor/index.js';

/**
 * @element umb-property-editor-ui-markdown-editor
 */
@customElement('umb-property-editor-ui-markdown-editor')
export class UmbPropertyEditorUIMarkdownEditorElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = '';

	@state()
	private _preview?: boolean;

	@state()
	private _overlaySize?: UUIModalSidebarSize;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._preview = config?.getValueByAlias('preview');
		this._overlaySize = config?.getValueByAlias('overlaySize') ?? undefined;
	}

	#onChange(e: Event) {
		this.value = (e.target as UmbInputMarkdownElement).value as string;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<umb-input-markdown
			?preview=${this._preview}
			.overlaySize=${this._overlaySize}
			@change=${this.#onChange}
			.value=${this.value}></umb-input-markdown>`;
	}
}

export default UmbPropertyEditorUIMarkdownEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-markdown-editor': UmbPropertyEditorUIMarkdownEditorElement;
	}
}
