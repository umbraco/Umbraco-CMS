import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, customElement, property, state, repeat, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyEditorUiElement, UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import type { UmbSorterConfig } from '@umbraco-cms/backoffice/sorter';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbBlockListEntriesContext } from '../../context/block-list-entries.context.js';
import type { UmbBlockListLayoutModel, UmbBlockListValueModel } from '../../types.js';
import type { UmbBlockListEntryElement } from '../../components/block-list-entry/index.js';
import { UmbBlockListManagerContext } from '../../context/block-list-manager.context.js';
import { UMB_BLOCK_LIST_PROPERTY_EDITOR_ALIAS } from './manifests.js';
import type { UmbBlockLayoutBaseModel } from '@umbraco-cms/backoffice/block';

import '../../components/block-list-entry/index.js';

const SORTER_CONFIG: UmbSorterConfig<UmbBlockListLayoutModel, UmbBlockListEntryElement> = {
	getUniqueOfElement: (element) => {
		return element.contentUdi!;
	},
	getUniqueOfModel: (modelEntry) => {
		return modelEntry.contentUdi;
	},
	//identifier: 'block-list-editor',
	itemSelector: 'umb-block-list-entry',
	//containerSelector: 'EMPTY ON PURPOSE, SO IT BECOMES THE HOST ELEMENT',
};

/**
 * @element umb-property-editor-ui-block-list
 */
@customElement('umb-property-editor-ui-block-list')
export class UmbPropertyEditorUIBlockListElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	#sorter = new UmbSorterController<UmbBlockListLayoutModel, UmbBlockListEntryElement>(this, {
		...SORTER_CONFIG,
		onChange: ({ model }) => {
			this.#entriesContext.setLayouts(model);
		},
	});

	//#catalogueModal: UmbModalRouteRegistrationController<typeof UMB_BLOCK_CATALOGUE_MODAL.DATA, undefined>;

	private _value: UmbBlockListValueModel = {
		layout: {},
		contentData: [],
		settingsData: [],
	};

	@property({ attribute: false })
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
	public get value(): UmbBlockListValueModel {
		return this._value;
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
			this._createButtonLabel = customCreateButtonLabel;
		} else if (blocks.length === 1) {
			this.#managerContext.contentTypesLoaded.then(() => {
				const firstContentTypeName = this.#managerContext.getContentTypeNameOf(blocks[0].contentElementTypeKey);
				this._createButtonLabel = `${this.localize.term('general_add')} ${firstContentTypeName}`;
			});
		}
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

		this.observe(this.#entriesContext.layoutEntries, (layouts) => {
			this._layouts = layouts;
			// Update sorter.
			this.#sorter.setModel(layouts);
			// Update manager:
			this.#managerContext.setLayouts(layouts);
		});

		// TODO: Prevent initial notification from these observes:
		this.observe(this.#managerContext.layouts, (layouts) => {
			this._value = { ...this._value, layout: { [UMB_BLOCK_LIST_PROPERTY_EDITOR_ALIAS]: layouts } };
			this.#fireChangeEvent();
		});
		this.observe(this.#managerContext.contents, (contents) => {
			this._value = { ...this._value, contentData: contents };
			this.#fireChangeEvent();
		});
		this.observe(this.#managerContext.settings, (settings) => {
			this._value = { ...this._value, settingsData: settings };
			this.#fireChangeEvent();
		});
		this.observe(this.#managerContext.blockTypes, (blockTypes) => {
			this._blocks = blockTypes;
		});

		this.observe(this.#entriesContext.catalogueRouteBuilder, (routeBuilder) => {
			this._catalogueRouteBuilder = routeBuilder;
		});
	}

	#fireChangeEvent = () => {
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	};

	override render() {
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
							label=${this._createButtonLabel}
							href=${this._catalogueRouteBuilder?.({ view: 'create', index: index }) ?? ''}></uui-button-inline-create>
						<umb-block-list-entry .contentUdi=${layoutEntry.contentUdi} .layout=${layoutEntry}>
						</umb-block-list-entry> `,
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

export default UmbPropertyEditorUIBlockListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-list': UmbPropertyEditorUIBlockListElement;
	}
}
