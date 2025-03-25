import { UmbBlockGridManagerContext } from '../../block-grid-manager/index.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from './constants.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	html,
	customElement,
	property,
	state,
	css,
	type PropertyValueMap,
	ref,
	nothing,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbFormControlMixin, UmbValidationContext } from '@umbraco-cms/backoffice/validation';
import type { UmbBlockTypeGroup } from '@umbraco-cms/backoffice/block-type';
import type { UmbBlockGridTypeModel, UmbBlockGridValueModel } from '@umbraco-cms/backoffice/block-grid';
import { debounceTime } from '@umbraco-cms/backoffice/external/rxjs';

// TODO: consider moving the components to the property editor folder as they are only used here
import '../../local-components.js';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';

/**
 * @element umb-property-editor-ui-block-grid
 */
@customElement('umb-property-editor-ui-block-grid')
export class UmbPropertyEditorUIBlockGridElement
	extends UmbFormControlMixin<UmbBlockGridValueModel, typeof UmbLitElement>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	#validationContext = new UmbValidationContext(this);
	#managerContext = new UmbBlockGridManagerContext(this);
	//

	#lastValue: UmbBlockGridValueModel | undefined = undefined;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const blocks = config.getValueByAlias<Array<UmbBlockGridTypeModel>>('blocks') ?? [];
		this.#managerContext.setBlockTypes(blocks);

		const blockGroups = config.getValueByAlias<Array<UmbBlockTypeGroup>>('blockGroups') ?? [];
		this.#managerContext.setBlockGroups(blockGroups);

		const useInlineEditingAsDefault = config.getValueByAlias<boolean>('useInlineEditingAsDefault');
		this.#managerContext.setInlineEditingMode(useInlineEditingAsDefault);

		this.style.maxWidth = config.getValueByAlias<string>('maxPropertyWidth') ?? '';

		//config.useLiveEditing, is covered by the EditorConfiguration of context. [NL]
		this.#managerContext.setEditorConfiguration(config);
	}

	@state()
	private _layoutColumns?: number;

	@property({ attribute: false })
	public override set value(value: UmbBlockGridValueModel | undefined) {
		this.#lastValue = value;

		if (!value) {
			super.value = undefined;
			return;
		}

		const buildUpValue: Partial<UmbBlockGridValueModel> = value ? { ...value } : {};
		buildUpValue.layout ??= {};
		buildUpValue.contentData ??= [];
		buildUpValue.settingsData ??= [];
		buildUpValue.expose ??= [];
		super.value = buildUpValue as UmbBlockGridValueModel;

		this.#managerContext.setLayouts(super.value.layout[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? []);
		this.#managerContext.setContents(super.value.contentData);
		this.#managerContext.setSettings(super.value.settingsData);
		this.#managerContext.setExposes(super.value.expose);
	}
	public override get value(): UmbBlockGridValueModel | undefined {
		return super.value;
	}

	@state()
	_notSupportedVariantSetting?: boolean;

	constructor() {
		super();

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (context) => {
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
								'configuration',
								'$',
								'#blockEditor_blockVariantConfigurationNotSupported',
							);
						}
					}
				},
			);
		}).passContextAliasMatches();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.observe(
				context.dataPath,
				(dataPath) => {
					if (dataPath) {
						// Set the data path for the local validation context:
						this.#validationContext.setDataPath(dataPath);
						this.#validationContext.autoReport();
					}
				},
				'observeDataPath',
			);
		});

		// TODO: Prevent initial notification from these observes
		this.consumeContext(UMB_PROPERTY_CONTEXT, (propertyContext) => {
			this.observe(
				observeMultiple([
					this.#managerContext.layouts,
					this.#managerContext.contents,
					this.#managerContext.settings,
					this.#managerContext.exposes,
				]).pipe(debounceTime(20)),
				([layouts, contents, settings, exposes]) => {
					if (layouts.length === 0) {
						super.value = undefined;
					} else {
						super.value = {
							...super.value,
							layout: { [UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]: layouts },
							contentData: contents,
							settingsData: settings,
							expose: exposes,
						};
					}

					// If we don't have a value set from the outside or an internal value, we don't want to set the value.
					// This is added to prevent the block grid from setting an empty value on startup.
					if (this.#lastValue === undefined && super.value === undefined) {
						return;
					}

					propertyContext.setValue(super.value);
				},
				'motherObserver',
			);

			this.observe(
				propertyContext?.alias,
				(alias) => {
					this.#managerContext.setPropertyAlias(alias);
				},
				'observePropertyAlias',
			);

			// If the current property is readonly all inner block content should also be readonly.
			this.observe(
				observeMultiple([propertyContext.isReadOnly, propertyContext.variantId]),
				([isReadOnly, variantId]) => {
					const unique = 'UMB_PROPERTY_EDITOR_UI';
					if (variantId === undefined) return;

					if (isReadOnly) {
						const state = {
							unique,
							variantId,
							message: '',
						};

						this.#managerContext.readOnlyState.addState(state);
					} else {
						this.#managerContext.readOnlyState.removeState(unique);
					}
				},
				'observeIsReadOnly',
			);
		});

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
			this.#managerContext.setVariantId(context.getVariantId());
		});
	}

	protected override firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);

		this.observe(this.#managerContext.gridColumns, (gridColumns) => {
			if (gridColumns) {
				this._layoutColumns = gridColumns;
				this.style.setProperty('--umb-block-grid--grid-columns', gridColumns.toString());
			}
		});
	}

	#currentEntriesElement?: Element;
	#gotRootEntriesElement(element: Element | undefined): void {
		if (this.#currentEntriesElement === element) return;
		if (this.#currentEntriesElement) {
			this.removeFormControlElement(this.#currentEntriesElement as any);
		}
		this.#currentEntriesElement = element;
		if (element) {
			this.addFormControlElement(element as any);
		}
	}

	override render() {
		if (this._notSupportedVariantSetting) {
			return nothing;
		}
		return html` <umb-block-grid-entries
			${ref(this.#gotRootEntriesElement)}
			.areaKey=${null}
			.layoutColumns=${this._layoutColumns}></umb-block-grid-entries>`;
	}

	static override styles = [
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

export default UmbPropertyEditorUIBlockGridElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid': UmbPropertyEditorUIBlockGridElement;
	}
}
