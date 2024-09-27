import type { UmbInputTiptapElement } from '../../components/input-tiptap/input-tiptap.element.js';
import { UMB_BLOCK_RTE_BLOCK_LAYOUT_ALIAS } from '../../../types.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbBlockRteEntriesContext, UmbBlockRteManagerContext } from '@umbraco-cms/backoffice/block-rte';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel } from '@umbraco-cms/backoffice/block-rte';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbBlockValueType } from '@umbraco-cms/backoffice/block';

import '../../components/input-tiptap/input-tiptap.element.js';

export interface UmbRichTextEditorValueType {
	markup: string;
	blocks: UmbBlockValueType<UmbBlockRteLayoutModel>;
}

const elementName = 'umb-property-editor-ui-tiptap';

/**
 * @element umb-property-editor-ui-tiptap
 */
@customElement(elementName)
export class UmbPropertyEditorUiTiptapElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._config = config;

		const blocks = config.getValueByAlias<Array<UmbBlockRteTypeModel>>('blocks') ?? [];
		this.#managerContext.setBlockTypes(blocks);

		this.#managerContext.setEditorConfiguration(config);
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

	// Separate state for markup, to avoid re-rendering/re-setting the value of the Tiptap editor when the value does not really change.
	@state()
	private _markup = '';
	private _latestMarkup = ''; // The latest value gotten from the Tiptap editor.

	readonly #managerContext = new UmbBlockRteManagerContext(this);
	readonly #entriesContext = new UmbBlockRteEntriesContext(this);

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
		const value = event.target.value;
		this._latestMarkup = value;

		this._value = {
			...this._value,
			markup: this._latestMarkup,
		};

		// Remove unused Blocks of Blocks Layout. Leaving only the Blocks that are present in Markup.
		const usedContentUdis: string[] = [];

		// Regex matching all block elements in the markup, and extracting the content UDI. It's the same as the one used on the backend.
		const regex = new RegExp(
			/<umb-rte-block(?:-inline)?(?: class="(?:.[^"]*)")? data-content-udi="(?<udi>.[^"]*)">(?:<!--Umbraco-Block-->)?<\/umb-rte-block(?:-inline)?>/gi,
		);
		let blockElement: RegExpExecArray | null;
		while ((blockElement = regex.exec(this._latestMarkup)) !== null) {
			if (blockElement.groups?.udi) {
				usedContentUdis.push(blockElement.groups.udi);
			}
		}

		const unusedBlocks = this.#managerContext.getLayouts().filter((x) => usedContentUdis.indexOf(x.contentUdi) === -1);
		unusedBlocks.forEach((blockLayout) => {
			this.#managerContext.removeOneLayout(blockLayout.contentUdi);
		});

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

export { UmbPropertyEditorUiTiptapElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbPropertyEditorUiTiptapElement;
	}
}
