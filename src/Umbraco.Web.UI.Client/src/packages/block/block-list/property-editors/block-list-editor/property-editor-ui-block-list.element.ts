import { UmbBlockListManagerContext } from '../../context/block-list-manager.context.js';
import '../../components/block-list-block/index.js';
import type { UmbPropertyEditorUIBlockListBlockElement } from '../../components/block-list-block/index.js';
import type { UmbBlockListLayoutModel, UmbBlockListValueModel } from '../../types.js';
import { UmbBlockListEntriesContext } from '../../context/block-list-entries.context.js';
import { UMB_BLOCK_LIST_PROPERTY_EDITOR_ALIAS } from './manifests.js';
import { html, customElement, property, state, repeat, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UMB_BLOCK_CATALOGUE_MODAL } from '@umbraco-cms/backoffice/block';
import type { UmbBlockLayoutBaseModel, UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block';
import type { NumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/modal';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbSorterConfig } from '@umbraco-cms/backoffice/sorter';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';

const SORTER_CONFIG: UmbSorterConfig<UmbBlockListLayoutModel, UmbPropertyEditorUIBlockListBlockElement> = {
	getUniqueOfElement: (element) => {
		return element.getAttribute('data-udi');
	},
	getUniqueOfModel: (modelEntry) => {
		return modelEntry.contentUdi;
	},
	//identifier: 'block-list-editor',
	itemSelector: 'umb-property-editor-ui-block-list-block',
	//containerSelector: 'EMPTY ON PURPOSE, SO IT BECOMES THE HOST ELEMENT',
};

/**
 * @element umb-property-editor-ui-block-list
 */
@customElement('umb-property-editor-ui-block-list')
export class UmbPropertyEditorUIBlockListElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	#sorter = new UmbSorterController<UmbBlockListLayoutModel, UmbPropertyEditorUIBlockListBlockElement>(this, {
		...SORTER_CONFIG,
		onChange: ({ model }) => {
			//this.#entriesContext.setLayoutEntries(model);
		},
	});

	#catalogueModal: UmbModalRouteRegistrationController<typeof UMB_BLOCK_CATALOGUE_MODAL.DATA, undefined>;

	private _value: UmbBlockListValueModel = {
		layout: {},
		contentData: [],
		settingsData: [],
	};

	@property({ attribute: false })
	public get value(): UmbBlockListValueModel {
		return this._value;
	}
	public set value(value: UmbBlockListValueModel | undefined) {
		const buildUpValue: Partial<UmbBlockListValueModel> = value ? { ...value } : {};
		buildUpValue.layout ??= {};
		buildUpValue.contentData ??= [];
		buildUpValue.settingsData ??= [];
		this._value = buildUpValue as UmbBlockListValueModel;

		this.#managerContext.setLayouts(this._value.layout[UMB_BLOCK_LIST_PROPERTY_EDITOR_ALIAS] ?? []);
		this.#managerContext.setContents(buildUpValue.contentData);
		this.#managerContext.setSettings(buildUpValue.settingsData);
	}

	@state()
	private _createButtonLabel = this.localize.term('content_createEmpty');

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const validationLimit = config.getValueByAlias<NumberRangeValueType>('validationLimit');

		this._limitMin = validationLimit?.min;
		this._limitMax = validationLimit?.max;

		const blocks = config.getValueByAlias<Array<UmbBlockTypeBaseModel>>('blocks') ?? [];
		this.#managerContext.setBlockTypes(blocks);

		const customCreateButtonLabel = config.getValueByAlias<string>('createLabel');
		if (customCreateButtonLabel) {
			this._createButtonLabel = customCreateButtonLabel;
		} else if (blocks.length === 1) {
			this._createButtonLabel = `${this.localize.term('general_add')} ${blocks[0].label}`;
		}

		const useInlineEditingAsDefault = config.getValueByAlias<boolean>('useInlineEditingAsDefault');
		this.#managerContext.setInlineEditingMode(useInlineEditingAsDefault);
		// TODO:
		//config.useSingleBlockMode, not done jey
		this.style.maxWidth = config.getValueByAlias<string>('maxPropertyWidth') ?? '';

		this.#managerContext.setEditorConfiguration(config);
	}

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

	#managerContext = new UmbBlockListManagerContext(this);
	#entriesContext = new UmbBlockListEntriesContext(this);

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_CONTEXT, (propertyContext) => {
			this.observe(
				propertyContext?.alias,
				(alias) => {
					this.#catalogueModal.setUniquePathValue('propertyAlias', alias);
				},
				'observePropertyAlias',
			);
		});

		// TODO: Prevent initial notification from these observes:
		this.observe(this.#managerContext.layouts, (layouts) => {
			this._value = { ...this._value, layout: { [UMB_BLOCK_LIST_PROPERTY_EDITOR_ALIAS]: layouts } };
			// Notify that the value has changed.
			//console.log('layout changed', this._value);
			// TODO: idea: consider inserting an await here, so other changes could appear first? Maybe some mechanism to only fire change event onces?
			//this.#entriesContext.setLayoutEntries(layouts);
			this.dispatchEvent(new UmbChangeEvent());
		});
		this.observe(this.#entriesContext.layoutEntries, (layouts) => {
			this._layouts = layouts;
			// Update sorter.
			this.#sorter.setModel(layouts);
			// Update manager:
			this.#managerContext.setLayouts(layouts);
		});
		this.observe(this.#managerContext.contents, (contents) => {
			this._value = { ...this._value, contentData: contents };
			// Notify that the value has changed.
			//console.log('content changed', this._value);
			this.dispatchEvent(new UmbChangeEvent());
		});
		this.observe(this.#managerContext.settings, (settings) => {
			this._value = { ...this._value, settingsData: settings };
			// Notify that the value has changed.
			//console.log('settings changed', this._value);
			this.dispatchEvent(new UmbChangeEvent());
		});
		this.observe(this.#managerContext.blockTypes, (blockTypes) => {
			this._blocks = blockTypes;
		});

		this.#catalogueModal = new UmbModalRouteRegistrationController(this, UMB_BLOCK_CATALOGUE_MODAL)
			.addUniquePaths(['propertyAlias'])
			.addAdditionalPath(':view/:index')
			.onSetup((routingInfo) => {
				const index = routingInfo.index ? parseInt(routingInfo.index) : -1;
				return {
					data: {
						blocks: this._blocks ?? [],
						openClipboard: routingInfo.view === 'clipboard',
						blockOriginData: { index: index },
					},
				};
			})
			.observeRouteBuilder((routeBuilder) => {
				this._catalogueRouteBuilder = routeBuilder;
			});
	}

	render() {
		let createPath: string | undefined;
		if (this._blocks?.length === 1) {
			const elementKey = this._blocks[0].contentElementTypeKey;
			createPath =
				this._catalogueRouteBuilder?.({ view: 'create', index: -1 }) + 'modal/umb-modal-workspace/create/' + elementKey;
		} else {
			createPath = this._catalogueRouteBuilder?.({ view: 'create', index: -1 });
		}
		return html` ${repeat(
				this._layouts,
				(x) => x.contentUdi,
				(layoutEntry, index) =>
					html`<uui-button-inline-create
							href=${this._catalogueRouteBuilder?.({ view: 'create', index: index }) ?? ''}></uui-button-inline-create>
						<umb-property-editor-ui-block-list-block data-udi=${layoutEntry.contentUdi} .layout=${layoutEntry}>
						</umb-property-editor-ui-block-list-block> `,
			)}
			<uui-button-group>
				<uui-button
					id="add-button"
					look="placeholder"
					label=${this._createButtonLabel}
					href=${createPath ?? ''}></uui-button>
				<uui-button
					label=${this.localize.term('content_createFromClipboard')}
					look="placeholder"
					href=${this._catalogueRouteBuilder?.({ view: 'clipboard', index: -1 }) ?? ''}>
					<uui-icon name="icon-paste-in"></uui-icon>
				</uui-button>
			</uui-button-group>`;
	}

	static styles = [
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
