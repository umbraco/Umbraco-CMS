import { UmbBlockSingleManagerContext } from '../../context/block-single-manager.context.js';
import { UmbBlockSingleEntriesContext } from '../../context/block-single-entries.context.js';
import type { UmbBlockSingleLayoutModel, UmbBlockSingleValueModel } from '../../types.js';
import type { UmbBlockSingleEntryElement } from '../../components/block-single-entry/index.js';
import { UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS } from './constants.js';
import { UmbLitElement, umbDestroyOnDisconnect } from '@umbraco-cms/backoffice/lit-element';
import {
	html,
	customElement,
	property,
	state,
	repeat,
	css,
	nothing,
	type PropertyValueMap,
	ifDefined,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import type { UmbSorterConfig } from '@umbraco-cms/backoffice/sorter';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import type { UmbBlockLayoutBaseModel } from '@umbraco-cms/backoffice/block';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';

import '../../components/block-single-entry/index.js';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import {
	extractJsonQueryProps,
	UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
	UmbFormControlMixin,
	UmbValidationContext,
} from '@umbraco-cms/backoffice/validation';
import { jsonStringComparison, observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { debounceTime } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';

const SORTER_CONFIG: UmbSorterConfig<UmbBlockSingleLayoutModel, UmbBlockSingleEntryElement> = {
	getUniqueOfElement: (element) => {
		return element.contentKey!;
	},
	getUniqueOfModel: (modelEntry) => {
		return modelEntry.contentKey;
	},
	//identifier: 'block-single-editor',
	itemSelector: 'umb-block-single-entry',
	//containerSelector: 'EMPTY ON PURPOSE, SO IT BECOMES THE HOST ELEMENT',
};

@customElement('umb-property-editor-ui-block-single')
export class UmbPropertyEditorUIBlockSingleElement
	extends UmbFormControlMixin<UmbBlockSingleValueModel | undefined, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	readonly #sorter = new UmbSorterController<UmbBlockSingleLayoutModel, UmbBlockSingleEntryElement>(this, {
		...SORTER_CONFIG,
		onChange: ({ model }) => {
			this.#entriesContext.setLayouts(model);
		},
	});

	readonly #validationContext = new UmbValidationContext(this);

	#lastValue: UmbBlockSingleValueModel | undefined = undefined;

	@property({ attribute: false })
	public override set value(value: UmbBlockSingleValueModel | undefined) {
		this.#lastValue = value;

		if (!value) {
			super.value = undefined;
			return;
		}

		const buildUpValue: Partial<UmbBlockSingleValueModel> = value ? { ...value } : {};
		buildUpValue.layout ??= {};
		buildUpValue.contentData ??= [];
		buildUpValue.settingsData ??= [];
		buildUpValue.expose ??= [];
		super.value = buildUpValue as UmbBlockSingleValueModel;

		this.#managerContext.setLayouts(super.value.layout[UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? []);
		this.#managerContext.setContents(super.value.contentData);
		this.#managerContext.setSettings(super.value.settingsData);
		this.#managerContext.setExposes(super.value.expose);
	}
	public override get value(): UmbBlockSingleValueModel | undefined {
		return super.value;
	}

	@state()
	private _createButtonLabel = this.localize.term('content_createEmpty');

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const blocks = config.getValueByAlias<Array<UmbBlockTypeBaseModel>>('blocks') ?? [];
		this.#managerContext.setBlockTypes(blocks);

		const useInlineEditingAsDefault = config.getValueByAlias<boolean>('useInlineEditingAsDefault');
		this.#managerContext.setInlineEditingMode(useInlineEditingAsDefault);
		this.style.maxWidth = config.getValueByAlias<string>('maxPropertyWidth') ?? '';

		this.#managerContext.setEditorConfiguration(config);

		const customCreateButtonLabel = config.getValueByAlias<string>('createLabel');
		if (customCreateButtonLabel) {
			this._createButtonLabel = this.localize.string(customCreateButtonLabel);
		} else if (blocks.length === 1) {
			this.#managerContext.contentTypesLoaded.then(() => {
				const firstContentTypeName = this.#managerContext.getContentTypeNameOf(blocks[0].contentElementTypeKey);
				this._createButtonLabel = this.localize.term('blockEditor_addThis', this.localize.string(firstContentTypeName));

				// If we are in a invariant context:
			});
		}
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @default
	 */
	public set readonly(value) {
		this.#readonly = value;

		if (this.#readonly) {
			this.#sorter.disable();
			this.#managerContext.readOnlyState.fallbackToPermitted();
		} else {
			this.#sorter.enable();
			this.#managerContext.readOnlyState.fallbackToNotPermitted();
		}
	}
	public get readonly() {
		return this.#readonly;
	}
	#readonly = false;

	@property({ type: Boolean })
	mandatory?: boolean;

	@property({ type: String })
	mandatoryMessage?: string | undefined;

	@state()
	private _blocks?: Array<UmbBlockTypeBaseModel>;

	@state()
	private _layouts: Array<UmbBlockLayoutBaseModel> = [];

	@state()
	private _catalogueRouteBuilder?: UmbModalRouteBuilder;

	readonly #managerContext = new UmbBlockSingleManagerContext(this);
	readonly #entriesContext = new UmbBlockSingleEntriesContext(this);

	@state()
	private _notSupportedVariantSetting?: boolean;

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
							this._notSupportedVariantSetting = promises.filter((x) => x === true).length > 0;

							if (this._notSupportedVariantSetting) {
								this.#validationContext.messages.addMessage(
									'config',
									'$',
									'#blockEditor_blockVariantConfigurationNotSupported',
									'blockConfigurationNotSupported',
								);
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

		// TODO: Why is this logic not part of the Block Grid and RTE Editors? [NL]
		// Observe Blocks and clean up validation messages for content/settings that are not in the block single anymore:
		this.observe(
			this.#managerContext.layouts,
			(layouts) => {
				const validationMessagesToRemove: string[] = [];
				const contentKeys = layouts.map((x) => x.contentKey);
				this.#validationContext.messages.getMessagesOfPathAndDescendant('$.contentData').forEach((message) => {
					// get the KEY from this string: $.contentData[?(@.key == 'KEY')]
					// TODO: Investigate if this is missing a part to just get the [] part of the path. Cause couldn't there be a sub path inside of this. [NL]
					const key = extractJsonQueryProps(message.path).key;
					if (key && contentKeys.indexOf(key) === -1) {
						validationMessagesToRemove.push(message.key);
					}
				});

				const settingsKeys = layouts.map((x) => x.settingsKey).filter((x) => x !== undefined) as string[];
				this.#validationContext.messages.getMessagesOfPathAndDescendant('$.settingsData').forEach((message) => {
					// TODO: Investigate if this is missing a part to just get the [] part of the path. Cause couldn't there be a sub path inside of this. [NL]
					// get the key from this string: $.settingsData[?(@.key == 'KEY')]
					const key = extractJsonQueryProps(message.path).key;
					if (key && settingsKeys.indexOf(key) === -1) {
						validationMessagesToRemove.push(message.key);
					}
				});

				// Remove the messages after the loop to prevent changing the array while iterating over it.
				this.#validationContext.messages.removeMessageByKeys(validationMessagesToRemove);
			},
			null,
		);

		this.consumeContext(UMB_VARIANT_CONTEXT, async (context) => {
			this.observe(
				context?.displayVariantId,
				(variantId) => {
					this.#managerContext.setVariantId(variantId);
				},
				'observeContextualVariantId',
			);
		});

		this.addValidator(
			'rangeOverflow',
			() => this.localize.term('validation_entriesExceed', 1, this.#entriesContext.getLength() - 1),
			() => this.#entriesContext.getLength() > 1,
		);

		this.addValidator(
			'valueMissing',
			() => this.mandatoryMessage ?? UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
			() => {
				if (!this.mandatory || this.readonly) return false;
				const count = this.value?.layout?.[UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS]?.length ?? 0;
				return count === 0;
			},
		);

		this.observe(
			this.#entriesContext.layoutEntries,
			(layouts) => {
				this._layouts = layouts;
				// Update sorter.
				this.#sorter.setModel(layouts);
				// Update manager:
				this.#managerContext.setLayouts(layouts);
			},
			null,
		);

		this.observe(
			this.#managerContext.blockTypes,
			(blockTypes) => {
				this._blocks = blockTypes;
			},
			null,
		);

		this.observe(
			this.#entriesContext.catalogueRouteBuilder,
			(routeBuilder) => {
				this._catalogueRouteBuilder = routeBuilder;
			},
			null,
		);
	}

	#hasAutoCreatedABlock = false;
	protected override willUpdate(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.willUpdate(_changedProperties);

		if (
			this.mandatory === true &&
			this.#hasAutoCreatedABlock === false &&
			this.readonly !== true &&
			this._layouts?.length === 0 &&
			this._blocks?.length === 1
		) {
			// Only auto create once.
			this.#hasAutoCreatedABlock = true;
			// no blocks, and one Bock type, then auto create a block:
			const elementKey = this._blocks[0].contentElementTypeKey;
			this.#managerContext.createWithPresets(elementKey).then((block) => {
				if (block) {
					this.#managerContext.insert(block.layout, block.content, block.settings, { index: 0 });
				}
			});
		}
	}

	#gotPropertyContext(context: typeof UMB_PROPERTY_CONTEXT.TYPE | undefined) {
		this.observe(
			context?.dataPath,
			(dataPath) => {
				if (dataPath) {
					// Set the data path for the local validation context:
					this.#validationContext.setDataPath(dataPath);
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
			]).pipe(debounceTime(20)),
			([layouts, contents, settings, exposes]) => {
				if (layouts.length === 0) {
					if (this.value === undefined) {
						return;
					}
					super.value = undefined;
				} else {
					const newValue = {
						...super.value,
						layout: { [UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS]: layouts },
						contentData: contents,
						settingsData: settings,
						expose: exposes,
					};
					if (jsonStringComparison(this.value, newValue)) {
						return;
					}
					super.value = newValue;
				}

				// If we don't have a value set from the outside or an internal value, we don't want to set the value.
				// This is added to prevent the editor from setting an empty value on startup.
				if (this.#lastValue === undefined && super.value === undefined) {
					return;
				}

				context?.setValue(super.value);
			},
			'motherObserver',
		);
	}

	protected override getFormElement() {
		return undefined;
	}

	override render() {
		if (this._notSupportedVariantSetting) {
			return nothing;
		}
		return html`
			${repeat(
				this._layouts,
				(x) => x.contentKey,
				(layoutEntry) => html`
					<umb-block-single-entry
						.contentKey=${layoutEntry.contentKey}
						.layout=${layoutEntry}
						${umbDestroyOnDisconnect()}>
					</umb-block-single-entry>
				`,
			)}
			${this.#renderCreateButtonGroup()}
		`;
	}

	#renderCreateButtonGroup() {
		// Are we in read-only more and have one item, or do we already have an item (special for single block) then hide create button.
		if ((this.readonly && this._layouts.length > 0) || this._layouts.length >= 1) {
			return nothing;
		} else {
			return html` <uui-button-group> ${this.#renderCreateButton()} ${this.#renderPasteButton()} </uui-button-group> `;
		}
	}

	#renderCreateButton() {
		const createPath = this.#entriesContext.getPathForCreateBlock(-1);
		return html`
			<uui-button
				look="placeholder"
				label=${this._createButtonLabel}
				href=${ifDefined(createPath)}
				?disabled=${this.readonly}
				@click=${async () => {
					// If no path, then we can conclude there is not modal flow for the user to follow, instead we will just insert the Block: [NL]
					if (createPath === undefined) {
						if (!this._blocks || this._blocks.length === 0) {
							throw new Error('No block types are configured for this Block List property editor');
						}
						const originData = { index: -1 };
						const created = await this.#entriesContext.create(this._blocks[0].contentElementTypeKey, {}, originData);
						if (created) {
							this.#entriesContext.insert(created.layout, created.content, created.settings, originData);
						} else {
							throw new Error('Failed to create block');
						}
					}
				}}></uui-button>
		`;
	}

	#renderPasteButton() {
		return html`
			<uui-button
				label=${this.localize.term('content_createFromClipboard')}
				look="placeholder"
				href=${this._catalogueRouteBuilder?.({ view: 'clipboard', index: -1 }) ?? ''}
				?disabled=${this.readonly}>
				<uui-icon name="icon-clipboard-paste"></uui-icon>
			</uui-button>
		`;
	}

	static override readonly styles = [
		UmbTextStyles,

		css`
			:host {
				display: grid;
				gap: 1px;
			}
			> div {
				display: flex;
				flex-direction: column;
				align-items: stretch;
			}

			uui-button-group {
				padding-top: 1px;
				display: grid;
				grid-template-columns: 1fr auto;
			}
		`,
	];
}

export default UmbPropertyEditorUIBlockSingleElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-single': UmbPropertyEditorUIBlockSingleElement;
	}
}
