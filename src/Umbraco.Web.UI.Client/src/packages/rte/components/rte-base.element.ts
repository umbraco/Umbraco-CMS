import type { UmbPropertyEditorRteValueType } from '../types.js';
import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbBlockRteEntriesContext, UmbBlockRteManagerContext } from '@umbraco-cms/backoffice/block-rte';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import {
	UmbFormControlMixin,
	UmbValidationContext,
	UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
} from '@umbraco-cms/backoffice/validation';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { StyleInfo } from '@umbraco-cms/backoffice/external/lit';
import type { UmbBlockDataModel } from '@umbraco-cms/backoffice/block';
import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel } from '@umbraco-cms/backoffice/block-rte';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';

/**
 * The abstract base class that is used as a base for the rich-text-editor component.
 * @cssprop --umb-rte-width - The width of the rich-text-editor (default: unset)
 * @cssprop --umb-rte-min-width - The minimum width of the rich-text-editor (default: unset)
 * @cssprop --umb-rte-max-width - The maximum width of the rich-text-editor (default: 100%)
 * @cssprop --umb-rte-height - The height of the rich-text-editor (default: 100%)
 * @cssprop --umb-rte-min-height - The minimum height of the rich-text-editor (default: 100%)
 * @cssprop --umb-rte-max-height - The maximum height of the rich-text-editor (default: 100%)
 */
