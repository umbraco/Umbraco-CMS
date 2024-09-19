import type UmbInputTiptapElement from '../../components/input-tiptap/input-tiptap.element.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

import '../../components/input-tiptap/input-tiptap.element.js';
import {
	UmbBlockRteEntriesContext,
	UmbBlockRteManagerContext,
	type UmbBlockRteLayoutModel,
} from '@umbraco-cms/backoffice/block-rte';
import type { UmbBlockValueType } from '@umbraco-cms/backoffice/block';

// Look at Tiny for correct types
export interface UmbRichTextEditorValueType {
	markup: string;
	blocks: UmbBlockValueType<UmbBlockRteLayoutModel>;
}

const UMB_BLOCK_RTE_BLOCK_LAYOUT_ALIAS = 'Umbraco.RichText';

/**
 * @element umb-property-editor-ui-tiptap
 */
@customElement('umb-property-editor-ui-tiptap')
export class UmbPropertyEditorUITiptapElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this._config = config;
	}

	@property({
		attribute: false,
		type: Object,
		hasChanged(value?: UmbRichTextEditorValueType, oldValue?: UmbRichTextEditorValueType) {
			return value?.markup !== oldValue?.markup;
		},
	})
	public set value(value: UmbRichTextEditorValueType | undefined) {
		const buildUpValue: Partial<UmbRichTextEditorValueType> = value ? { ...value } : {};
		buildUpValue.markup ??= '';
		buildUpValue.blocks ??= { layout: {}, contentData: [], settingsData: [] };
		buildUpValue.blocks.layout ??= {};
		buildUpValue.blocks.contentData ??= [];
		buildUpValue.blocks.settingsData ??= [];
		this._value = buildUpValue as UmbRichTextEditorValueType;

		// Only update the actual editor markup if it is not the same as the value.
		if (this._latestMarkup !== buildUpValue.markup) {
			this._markup = buildUpValue.markup;
		}

		this.#managerContext.setLayouts(buildUpValue.blocks.layout[UMB_BLOCK_RTE_BLOCK_LAYOUT_ALIAS] ?? []);
		this.#managerContext.setContents(buildUpValue.blocks.contentData);
		this.#managerContext.setSettings(buildUpValue.blocks.settingsData);
	}
	public get value(): UmbRichTextEditorValueType {
		return this._value;
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	_config?: UmbPropertyEditorConfigCollection;

	@state()
	private _value: UmbRichTextEditorValueType = {
		markup: '',
		blocks: { layout: {}, contentData: [], settingsData: [] },
	};

	@state()
	private _markup = '';
	private _latestMarkup = '';

	#managerContext = new UmbBlockRteManagerContext(this);
	#entriesContext = new UmbBlockRteEntriesContext(this);

	constructor() {
		super();

		this.observe(this.#entriesContext.layoutEntries, (layouts) => {
			// Update manager:
			this.#managerContext.setLayouts(layouts);
		});

		this.observe(this.#managerContext.layouts, (layouts) => {
			this._value = {
				...this._value,
				blocks: { ...this._value.blocks, layout: { [UMB_BLOCK_RTE_BLOCK_LAYOUT_ALIAS]: layouts } },
			};
			this.#fireChangeEvent();
		});
		this.observe(this.#managerContext.contents, (contents) => {
			this._value = { ...this._value, blocks: { ...this._value.blocks, contentData: contents } };
			this.#fireChangeEvent();
		});
		this.observe(this.#managerContext.settings, (settings) => {
			this._value = { ...this._value, blocks: { ...this._value.blocks, settingsData: settings } };
			this.#fireChangeEvent();
		});
	}

	#fireChangeEvent() {
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onChange(event: CustomEvent & { target: UmbInputTiptapElement }) {
		const value = event.target.value as string;

		this._latestMarkup = value;

		this._value = {
			...this._value,
			markup: this._latestMarkup,
		};

		// TODO: Validate blocks
		// Loop through used, to remove the classes on these.
		/*const blockEls = div.querySelectorAll(`umb-rte-block, umb-rte-block-inline`);
		blockEls.forEach((blockEl) => {
			blockEl.removeAttribute('contenteditable');
			blockEl.removeAttribute('class');
		});

		// Remove unused Blocks of Blocks Layout. Leaving only the Blocks that are present in Markup.
		//const blockElements = editor.dom.select(`umb-rte-block, umb-rte-block-inline`);
		const usedContentUdis = Array.from(blockEls).map((blockElement) => blockElement.getAttribute('data-content-udi'));
		const unusedBlocks = this.#managerContext.getLayouts().filter((x) => usedContentUdis.indexOf(x.contentUdi) === -1);
		unusedBlocks.forEach((blockLayout) => {
			this.#managerContext.removeOneLayout(blockLayout.contentUdi);
		});*/

		this.#fireChangeEvent();
	}

	override render() {
		return html`
			<umb-input-tiptap
				.configuration=${this._config}
				.value=${this._markup as any}
				?readonly=${this.readonly}
				@change=${this.#onChange}></umb-input-tiptap>
		`;
	}
}

export default UmbPropertyEditorUITiptapElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiptap': UmbPropertyEditorUITiptapElement;
	}
}
