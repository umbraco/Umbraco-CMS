import type { UmbInputTiptapElement } from '../../components/input-tiptap/input-tiptap.element.js';
import { css, customElement, html, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorUiRteElementBase } from '@umbraco-cms/backoffice/rte';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';

import '../../components/input-tiptap/input-tiptap.element.js';

/**
 * @element umb-property-editor-ui-tiptap
 */
@customElement('umb-property-editor-ui-tiptap')
export class UmbPropertyEditorUiTiptapElement extends UmbPropertyEditorUiRteElementBase {
	protected override firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);

		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-tiptap') as UmbInputTiptapElement);
	}

	#onChange(event: CustomEvent & { target: UmbInputTiptapElement }) {
		const tipTapElement = event.target;
		const markup = tipTapElement.value;

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
			<umb-input-tiptap
				style=${styleMap(this._css)}
				.configuration=${this._config}
				.label=${this.name}
				.requiredMessage=${this.mandatoryMessage}
				.value=${this._markup}
				?readonly=${this.readonly}
				?required=${this.mandatory}
				@change=${this.#onChange}></umb-input-tiptap>
		`;
	}

	static override styles = css`
		:host(:invalid:not([pristine])) umb-input-tiptap {
			--umb-tiptap-edge-border-color: var(--uui-color-invalid);
		}
	`;
}

export { UmbPropertyEditorUiTiptapElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiptap': UmbPropertyEditorUiTiptapElement;
	}
}
