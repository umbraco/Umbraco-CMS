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

		// Remove unused Blocks of Blocks Layout. Leaving only the Blocks that are present in Markup.
		// Extract the layout key (data-key) from each block element to uniquely identify layout entries.
		// For legacy markup without data-key, fall back to data-content-key — safe because
		// setLayouts() coerces layout.key ??= layout.contentKey for persisted data without a key.
		const usedLayoutKeys: string[] = [];

		const blockRegex = /<umb-rte-block(?:-inline)?(?:[^>]*)>/gi;
		let blockElement: RegExpExecArray | null;
		while ((blockElement = blockRegex.exec(markup)) !== null) {
			const tag = blockElement[0];
			const layoutKey = tag.match(/ data-key="([^"]+)"/)?.[1] ?? tag.match(/ data-content-key="([^"]+)"/)?.[1];
			if (layoutKey) {
				usedLayoutKeys.push(layoutKey);
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
		this._filterUnusedBlocks(usedLayoutKeys);

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
