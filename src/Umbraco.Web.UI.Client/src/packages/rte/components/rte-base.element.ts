import { type UmbPropertyEditorUiValueType, UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../types.js';
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
	type UmbBlockRteTypeModel,
} from '@umbraco-cms/backoffice/block-rte';
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { debounceTime } from '@umbraco-cms/backoffice/external/rxjs';

export abstract class UmbRteBaseElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._config = config;

		const blocks = config.getValueByAlias<Array<UmbBlockRteTypeModel>>('blocks') ?? [];
		this.#managerContext.setBlockTypes(blocks);

		this.#managerContext.setEditorConfiguration(config);
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@property({
		attribute: false,
		type: Object,
		hasChanged(value?: UmbPropertyEditorUiValueType, oldValue?: UmbPropertyEditorUiValueType) {
			return value?.markup !== oldValue?.markup;
		},
	})
	public set value(value: UmbPropertyEditorUiValueType | undefined) {
		const buildUpValue: Partial<UmbPropertyEditorUiValueType> = value ? { ...value } : {};
		buildUpValue.markup ??= '';
		buildUpValue.blocks ??= { layout: {}, contentData: [], settingsData: [], expose: [] };
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

	@state()
	protected _config?: UmbPropertyEditorConfigCollection;

	@state()
	protected _value: UmbPropertyEditorUiValueType = {
		markup: '',
		blocks: { layout: {}, contentData: [], settingsData: [], expose: [] },
	};

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

			this.observe(
				observeMultiple([
					this.#managerContext.layouts,
					this.#managerContext.contents,
					this.#managerContext.settings,
					this.#managerContext.exposes,
				]).pipe(debounceTime(20)),
				([layouts, contents, settings, exposes]) => {
					console.log('new blocks', layouts, contents, exposes);
					this._value = {
						...this._value,
						blocks: {
							layout: { [UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]: layouts },
							contentData: contents,
							settingsData: settings,
							expose: exposes,
						},
					};
					//context.setValue(this._value);
					this._fireChangeEvent();
				},
				'motherObserver',
			);
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
		const unusedBlocks = this.#managerContext.getLayouts().filter((x) => usedContentKeys.indexOf(x.contentKey) === -1);
		unusedBlocks.forEach((blockLayout) => {
			this.#managerContext.removeOneLayout(blockLayout.contentKey);
		});
	}

	protected _fireChangeEvent() {
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}
}
