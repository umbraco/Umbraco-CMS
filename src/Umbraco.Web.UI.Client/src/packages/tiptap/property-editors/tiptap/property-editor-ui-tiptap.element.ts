import type { UmbInputTiptapElement } from '../../components/input-tiptap/input-tiptap.element.js';
import { UmbPropertyEditorUiRteElementBase } from '@umbraco-cms/backoffice/rte';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

import '../../components/input-tiptap/input-tiptap.element.js';

/**
 * @element umb-property-editor-ui-tiptap
 */
@customElement('umb-property-editor-ui-tiptap')
export class UmbPropertyEditorUiTiptapElement extends UmbPropertyEditorUiRteElementBase {
	#onChange(event: CustomEvent & { target: UmbInputTiptapElement }) {
		const tipTapElement = event.target;
		const value = tipTapElement.value;

		// If we don't get any markup clear the property editor value.
		if (tipTapElement.isEmpty()) {
			this.value = undefined;
			this._fireChangeEvent();
			return;
		}

		// Remove unused Blocks of Blocks Layout. Leaving only the Blocks that are present in Markup.
		const usedContentKeys: string[] = [];

		// Regex matching all block elements in the markup, and extracting the content key. It's the same as the one used on the backend.
		const regex = new RegExp(
			/<umb-rte-block(?:-inline)?(?: class="(?:.[^"]*)")? data-content-key="(?<key>.[^"]*)">(?:<!--Umbraco-Block-->)?<\/umb-rte-block(?:-inline)?>/gi,
		);
		let blockElement: RegExpExecArray | null;
		while ((blockElement = regex.exec(value)) !== null) {
			if (blockElement.groups?.key) {
				usedContentKeys.push(blockElement.groups.key);
			}
		}

		this._filterUnusedBlocks(usedContentKeys);

		this._latestMarkup = value;

		if (this.value) {
			this.value = {
				...this.value,
				markup: this._latestMarkup,
			};
		} else {
			this.value = {
				markup: this._latestMarkup,
				blocks: {
					layout: {},
					contentData: [],
					settingsData: [],
					expose: [],
				},
			};
		}

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
		'umb-property-editor-ui-tiptap': UmbPropertyEditorUiTiptapElement;
	}
}
