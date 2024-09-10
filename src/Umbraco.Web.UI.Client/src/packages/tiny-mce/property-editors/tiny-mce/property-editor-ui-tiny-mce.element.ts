import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbBlockRteEntriesContext,
	type UmbBlockRteLayoutModel,
	UmbBlockRteManagerContext,
	type UmbBlockRteTypeModel,
} from '@umbraco-cms/backoffice/block-rte';
import type { UmbBlockValueType } from '@umbraco-cms/backoffice/block';

import '../../components/input-tiny-mce/input-tiny-mce.element.js';

export interface UmbRichTextEditorValueType {
	markup: string;
	blocks: UmbBlockValueType<UmbBlockRteLayoutModel>;
}

const UMB_BLOCK_RTE_BLOCK_LAYOUT_ALIAS = 'Umbraco.TinyMCE'; // Not rich text, cause this has not been migrated [NL]

/**
 * @element umb-property-editor-ui-tiny-mce
 */
@customElement('umb-property-editor-ui-tiny-mce')
export class UmbPropertyEditorUITinyMceElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	// No need to registerer as a LIT-property, as we are calling it directly and no need for it to be reactive [NL]
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._config = config;

		const blocks = config.getValueByAlias<Array<UmbBlockRteTypeModel>>('blocks') ?? [];
		this.#managerContext.setBlockTypes(blocks);

		this.#managerContext.setEditorConfiguration(config);
	}

	@property({ attribute: false })
	public set value(value: UmbRichTextEditorValueType | undefined) {
		const buildUpValue: Partial<UmbRichTextEditorValueType> = value ? { ...value } : {};
		buildUpValue.markup ??= '';
		buildUpValue.blocks ??= { layout: {}, contentData: [], settingsData: [] };
		buildUpValue.blocks.layout ??= {};
		buildUpValue.blocks.contentData ??= [];
		buildUpValue.blocks.settingsData ??= [];
		this._value = buildUpValue as UmbRichTextEditorValueType;

		if (this._latestMarkup !== this._value.markup) {
			this._markup = this._value.markup;
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
	 * @type {boolean}
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

	// Separate state for markup, to avoid re-rendering/re-setting the value of the TinyMCE editor when the value does not really change.
	@state()
	private _markup = '';
	private _latestMarkup = ''; // The latest value gotten from the TinyMCE editor.

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

	#onChange() {
		const editor = this.#managerContext.getTinyMceEditor();
		if (!editor) return;

		// Clone the DOM, to remove the classes and attributes on the original:
		const div = document.createElement('div');
		div.innerHTML = editor.getContent();

		// Loop through used, to remove the classes on these.
		const blockEls = div.querySelectorAll(`umb-rte-block, umb-rte-block-inline`);
		blockEls.forEach((blockEl) => {
			blockEl.removeAttribute('contenteditable');
			blockEl.removeAttribute('class');
		});

		const markup = div.innerHTML;

		// Remove unused Blocks of Blocks Layout. Leaving only the Blocks that are present in Markup.
		//const blockElements = editor.dom.select(`umb-rte-block, umb-rte-block-inline`);
		const usedContentUdis = Array.from(blockEls).map((blockElement) => blockElement.getAttribute('data-content-udi'));
		const unusedBlocks = this.#managerContext.getLayouts().filter((x) => usedContentUdis.indexOf(x.contentUdi) === -1);
		unusedBlocks.forEach((blockLayout) => {
			this.#managerContext.removeOneLayout(blockLayout.contentUdi);
		});

		// Then get the content of the editor and update the value.
		// maybe in this way doc.body.innerHTML;

		this._latestMarkup = markup;

		this._value = {
			...this._value,
			markup: markup,
		};
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
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
