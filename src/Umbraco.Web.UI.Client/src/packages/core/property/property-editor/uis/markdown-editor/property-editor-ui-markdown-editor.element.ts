import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbPropertyEditorConfigCollection } from 'src/packages/core/property/property-editor';
import { UmbInputMarkdownElement } from '@umbraco-cms/backoffice/components';
import { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';

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

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._preview = config?.getValueByAlias('preview');
		this._overlaySize = config?.getValueByAlias('overlaySize') ?? undefined;
	}

	#onChange(e: Event) {
		this.value = (e.target as UmbInputMarkdownElement).value as string;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-markdown
			?preview=${this._preview}
			.overlaySize=${this._overlaySize}
			@change=${this.#onChange}
			.value=${this.value}></umb-input-markdown>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIMarkdownEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-markdown-editor': UmbPropertyEditorUIMarkdownEditorElement;
	}
}
