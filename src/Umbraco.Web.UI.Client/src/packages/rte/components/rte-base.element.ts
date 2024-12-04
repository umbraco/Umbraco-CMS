import type { UmbPropertyEditorUiValueType } from '../types.js';
import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';
import { property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import {
	UmbBlockRteEntriesContext,
	UmbBlockRteManagerContext,
	type UmbBlockRteLayoutModel,
	type UmbBlockRteTypeModel,
} from '@umbraco-cms/backoffice/block-rte';
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbBlockValueType } from '@umbraco-cms/backoffice/block';

// eslint-disable-next-line local-rules/enforce-element-suffix-on-element-class-name
export abstract class UmbPropertyEditorUiRteElementBase extends UmbLitElement implements UmbPropertyEditorUiElement {
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
		hasChanged(value?: UmbPropertyEditorUiValueType, oldValue?: UmbPropertyEditorUiValueType) {
			return value?.markup !== oldValue?.markup;
		},
	})
	public set value(value: UmbPropertyEditorUiValueType | undefined) {
		if (!value) {
			this._value = undefined;
			this._markup = '';
			this.#managerContext.setLayouts([]);
			this.#managerContext.setContents([]);
			this.#managerContext.setSettings([]);
			this.#managerContext.setExposes([]);
			return;
		}

		const buildUpValue: Partial<UmbPropertyEditorUiValueType> = value ? { ...value } : {};
		buildUpValue.markup ??= '';
		buildUpValue.blocks ??= { layout: {}, contentData: [], settingsData: [], expose: [] };
		buildUpValue.blocks.layout ??= {};
		buildUpValue.blocks.contentData ??= [];
		buildUpValue.blocks.settingsData ??= [];
		buildUpValue.blocks.expose ??= [];
		this._value = buildUpValue as UmbPropertyEditorUiValueType;

		// Only update the actual editor markup if it is not the same as the value.
		if (this._latestMarkup !== this._value.markup) {
			this._markup = this._value.markup;
		}

		this.#managerContext.setLayouts(buildUpValue.blocks.layout[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? []);
		this.#managerContext.setContents(buildUpValue.blocks.contentData);
		this.#managerContext.setSettings(buildUpValue.blocks.settingsData);
		this.#managerContext.setExposes(buildUpValue.blocks.expose);
	}
	public get value() {
		return this._value;
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	protected _config?: UmbPropertyEditorConfigCollection;

	@state()
	protected _value?: UmbPropertyEditorUiValueType | undefined;

	/**
	 * Separate state for markup, to avoid re-rendering/re-setting the value of the Tiptap editor when the value does not really change.
	 */
	@state()
	protected _markup = '';

	/**
	 * The latest value gotten from the RTE editor.
	 */
	protected _latestMarkup = '';

	readonly #managerContext = new UmbBlockRteManagerContext(this);
	readonly #entriesContext = new UmbBlockRteEntriesContext(this);

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			// TODO: Implement validation translation for RTE Blocks:
			/*
			this.observe(
				context.dataPath,
				(dataPath) => {
					// Translate paths for content/settings:
					this.#contentDataPathTranslator?.destroy();
					this.#settingsDataPathTranslator?.destroy();
					if (dataPath) {
						// Set the data path for the local validation context:
						this.#validationContext.setDataPath(dataPath);

						this.#contentDataPathTranslator = new UmbBlockElementDataValidationPathTranslator(this, 'contentData');
						this.#settingsDataPathTranslator = new UmbBlockElementDataValidationPathTranslator(this, 'settingsData');
					}
				},
				'observeDataPath',
			);
			*/

			this.observe(
				context?.alias,
				(alias) => {
					this.#managerContext.setPropertyAlias(alias);
				},
				'observePropertyAlias',
			);

			this.observe(this.#entriesContext.layoutEntries, (layouts) => {
				// Update manager:
				this.#managerContext.setLayouts(layouts);
			});

			// Observe the value of the property and update the editor value.
			this.observe(this.#managerContext.layouts, (layouts) => {
				const blocksValue =
					this._value && layouts?.length > 0
						? { ...this._value.blocks, layout: { [UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]: layouts } }
						: undefined;

				this.#setBlocksValue(blocksValue);
			});

			this.observe(this.#managerContext.contents, (contents) => {
				const blocksValue = this._value ? { ...this._value.blocks, contentData: contents } : undefined;
				this.#setBlocksValue(blocksValue);
			});

			this.observe(this.#managerContext.settings, (settings) => {
				const blocksValue = this._value ? { ...this._value.blocks, settingsData: settings } : undefined;
				this.#setBlocksValue(blocksValue);
			});

			this.observe(this.#managerContext.exposes, (exposes) => {
				const blocksValue = this._value ? { ...this._value.blocks, expose: exposes } : undefined;
				this.#setBlocksValue(blocksValue);
			});
		});

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
			this.#managerContext.setVariantId(context.getVariantId());
		});

		this.observe(this.#entriesContext.layoutEntries, (layouts) => {
			// Update manager:
			this.#managerContext.setLayouts(layouts);
		});
	}

	protected _filterUnusedBlocks(usedContentKeys: (string | null)[]) {
		const unusedBlockContents = this.#managerContext.getContents().filter((x) => usedContentKeys.indexOf(x.key) === -1);
		unusedBlockContents.forEach((blockContent) => {
			this.#managerContext.removeOneContent(blockContent.key);
		});
		const unusedBlocks = this.#managerContext.getLayouts().filter((x) => usedContentKeys.indexOf(x.contentKey) === -1);
		unusedBlocks.forEach((blockLayout) => {
			this.#managerContext.removeOneLayout(blockLayout.contentKey);
		});
	}

	#setBlocksValue(blocksValue?: UmbBlockValueType<UmbBlockRteLayoutModel>) {
		if (!blocksValue || !this._value) {
			return;
		}

		this._value = {
			...this._value,
			blocks: blocksValue,
		};

		this._fireChangeEvent();
	}

	protected _fireChangeEvent() {
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}
}