export abstract class UmbPropertyEditorUiRteElementBase
	extends UmbFormControlMixin<UmbPropertyEditorRteValueType | undefined, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	public name?: string;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._config = config;

		const blocks = config.getValueByAlias<Array<UmbBlockRteTypeModel>>('blocks') ?? [];
		this.#managerContext.setBlockTypes(blocks);

		this.#managerContext.setEditorConfiguration(config);

		const dimensions = config.getValueByAlias<{ width?: number; height?: number }>('dimensions');
		this._css = {
			'--umb-rte-max-width': dimensions?.width ? `${dimensions.width}px` : '100%',
			'--umb-rte-height': dimensions?.height ? `${dimensions.height}px` : 'unset',
		};
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
		if (buildUpValue.blocks) {
			buildUpValue.blocks = { ...buildUpValue.blocks };
		} else {
			buildUpValue.blocks ??= { layout: {}, contentData: [], settingsData: [], expose: [] };
		}
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

	@state()
	protected _css: StyleInfo = {};

	/**
	 * @deprecated _value is depreacated, use `super.value` instead.
	 * @returns {UmbPropertyEditorRteValueType | undefined} The value of the property editor.
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

	readonly #unusedLayoutLookup: Map<string, UmbBlockRteLayoutModel> = new Map();
	readonly #unusedContentLookup: Map<string, UmbBlockDataModel> = new Map();
	readonly #unusedSettingsLookup: Map<string, UmbBlockDataModel> = new Map();

	constructor() {
		super();

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (context) => {
			if (context) {
				this.observe(
					observeMultiple([
						this.#managerContext.blockTypes,
						context.structure.variesByCulture,
						context.structure.variesBySegment,
					]),
					async ([blockTypes, variesByCulture, variesBySegment]) => {
						if (blockTypes.length > 0 && (variesByCulture === false || variesBySegment === false)) {
							// check if any of the Blocks varyByCulture or Segment and then display a warning.
							const promises = await Promise.all(
								blockTypes.map(async (blockType) => {
									const elementType = blockType.contentElementTypeKey;
									await this.#managerContext.contentTypesLoaded;
									const structure = await this.#managerContext.getStructure(elementType);
									if (variesByCulture === false && structure?.getVariesByCulture() === true) {
										// If block varies by culture but document does not.
										return true;
									} else if (variesBySegment === false && structure?.getVariesBySegment() === true) {
										// If block varies by segment but document does not.
										return true;
									}
									return false;
								}),
							);
							const notSupportedVariantSetting = promises.filter((x) => x === true).length > 0;

							if (notSupportedVariantSetting) {
								this.setCustomValidity('#blockEditor_blockVariantConfigurationNotSupported');
								this.checkValidity();
							}
						}
					},
					'blockTypeConfigurationCheck',
				);
			} else {
				this.removeUmbControllerByAlias('blockTypeConfigurationCheck');
			}
		}).passContextAliasMatches();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.#gotPropertyContext(context);
		});

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
			this.#managerContext.setVariantId(context?.getVariantId());
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

	#gotPropertyContext(context: typeof UMB_PROPERTY_CONTEXT.TYPE | undefined) {
		this.observe(
			context?.dataPath,
			(dataPath) => {
				if (dataPath) {
					// Set the data path for the local validation context:
					this.#validationContext.setDataPath(dataPath + '.blocks');
					this.#validationContext.autoReport();
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

				context?.setValue(super.value);
			},
			'blockManagerObserver',
		);
	}

	#setUnusedBlockLookups(unusedLayouts: Array<UmbBlockRteLayoutModel>) {
		if (unusedLayouts.length) {
			unusedLayouts.forEach((layout) => {
				if (layout.contentKey) {
					this.#unusedLayoutLookup.set(layout.contentKey, layout);

					const contentBlock = this.#managerContext.getContentOf(layout.contentKey);
					if (contentBlock) {
						this.#unusedContentLookup.set(layout.contentKey, contentBlock);
					} else {
						console.warn(
							`Expected content block for '${layout.contentKey}' was not found. This may indicate a data consistency issue.`,
						);
					}

					if (layout.settingsKey) {
						const settingsBlock = this.#managerContext.getSettingsOf(layout.settingsKey);
						if (settingsBlock) {
							this.#unusedSettingsLookup.set(layout.settingsKey, settingsBlock);
						} else {
							console.warn(
								`Expected settings block for '${layout.settingsKey}' was not found. This may indicate a data consistency issue.`,
							);
						}
					}
				}
			});
		}
	}

	#restoreUnusedBlocks(usedContentKeys: Array<string | null>) {
		if (usedContentKeys.length) {
			usedContentKeys.forEach((contentKey) => {
				if (contentKey && this.#unusedLayoutLookup.has(contentKey)) {
					const layout = this.#unusedLayoutLookup.get(contentKey);
					if (layout) {
						this.#managerContext.setOneLayout(layout);
						this.#unusedLayoutLookup.delete(contentKey);

						const contentBlock = this.#unusedContentLookup.get(contentKey);
						if (contentBlock) {
							this.#managerContext.setOneContent(contentBlock);
							this.#managerContext.setOneExpose(contentKey, UmbVariantId.CreateInvariant());
							this.#unusedContentLookup.delete(contentKey);
						}

						if (layout.settingsKey && this.#unusedSettingsLookup.has(layout.settingsKey)) {
							const settingsBlock = this.#unusedSettingsLookup.get(layout.settingsKey);
							if (settingsBlock) {
								this.#managerContext.setOneSettings(settingsBlock);
								this.#unusedSettingsLookup.delete(layout.settingsKey);
							}
						}
					}
				}
			});
		}
	}

	protected _filterUnusedBlocks(usedContentKeys: (string | null)[]) {
		const unusedLayouts = this.#managerContext.getLayouts().filter((x) => usedContentKeys.indexOf(x.contentKey) === -1);

		// Temporarily set the unused layouts to the lookup, as they could be restored later, e.g. via an RTE undo action. [LK]
		this.#restoreUnusedBlocks(usedContentKeys);
		this.#setUnusedBlockLookups(unusedLayouts);

		const unusedContentKeys = unusedLayouts.map((x) => x.contentKey);

		const unusedSettingsKeys = unusedLayouts
			.map((x) => x.settingsKey)
			.filter((x) => typeof x === 'string') as Array<string>;

		this.#managerContext.removeManyContent(unusedContentKeys);
		this.#managerContext.removeManySettings(unusedSettingsKeys);
		this.#managerContext.removeManyLayouts(unusedContentKeys);
	}

	protected _fireChangeEvent() {
		this.dispatchEvent(new UmbChangeEvent());
	}

	override destroy() {
		super.destroy();
		this.#unusedLayoutLookup.clear();
		this.#unusedContentLookup.clear();
		this.#unusedSettingsLookup.clear();
	}
}
