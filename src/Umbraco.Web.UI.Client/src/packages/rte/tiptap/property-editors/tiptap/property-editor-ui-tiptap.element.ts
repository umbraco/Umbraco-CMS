import type { UmbInputTiptapElement } from '../../components/input-tiptap/input-tiptap.element.js';
import { UmbRteBaseElement } from '../../../components/rte-base.element.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

import '../../components/input-tiptap/input-tiptap.element.js';

const elementName = 'umb-property-editor-ui-tiptap';

/**
 * @element umb-property-editor-ui-tiptap
 */
@customElement(elementName)
export class UmbPropertyEditorUiTiptapElement extends UmbRteBaseElement {
	#onChange(event: CustomEvent & { target: UmbInputTiptapElement }) {
		const value = event.target.value;
		this._latestMarkup = value;

		this._value = {
			...this._value,
			markup: this._latestMarkup,
		};

		// Remove unused Blocks of Blocks Layout. Leaving only the Blocks that are present in Markup.
		const usedContentKeys: string[] = [];

		// Regex matching all block elements in the markup, and extracting the content key. It's the same as the one used on the backend.
		const regex = new RegExp(
			/<umb-rte-block(?:-inline)?(?: class="(?:.[^"]*)")? data-content-key="(?<key>.[^"]*)">(?:<!--Umbraco-Block-->)?<\/umb-rte-block(?:-inline)?>/gi,
		);
		let blockElement: RegExpExecArray | null;
		while ((blockElement = regex.exec(this._latestMarkup)) !== null) {
			if (blockElement.groups?.key) {
				usedContentKeys.push(blockElement.groups.key);
			}
		}

		this._filterUnusedBlocks(usedContentKeys);

		this._fireChangeEvent();
	}

	override render() {
		return html`
			<umb-input-tiptap
				.configuration=${this._config}
				.value=${this._markup}
				?readonly=${this.readonly}
				@change=${this.#onChange}></umb-input-tiptap>
		`;
	}
}

export { UmbPropertyEditorUiTiptapElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbPropertyEditorUiTiptapElement;
	}
}
