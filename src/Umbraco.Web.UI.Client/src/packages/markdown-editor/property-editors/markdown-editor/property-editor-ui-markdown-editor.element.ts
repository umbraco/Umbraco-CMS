import type { UmbInputMarkdownElement } from '../../components/input-markdown-editor/index.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';

import '../../components/input-markdown-editor/index.js';

const elementName = 'umb-property-editor-ui-markdown-editor';
/**
 * @element umb-property-editor-ui-markdown-editor
 */
@customElement(elementName)
export class UmbPropertyEditorUIMarkdownEditorElement extends UmbLitElement implements UmbPropertyEditorUiElement {
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
	private _preview?: boolean;

	@state()
	private _overlaySize: UUIModalSidebarSize = 'small';

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._preview = config.getValueByAlias('preview');
		this._overlaySize = config.getValueByAlias('overlaySize') ?? 'small';
	}

	#onChange(event: Event & { target: UmbInputMarkdownElement }) {
		this.value = event.target.value as string;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
			<umb-input-markdown
				value=${this.value}
				.overlaySize=${this._overlaySize}
				?preview=${this._preview}
				@change=${this.#onChange}
				?readonly=${this.readonly}></umb-input-markdown>
		`;
	}
}

export { UmbPropertyEditorUIMarkdownEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbPropertyEditorUIMarkdownEditorElement;
	}
}
