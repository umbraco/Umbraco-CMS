import type { UmbPropertyEditorRteValueType } from '../types.js';
import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';
import { property, state } from '@umbraco-cms/backoffice/external/lit';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UmbBlockRteEntriesContext, UmbBlockRteManagerContext } from '@umbraco-cms/backoffice/block-rte';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbBlockRteTypeModel } from '@umbraco-cms/backoffice/block-rte';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import {
	UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
	UmbFormControlMixin,
	UmbValidationContext,
} from '@umbraco-cms/backoffice/validation';

export abstract class UmbPropertyEditorUiRteElementBase
	extends UmbFormControlMixin<UmbPropertyEditorRteValueType | undefined, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
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
		hasChanged(value?: UmbPropertyEditorRteValueType, oldValue?: UmbPropertyEditorRteValueType) {
			return value?.markup !== oldValue?.markup;
		},
	})
	public override set value(value: UmbPropertyEditorRteValueType | undefined) {
		if (!value) {
			super.value = undefined;
			this._markup = '';
			this.#managerContext.setLayouts([]);
			this.#managerContext.setContents([]);
			this.#managerContext.setSettings([]);
			this.#managerContext.setExposes([]);
			return;
		}

		const buildUpValue: Partial<UmbPropertyEditorRteValueType> = value ? { ...value } : {};
		buildUpValue.markup ??= '';
		buildUpValue.blocks ??= { layout: {}, contentData: [], settingsData: [], expose: [] };
		buildUpValue.blocks.layout ??= {};
		buildUpValue.blocks.contentData ??= [];
		buildUpValue.blocks.settingsData ??= [];
		buildUpValue.blocks.expose ??= [];
		super.value = buildUpValue as UmbPropertyEditorRteValueType;

		// Only update the actual editor markup if it is not the same as the value.
		if (this._markup !== super.value.markup) {
			this._markup = super.value.markup;
		}

		this.#managerContext.setLayouts(buildUpValue.blocks.layout[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? []);
		this.#managerContext.setContents(buildUpValue.blocks.contentData);
		this.#managerContext.setSettings(buildUpValue.blocks.settingsData);
		this.#managerContext.setExposes(buildUpValue.blocks.expose);
	}
	public override get value(): UmbPropertyEditorRteValueType | undefined {
		return super.value;
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@property({ type: Boolean })
	mandatory?: boolean;

	@property({ type: String })
	mandatoryMessage?: string | undefined;

	@state()
	protected _config?: UmbPropertyEditorConfigCollection;

	/**
	 * @deprecated _value is depreacated, use `super.value` instead.
	 */
	@state()
	protected get _value(): UmbPropertyEditorRteValueType | undefined {
		return super.value;
	}
	protected set _value(value: UmbPropertyEditorRteValueType | undefined) {
		super.value = value;
	}

	/**
	 * Separate state for markup, to avoid re-rendering/re-setting the value of the Tiptap editor when the value does not really change.
	 */
	@state()
	protected _markup = '';

	readonly #managerContext = new UmbBlockRteManagerContext(this);
	readonly #entriesContext = new UmbBlockRteEntriesContext(this);

	readonly #validationContext = new UmbValidationContext(this);

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.#gotPropertyContext(context);
		});

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
			this.#managerContext.setVariantId(context.getVariantId());
		});

		this.observe(
			this.#entriesContext.layoutEntries,
			(layouts) => {
				// Update manager:
				this.#managerContext.setLayouts(layouts);
			},
			null,
		);

		this.addValidator(
			'valueMissing',
			() => this.mandatoryMessage ?? UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
			() => !!this.mandatory && this.value === undefined,
		);
	}

	#gotPropertyContext(context: typeof UMB_PROPERTY_CONTEXT.TYPE) {
		this.observe(
			context.dataPath,
			(dataPath) => {
				if (dataPath) {
					// Set the data path for the local validation context:
					this.#validationContext.setDataPath(dataPath + '.blocks');
				}
			},
			'observeDataPath',
		);

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
			]),
			([layouts, contents, settings, exposes]) => {
				if (layouts.length === 0) {
					if (super.value?.markup === undefined) {
						super.value = undefined;
					} else {
						super.value = {
							...super.value,
							blocks: {
								layout: {},
								contentData: [],
								settingsData: [],
								expose: [],
							},
						};
					}
				} else {
					super.value = {
						markup: this._markup,
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
				if (super.value?.markup === undefined) {
					return;
				}

				context.setValue(super.value);
			},
			'motherObserver',
		);
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
