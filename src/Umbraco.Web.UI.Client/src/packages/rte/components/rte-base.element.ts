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
	type UmbBlockRteTypeModel,
} from '@umbraco-cms/backoffice/block-rte';
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

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
			this._markup = this._latestMarkup = '';
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

			this.observe(
				observeMultiple([
					this.#managerContext.layouts,
					this.#managerContext.contents,
					this.#managerContext.settings,
					this.#managerContext.exposes,
				]),
				([layouts, contents, settings, exposes]) => {
					if (layouts.length === 0) {
						this._value = undefined;
					} else {
						this._value = {
							markup: this._latestMarkup,
							blocks: {
								layout: { [UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]: layouts },
								contentData: contents,
								settingsData: settings,
								expose: exposes,
							},
						};
					}

					// If we don't have a value set from the outside or an internal value, we don't want to set the value.
					// This is added to prevent the block list from setting an empty value on startup.
					if (!this._latestMarkup && !this._value?.markup) {
						return;
					}

					context.setValue(this._value);
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
		const unusedLayouts = this.#managerContext.getLayouts().filter((x) => usedContentKeys.indexOf(x.contentKey) === -1);

		const unusedContentKeys = unusedLayouts.map((x) => x.contentKey);

		const unusedSettingsKeys = unusedLayouts
			.map((x) => x.settingsKey)
			.filter((x) => typeof x === 'string') as Array<string>;

		this.#managerContext.removeManyContent(unusedContentKeys);
		this.#managerContext.removeManySettings(unusedSettingsKeys);
		this.#managerContext.removeManyLayouts(unusedContentKeys);
	}
	protected _fireChangeEvent() {
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}
}
