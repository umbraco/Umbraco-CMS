import { UmbBlockListManagerContext } from '../../manager/block-list-manager.context.js';
import '../../components/block-list-block/index.js';
import { type UmbPropertyEditorUIBlockListBlockElement } from '../../components/block-list-block/index.js';
import { UMB_BLOCK_LIST_PROPERTY_EDITOR_ALIAS } from './manifests.js';
import { html, customElement, property, state, repeat, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import {
	UMB_BLOCK_CATALOGUE_MODAL,
	UmbBlockLayoutBaseModel,
	UmbBlockTypeBaseModel,
	type UmbBlockValueType,
} from '@umbraco-cms/backoffice/block';
import type { NumberRangeValueType } from '@umbraco-cms/backoffice/models';
import { UmbModalRouteBuilder, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbSorterConfig, UmbSorterController } from '@umbraco-cms/backoffice/sorter';

export interface UmbBlockListLayoutModel extends UmbBlockLayoutBaseModel {}

export interface UmbBlockListValueModel extends UmbBlockValueType<UmbBlockListLayoutModel> {}

const SORTER_CONFIG: UmbSorterConfig<UmbBlockListLayoutModel, UmbPropertyEditorUIBlockListBlockElement> = {
	compareElementToModel: (element, model) => {
		return element.getAttribute('data-udi') === model.contentUdi;
	},
	querySelectModelToElement: (container, modelEntry) => {
		return container.querySelector("umb-property-editor-ui-block-list-block[data-udi='" + modelEntry.contentUdi + "']");
	},
	identifier: 'block-list-editor',
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
			this.#context.setLayouts(model);
		},
	});

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

		this.#context.setLayouts(this._value.layout[UMB_BLOCK_LIST_PROPERTY_EDITOR_ALIAS] ?? []);
		this.#context.setContents(buildUpValue.contentData);
		this.#context.setSettings(buildUpValue.settingsData);
	}

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;
		const validationLimit = config.getValueByAlias<NumberRangeValueType>('validationLimit');

		this._limitMin = validationLimit?.min;
		this._limitMax = validationLimit?.max;

		const blocks = config.getValueByAlias<Array<UmbBlockTypeBaseModel>>('blocks') ?? [];
		this.#context.setBlockTypes(blocks);

		const useInlineEditingAsDefault = config.getValueByAlias<boolean>('useInlineEditingAsDefault');
		this.#context.setInlineEditingMode(useInlineEditingAsDefault);
		//config.useSingleBlockMode
		//config.useLiveEditing
		//config.useInlineEditingAsDefault
		this.style.maxWidth = config.getValueByAlias<string>('maxPropertyWidth') ?? '';

		this.#context.setEditorConfiguration(config);
	}

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;

	@state()
	private _blocks?: Array<UmbBlockTypeBaseModel>;

	@state()
	_layouts: Array<UmbBlockLayoutBaseModel> = [];

	@state()
	_catalogueRouteBuilder?: UmbModalRouteBuilder;

	#context = new UmbBlockListManagerContext(this);

	constructor() {
		super();

		// TODO: Prevent initial notification from these observes:
		this.observe(this.#context.layouts, (layouts) => {
			this._value = { ...this._value, layout: { [UMB_BLOCK_LIST_PROPERTY_EDITOR_ALIAS]: layouts } };
			// Notify that the value has changed.
			//console.log('layout changed', this._value);
			// TODO: idea: consider inserting an await here, so other changes could appear first? Maybe some mechanism to only fire change event onces?
			this._layouts = layouts;
			this.#sorter.setModel(layouts);
			this.dispatchEvent(new UmbChangeEvent());
		});
		this.observe(this.#context.contents, (contents) => {
			this._value = { ...this._value, contentData: contents };
			// Notify that the value has changed.
			//console.log('content changed', this._value);
			this.dispatchEvent(new UmbChangeEvent());
		});
		this.observe(this.#context.settings, (settings) => {
			this._value = { ...this._value, settingsData: settings };
			// Notify that the value has changed.
			//console.log('settings changed', this._value);
			this.dispatchEvent(new UmbChangeEvent());
		});
		this.observe(this.#context.blockTypes, (blockTypes) => {
			this._blocks = blockTypes;
		});

		new UmbModalRouteRegistrationController(this, UMB_BLOCK_CATALOGUE_MODAL)
			.addAdditionalPath(':view')
			.onSetup((routingInfo) => {
				return { data: { blocks: this._blocks ?? [], openClipboard: routingInfo.view === 'clipboard' } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._catalogueRouteBuilder = routeBuilder;
			});
	}

	render() {
		return html` ${repeat(
				this._layouts,
				(x) => x.contentUdi,
				(layoutEntry) =>
					html`<uui-button-inline-create></uui-button-inline-create>
						<umb-property-editor-ui-block-list-block data-udi=${layoutEntry.contentUdi} .layout=${layoutEntry}>
						</umb-property-editor-ui-block-list-block> `,
			)}
			<uui-button-group>
				<uui-button
					id="add-button"
					look="placeholder"
					label=${this.localize.term('content_createEmpty')}
					href=${this._catalogueRouteBuilder?.({ view: 'create' }) ?? ''}>
					${this.localize.term('content_createEmpty')}
				</uui-button>
				<uui-button
					label=${this.localize.term('content_createFromClipboard')}
					look="placeholder"
					href=${this._catalogueRouteBuilder?.({ view: 'clipboard' }) ?? ''}>
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
