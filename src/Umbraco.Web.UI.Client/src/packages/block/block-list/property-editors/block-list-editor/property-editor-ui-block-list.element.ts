import { UmbBlockListManagerContext } from '../../context/block-list-manager.context.js';
import { UmbBlockListEntriesContext } from '../../context/block-list-entries.context.js';
import type { UmbBlockListLayoutModel, UmbBlockListValueModel } from '../../types.js';
import type { UmbBlockListEntryElement } from '../../components/block-list-entry/index.js';
import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from './constants.js';
import { UmbLitElement, umbDestroyOnDisconnect } from '@umbraco-cms/backoffice/lit-element';
import { html, customElement, property, state, repeat, css, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import type { UmbSorterConfig } from '@umbraco-cms/backoffice/sorter';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { type UmbBlockLayoutBaseModel } from '@umbraco-cms/backoffice/block';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';

import '../../components/block-list-entry/index.js';
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import {
	extractJsonQueryProps,
	UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
	UmbFormControlMixin,
	UmbValidationContext,
} from '@umbraco-cms/backoffice/validation';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import { debounceTime } from '@umbraco-cms/backoffice/external/rxjs';

const SORTER_CONFIG: UmbSorterConfig<UmbBlockListLayoutModel, UmbBlockListEntryElement> = {
	getUniqueOfElement: (element) => {
		return element.contentKey!;
	},
	getUniqueOfModel: (modelEntry) => {
		return modelEntry.contentKey;
	},
	//identifier: 'block-list-editor',
	itemSelector: 'umb-block-list-entry',
	//containerSelector: 'EMPTY ON PURPOSE, SO IT BECOMES THE HOST ELEMENT',
};

@customElement('umb-property-editor-ui-block-list')
export class UmbPropertyEditorUIBlockListElement
	extends UmbFormControlMixin<UmbBlockListValueModel | undefined, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	readonly #sorter = new UmbSorterController<UmbBlockListLayoutModel, UmbBlockListEntryElement>(this, {
		...SORTER_CONFIG,
		onChange: ({ model }) => {
			this.#entriesContext.setLayouts(model);
		},
	});

	readonly #validationContext = new UmbValidationContext(this);

	#lastValue: UmbBlockListValueModel | undefined = undefined;

	@property({ attribute: false })
	public override set value(value: UmbBlockListValueModel | undefined) {
		this.#lastValue = value;

		if (!value) {
			super.value = undefined;
			return;
		}

		const buildUpValue: Partial<UmbBlockListValueModel> = value ? { ...value } : {};
		buildUpValue.layout ??= {};
		buildUpValue.contentData ??= [];
		buildUpValue.settingsData ??= [];
		buildUpValue.expose ??= [];
		super.value = buildUpValue as UmbBlockListValueModel;

		this.#managerContext.setLayouts(super.value.layout[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? []);
		this.#managerContext.setContents(super.value.contentData);
		this.#managerContext.setSettings(super.value.settingsData);
		this.#managerContext.setExposes(super.value.expose);
	}
	public override get value(): UmbBlockListValueModel | undefined {
		return super.value;
	}

	@state()
	private _createButtonLabel = this.localize.term('content_createEmpty');

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const validationLimit = config.getValueByAlias<UmbNumberRangeValueType>('validationLimit');

		this._limitMin = validationLimit?.min;
		this._limitMax = validationLimit?.max;

		const blocks = config.getValueByAlias<Array<UmbBlockTypeBaseModel>>('blocks') ?? [];
		this.#managerContext.setBlockTypes(blocks);

		const useInlineEditingAsDefault = config.getValueByAlias<boolean>('useInlineEditingAsDefault');
		this.#managerContext.setInlineEditingMode(useInlineEditingAsDefault);
		this.style.maxWidth = config.getValueByAlias<string>('maxPropertyWidth') ?? '';
		// TODO:
		//config.useSingleBlockMode, not done jet

		this.#managerContext.setEditorConfiguration(config);

		const customCreateButtonLabel = config.getValueByAlias<string>('createLabel');
		if (customCreateButtonLabel) {
			this._createButtonLabel = this.localize.string(customCreateButtonLabel);
		} else if (blocks.length === 1) {
			this.#managerContext.contentTypesLoaded.then(() => {
				const firstContentTypeName = this.#managerContext.getContentTypeNameOf(blocks[0].contentElementTypeKey);
				this._createButtonLabel = this.localize.term('blockEditor_addThis', this.localize.string(firstContentTypeName));
			});
		}
	}

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @default
	 */
	@property({ type: Boolean, reflect: true })
	public set readonly(value) {
		this.#readonly = value;

		if (this.#readonly) {
			this.#sorter.disable();
		} else {
			this.#sorter.enable();
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
	private _limitMin?: number;
	@state()
	private _limitMax?: number;

	@state()
	private _blocks?: Array<UmbBlockTypeBaseModel>;

	@state()
	private _layouts: Array<UmbBlockLayoutBaseModel> = [];

	@state()
	private _catalogueRouteBuilder?: UmbModalRouteBuilder;

	readonly #managerContext = new UmbBlockListManagerContext(this);
	readonly #entriesContext = new UmbBlockListEntriesContext(this);

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.#gotPropertyContext(context);
		});

		// Observe Blocks and clean up validation messages for content/settings that are not in the block list anymore:
		this.observe(
			this.#managerContext.layouts,
			(layouts) => {
				const contentKeys = layouts.map((x) => x.contentKey);
				this.#validationContext.messages.getMessagesOfPathAndDescendant('$.contentData').forEach((message) => {
					// get the KEY from this string: $.contentData[?(@.key == 'KEY')]
					const key = extractJsonQueryProps(message.path).key;
					if (key && contentKeys.indexOf(key) === -1) {
						this.#validationContext.messages.removeMessageByKey(message.key);
					}
				});

				const settingsKeys = layouts.map((x) => x.settingsKey).filter((x) => x !== undefined) as string[];
				this.#validationContext.messages.getMessagesOfPathAndDescendant('$.settingsData').forEach((message) => {
					// get the key from this string: $.settingsData[?(@.key == 'KEY')]
					const key = extractJsonQueryProps(message.path).key;
					if (key && settingsKeys.indexOf(key) === -1) {
						this.#validationContext.messages.removeMessageByKey(message.key);
					}
				});
			},
			null,
		);

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
			this.#managerContext.setVariantId(context.getVariantId());
		});

		this.addValidator(
			'rangeUnderflow',
			() =>
				this.localize.term(
					'validation_entriesShort',
					this._limitMin,
					(this._limitMin ?? 0) - this.#entriesContext.getLength(),
				),
			() => !!this._limitMin && this.#entriesContext.getLength() < this._limitMin,
		);

		this.addValidator(
			'rangeOverflow',
			() =>
				this.localize.term(
					'validation_entriesExceed',
					this._limitMax,
					this.#entriesContext.getLength() - (this._limitMax || 0),
				),
			() => !!this._limitMax && this.#entriesContext.getLength() > this._limitMax,
		);

		this.addValidator(
			'valueMissing',
			() => this.mandatoryMessage ?? UMB_VALIDATION_EMPTY_LOCALIZATION_KEY,
			() => !!this.mandatory && this.#entriesContext.getLength() === 0,
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

	#gotPropertyContext(context: typeof UMB_PROPERTY_CONTEXT.TYPE) {
		this.observe(
			context.dataPath,
			(dataPath) => {
				if (dataPath) {
					// Set the data path for the local validation context:
					this.#validationContext.setDataPath(dataPath);
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
					super.value = undefined;
				} else {
					super.value = {
						...super.value,
						layout: { [UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS]: layouts },
						contentData: contents,
						settingsData: settings,
						expose: exposes,
					};
				}

				// If we don't have a value set from the outside or an internal value, we don't want to set the value.
				// This is added to prevent the block list from setting an empty value on startup.
				if (this.#lastValue === undefined && super.value === undefined) {
					return;
				}

				context.setValue(super.value);
			},
			'motherObserver',
		);

		// If the current property is readonly all inner block content should also be readonly.
		this.observe(
			observeMultiple([context.isReadOnly, context.variantId]),
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
	}

	protected override getFormElement() {
		return undefined;
	}

	override render() {
		return html`
			${repeat(
				this._layouts,
				(x) => x.contentKey,
				(layoutEntry, index) => html`
					${this.#renderInlineCreateButton(index)}
					<umb-block-list-entry
						.contentKey=${layoutEntry.contentKey}
						.layout=${layoutEntry}
						${umbDestroyOnDisconnect()}>
					</umb-block-list-entry>
				`,
			)}
			${this.#renderCreateButtonGroup()}
		`;
	}

	#renderCreateButtonGroup() {
		if (this.readonly && this._layouts.length > 0) {
			return nothing;
		} else {
			return html` <uui-button-group> ${this.#renderCreateButton()} ${this.#renderPasteButton()} </uui-button-group> `;
		}
	}

	#renderInlineCreateButton(index: number) {
		if (this.readonly) return nothing;
		return html`<uui-button-inline-create
			label=${this._createButtonLabel}
			href=${this._catalogueRouteBuilder?.({ view: 'create', index: index }) ?? ''}></uui-button-inline-create>`;
	}

	#renderCreateButton() {
		let createPath: string | undefined;
		if (this._blocks?.length === 1) {
			const elementKey = this._blocks[0].contentElementTypeKey;
			createPath =
				this._catalogueRouteBuilder?.({ view: 'create', index: -1 }) + 'modal/umb-modal-workspace/create/' + elementKey;
		} else {
			createPath = this._catalogueRouteBuilder?.({ view: 'create', index: -1 });
		}
		return html`
			<uui-button
				look="placeholder"
				label=${this._createButtonLabel}
				href=${createPath ?? ''}
				?disabled=${this.readonly}></uui-button>
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

export default UmbPropertyEditorUIBlockListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-list': UmbPropertyEditorUIBlockListElement;
	}
}
