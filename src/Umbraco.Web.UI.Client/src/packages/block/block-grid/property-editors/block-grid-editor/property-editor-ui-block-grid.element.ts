import { UmbBlockGridManagerContext } from '../../context/block-grid-manager.context.js';
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
import { UmbBlockElementDataValidationPathTranslator } from '@umbraco-cms/backoffice/block';
import { debounceTime } from '@umbraco-cms/backoffice/external/rxjs';

import '../../components/block-grid-entries/index.js';

/**
 * @element umb-property-editor-ui-block-grid
 */
@customElement('umb-property-editor-ui-block-grid')
export class UmbPropertyEditorUIBlockGridElement
	extends UmbFormControlMixin<UmbBlockGridValueModel, typeof UmbLitElement>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	#validationContext = new UmbValidationContext(this);
	#contentDataPathTranslator?: UmbBlockElementDataValidationPathTranslator;
	#settingsDataPathTranslator?: UmbBlockElementDataValidationPathTranslator;
	#managerContext = new UmbBlockGridManagerContext(this);
	//
	private _value: UmbBlockGridValueModel = {
		layout: {},
		contentData: [],
		settingsData: [],
		expose: [],
	};

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
		const buildUpValue: Partial<UmbBlockGridValueModel> = value ? { ...value } : {};
		buildUpValue.layout ??= {};
		buildUpValue.contentData ??= [];
		buildUpValue.settingsData ??= [];
		buildUpValue.expose ??= [];
		this._value = buildUpValue as UmbBlockGridValueModel;

		this.#managerContext.setLayouts(this._value.layout[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? []);
		this.#managerContext.setContents(this._value.contentData);
		this.#managerContext.setSettings(this._value.settingsData);
		this.#managerContext.setExposes(this._value.expose);
	}
	public override get value(): UmbBlockGridValueModel {
		return this._value;
	}

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
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
					this._value = {
						...this._value,
						layout: { [UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]: layouts },
						contentData: contents,
						settingsData: settings,
						expose: exposes,
					};
					propertyContext.setValue(this._value);
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
