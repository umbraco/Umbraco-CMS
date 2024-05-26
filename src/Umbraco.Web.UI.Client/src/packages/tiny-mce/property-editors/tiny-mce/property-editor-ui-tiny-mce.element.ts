import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from './manifests.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

import '../../components/input-tiny-mce/input-tiny-mce.element.js';
import {
	UmbBlockRteEntriesContext,
	type UmbBlockRteLayoutModel,
	UmbBlockRteManagerContext,
	type UmbBlockRteTypeModel,
} from '@umbraco-cms/backoffice/block-rte';
import type { UmbBlockValueType } from '@umbraco-cms/backoffice/block';

export interface UmbRichTextEditorValueType {
	markup: string;
	blocks: UmbBlockValueType<UmbBlockRteLayoutModel>;
}

/**
 * @element umb-property-editor-ui-tiny-mce
 */
@customElement('umb-property-editor-ui-tiny-mce')
export class UmbPropertyEditorUITinyMceElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	// No need to registerer as a LIT-property, as we are calling it directly and no need for it to be reactive [NL]
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._config = config;

		const blocks = config.getValueByAlias<Array<UmbBlockRteTypeModel>>('blocks') ?? [];
		this.#managerContext.setBlockTypes(blocks);

		this.#managerContext.setEditorConfiguration(config);
	}

	@property({ attribute: false })
	public set value(value: UmbRichTextEditorValueType | undefined) {
		const buildUpValue: Partial<UmbRichTextEditorValueType> = value ? { ...value } : {};
		buildUpValue.markup ??= '';
		buildUpValue.blocks ??= { layout: {}, contentData: [], settingsData: [] };
		buildUpValue.blocks.layout ??= {};
		buildUpValue.blocks.contentData ??= [];
		buildUpValue.blocks.settingsData ??= [];
		this._value = buildUpValue as UmbRichTextEditorValueType;

		this.#managerContext.setLayouts(buildUpValue.blocks.layout[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? []);
		this.#managerContext.setContents(buildUpValue.blocks.contentData);
		this.#managerContext.setSettings(buildUpValue.blocks.settingsData);
	}
	public get value(): UmbRichTextEditorValueType {
		return this._value;
	}

	@state()
	_config?: UmbPropertyEditorConfigCollection;

	@state()
	private _value: UmbRichTextEditorValueType = {
		markup: '',
		blocks: { layout: {}, contentData: [], settingsData: [] },
	};

	#managerContext = new UmbBlockRteManagerContext(this);
	#entriesContext = new UmbBlockRteEntriesContext(this);

	constructor() {
		super();

		this.observe(this.#managerContext.layouts, (layouts) => {
			this._value = {
				...this._value,
				blocks: { ...this._value.blocks, layout: { [UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]: layouts } },
			};
			this.#fireChangeEvent();
		});
		this.observe(this.#managerContext.contents, (contents) => {
			this._value = { ...this._value, blocks: { ...this._value.blocks, contentData: contents } };
			this.#fireChangeEvent();
		});
		this.observe(this.#managerContext.settings, (settings) => {
			this._value = { ...this._value, blocks: { ...this._value.blocks, settingsData: settings } };
			this.#fireChangeEvent();
		});
	}

	#fireChangeEvent() {
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onChange(event: InputEvent & { target: HTMLInputElement }) {
		this._value = {
			...this._value,
			markup: event.target.value,
		};
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`
			<umb-input-tiny-mce .configuration=${this._config} .value=${this._value?.markup ?? ''} @change=${this.#onChange}>
			</umb-input-tiny-mce>
		`;
	}
}

export default UmbPropertyEditorUITinyMceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce': UmbPropertyEditorUITinyMceElement;
	}
}
