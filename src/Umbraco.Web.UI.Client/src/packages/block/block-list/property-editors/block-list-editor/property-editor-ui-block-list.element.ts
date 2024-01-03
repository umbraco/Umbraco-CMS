import { UMB_BLOCK_LIST_PROPERTY_EDITOR_ALIAS } from './manifests.js';
import { html, customElement, property, state, styleMap, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import {
	UmbBlockLayoutBaseModel,
	UmbBlockManagerContext,
	UmbBlockTypeBase,
	type UmbBlockValueType,
} from '@umbraco-cms/backoffice/block';
import './block-list-block.js';
import { buildUdi } from '@umbraco-cms/backoffice/utils';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { NumberRangeValueType } from '@umbraco-cms/backoffice/models';

export interface UmbBlockListLayoutModel extends UmbBlockLayoutBaseModel {}

export interface UmbBlockListValueModel extends UmbBlockValueType<UmbBlockListLayoutModel> {}

/**
 * @element umb-property-editor-ui-block-list
 */
@customElement('umb-property-editor-ui-block-list')
export class UmbPropertyEditorUIBlockListElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	private _value?: UmbBlockListValueModel;

	@property({ attribute: false })
	public get value(): UmbBlockListValueModel | undefined {
		return this._value;
	}
	public set value(value: UmbBlockListValueModel | undefined) {
		const buildUpValue: Partial<UmbBlockListValueModel> = value ?? {};
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
		this._inlineStyles.width = config.getValueByAlias<string>('maxPropertyWidth');
	}

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;

	#context = new UmbBlockManagerContext(this);

	@state()
	_inlineStyles: { [key: string]: string | undefined } = { width: undefined };

	@state()
	_layouts: Array<UmbBlockLayoutBaseModel> = [];

	#openBlockCatalogue() {
		// Open modal.

		// TEMP Hack:

		const contentElementTypeKey = this.#context.getBlockTypes()[0]!.contentElementTypeKey;

		console.log('about to create', contentElementTypeKey);

		const contentUdi = buildUdi('element', UmbId.new());
		const settingsUdi = buildUdi('element', UmbId.new());

		this.#context.createBlock(
			{
				contentUdi,
				settingsUdi,
			},
			contentElementTypeKey,
		);
	}

	render() {
		return html`<div style=${styleMap(this._inlineStyles)}>
			${repeat(
				this._layouts,
				(x) => x.contentUdi,
				(layoutEntry) =>
					html`<umb-property-editor-ui-block-list-block .layout=${layoutEntry}>
					</umb-property-editor-ui-block-list-block>`,
			)}
			<uui-button id="add-button" look="placeholder" @click=${this.#openBlockCatalogue} label="open">Add</uui-button>
		</div>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIBlockListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-list': UmbPropertyEditorUIBlockListElement;
	}
}
