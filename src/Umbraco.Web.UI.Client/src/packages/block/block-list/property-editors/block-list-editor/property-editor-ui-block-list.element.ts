import { UMB_BLOCK_LIST_PROPERTY_EDITOR_ALIAS } from './manifests.js';
import { html, customElement, property, state, repeat, css, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import {
	UMB_BLOCK_CATALOGUE_MODAL,
	UmbBlockLayoutBaseModel,
	UmbBlockManagerContext,
	UmbBlockTypeBase,
	type UmbBlockValueType,
} from '@umbraco-cms/backoffice/block';
import '../../components/block-list-block/index.js';
import { buildUdi } from '@umbraco-cms/backoffice/utils';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { NumberRangeValueType } from '@umbraco-cms/backoffice/models';
import { UMB_MODAL_MANAGER_CONTEXT_TOKEN, UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

export interface UmbBlockListLayoutModel extends UmbBlockLayoutBaseModel {}

export interface UmbBlockListValueModel extends UmbBlockValueType<UmbBlockListLayoutModel> {}

/**
 * @element umb-property-editor-ui-block-list
 */
@customElement('umb-property-editor-ui-block-list')
export class UmbPropertyEditorUIBlockListElement extends UmbLitElement implements UmbPropertyEditorUiElement {
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
		const buildUpValue: Partial<UmbBlockListValueModel> = { ...value } ?? {};
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

		const blocks = config.getValueByAlias<Array<UmbBlockTypeBase>>('blocks') ?? [];
		this.#context.setBlockTypes(blocks);
		//config.useSingleBlockMode
		//config.useLiveEditing
		//config.useInlineEditingAsDefault
		this.style.maxWidth = config.getValueByAlias<string>('maxPropertyWidth') ?? '';
	}

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;

	@state()
	private _blocks?: Array<UmbBlockTypeBase>;

	#context = new UmbBlockManagerContext(this);

	@state()
	_layouts: Array<UmbBlockLayoutBaseModel> = [];

	#modalContext?: UmbModalManagerContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});

		// TODO: Prevent initial notification from these observes:
		this.observe(this.#context.layouts, (layouts) => {
			this._value = { ...this._value, layout: { [UMB_BLOCK_LIST_PROPERTY_EDITOR_ALIAS]: layouts } };
			// Notify that the value has changed.
			//console.log('layout changed', this._value);

			// TODO: idea: consider inserting an await here, so other changes could appear first? Maybe some mechanism to only fire change event onces?
			this._layouts = layouts;
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
	}

	async #openBlockCatalogue(openClipboard: boolean = false) {
		//Open modal
		const modalContext = this.#modalContext?.open(UMB_BLOCK_CATALOGUE_MODAL, {
			data: { blocks: this._blocks ?? [], openClipboard },
		});

		const data = await modalContext?.onSubmit();

		/**TODO: Insert next modal for data */
		console.log('submitted', data);

		if (!data) return;

		const block = this._blocks?.find((x) => x.contentElementTypeKey === data.key);

		if (!block?.contentElementTypeKey) return;

		this.#context.createBlock(
			{
				contentUdi: buildUdi('element', UmbId.new()),
				settingsUdi: buildUdi('element', UmbId.new()),
			},
			block.contentElementTypeKey,
		);
	}

	render() {
		return html` ${repeat(
				this._layouts,
				(x) => x.contentUdi,
				(layoutEntry) =>
					html`<uui-button-inline-create></uui-button-inline-create>
						<umb-property-editor-ui-block-list-block .layout=${layoutEntry}>
						</umb-property-editor-ui-block-list-block> `,
			)}
			<uui-button-group>
				<uui-button
					id="add-button"
					look="placeholder"
					label=${this.localize.term('content_createEmpty')}
					@click=${() => this.#openBlockCatalogue()}>
					${this.localize.term('content_createEmpty')}
				</uui-button>
				<uui-button
					label=${this.localize.term('content_createFromClipboard')}
					look="placeholder"
					@click=${() => this.#openBlockCatalogue(true)}>
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
