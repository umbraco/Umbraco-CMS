import type { UmbInputTinyMceElement } from '../../components/input-tiny-mce/input-tiny-mce.element.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorUiRteElementBase } from '@umbraco-cms/backoffice/rte';

import '../../components/input-tiny-mce/input-tiny-mce.element.js';

/**
 * @element umb-property-editor-ui-tiny-mce
 */
@customElement('umb-property-editor-ui-tiny-mce')
export class UmbPropertyEditorUITinyMceElement extends UmbPropertyEditorUiRteElementBase {
	#onChange(event: CustomEvent & { target: UmbInputTinyMceElement }) {
		const markup = typeof event.target.value === 'string' ? event.target.value : '';

		// If we don't get any markup clear the property editor value.
		if (markup === '') {
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
		while ((blockElement = regex.exec(markup)) !== null) {
			if (blockElement.groups?.key) {
				usedContentKeys.push(blockElement.groups.key);
			}
		}

		if (this.value) {
			this.value = {
				...this.value,
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
