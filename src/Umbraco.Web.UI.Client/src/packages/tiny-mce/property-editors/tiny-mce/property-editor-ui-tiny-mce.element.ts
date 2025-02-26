import type { UmbInputTinyMceElement } from '../../components/input-tiny-mce/input-tiny-mce.element.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorUiRteElementBase, UMB_BLOCK_RTE_DATA_CONTENT_KEY } from '@umbraco-cms/backoffice/rte';

import '../../components/input-tiny-mce/input-tiny-mce.element.js';

/**
 * @element umb-property-editor-ui-tiny-mce
 */
@customElement('umb-property-editor-ui-tiny-mce')
export class UmbPropertyEditorUITinyMceElement extends UmbPropertyEditorUiRteElementBase {
	#onChange(event: CustomEvent & { target: UmbInputTinyMceElement }) {
		const value = typeof event.target.value === 'string' ? event.target.value : '';

		// If we don't get any markup clear the property editor value.
		if (value === '') {
			this.value = undefined;
			this._fireChangeEvent();
			return;
		}

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

		if (super.value) {
			super.value = {
				...super.value,
				markup: markup,
			};
		} else {
			this.value = {
				markup: markup,
				blocks: {
					layout: {},
					contentData: [],
					settingsData: [],
					expose: [],
				},
			};
		}

		// lets run this one after we set the value, to make sure we don't reset the value.
		this._filterUnusedBlocks(usedContentKeys);

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
