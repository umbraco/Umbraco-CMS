import type { UmbInputTinyMceElement } from '../../components/input-tiny-mce/input-tiny-mce.element.js';
import { UmbPropertyEditorUiRteElementBase, UMB_BLOCK_RTE_DATA_CONTENT_KEY } from '@umbraco-cms/backoffice/rte';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

import '../../components/input-tiny-mce/input-tiny-mce.element.js';

/**
 * @element umb-property-editor-ui-tiny-mce
 */
@customElement('umb-property-editor-ui-tiny-mce')
export class UmbPropertyEditorUITinyMceElement extends UmbPropertyEditorUiRteElementBase {
	#onChange(event: CustomEvent & { target: UmbInputTinyMceElement }) {
		const value = typeof event.target.value === 'string' ? event.target.value : '';

		// Clone the DOM, to remove the classes and attributes on the original:
		const div = document.createElement('div');
		div.innerHTML = value;

		// Loop through used, to remove the classes on these.
		const blockEls = div.querySelectorAll(`umb-rte-block, umb-rte-block-inline`);
		blockEls.forEach((blockEl) => {
			blockEl.removeAttribute('contenteditable');
			blockEl.removeAttribute('class');
		});

		const markup = div.innerHTML;

		// Remove unused Blocks of Blocks Layout. Leaving only the Blocks that are present in Markup.
		//const blockElements = editor.dom.select(`umb-rte-block, umb-rte-block-inline`);
		const usedContentKeys = Array.from(blockEls).map((blockElement) =>
			blockElement.getAttribute(UMB_BLOCK_RTE_DATA_CONTENT_KEY),
		);

		this._filterUnusedBlocks(usedContentKeys);

		// Then get the content of the editor and update the value.
		// maybe in this way doc.body.innerHTML;

		this._latestMarkup = markup;

		this._value = {
			...this._value,
			markup: markup,
		};

		this._fireChangeEvent();
	}

	override render() {
		return html`
			<umb-input-tiny-mce
				.configuration=${this._config}
				.value=${this._markup}
				@change=${this.#onChange}
				?readonly=${this.readonly}>
			</umb-input-tiny-mce>
		`;
	}
}

export default UmbPropertyEditorUITinyMceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce': UmbPropertyEditorUITinyMceElement;
	}
}
