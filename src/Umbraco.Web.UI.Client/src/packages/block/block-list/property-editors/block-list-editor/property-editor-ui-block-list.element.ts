import { UMB_BLOCK_LIST_PROPERTY_EDITOR_ALIAS } from './manifests.js';
import { html, customElement, property, state, styleMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbBlockManagerContext, type UmbBlockValueType } from '@umbraco-cms/backoffice/block';

/**
 * @element umb-property-editor-ui-block-list
 */
@customElement('umb-property-editor-ui-block-list')
export class UmbPropertyEditorUIBlockListElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	private _value?: UmbBlockValueType;

	@property({ attribute: false })
	public get value(): UmbBlockValueType | undefined {
		return this._value;
	}
	public set value(value: UmbBlockValueType | undefined) {
		const buildUpValue: Partial<UmbBlockValueType> = value ?? {};
		buildUpValue.layout ??= {};
		buildUpValue.contentData ??= [];
		buildUpValue.settingsData ??= [];
		this._value = buildUpValue as UmbBlockValueType;

		this.#context.setLayouts(this._value.layout[UMB_BLOCK_LIST_PROPERTY_EDITOR_ALIAS]);
		this.#context.setContents(buildUpValue.contentData);
		this.#context.setSettings(buildUpValue.settingsData);
	}

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;
		const validationLimit = config.find((x) => x.alias === 'validationLimit')?.value;

		this._limitMin = validationLimit?.min;
		this._limitMax = validationLimit?.max;

		const blocks = config.find((x) => x.alias === 'blocks')?.value;
		this.#context.setBlockTypes(blocks);
		//config.useSingleBlockMode
		//config.useLiveEditing
		//config.useInlineEditingAsDefault
		this.inlineStyles.width = config.find((x) => x.alias === 'maxPropertyWidth')?.value;
	}

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;

	#context = new UmbBlockManagerContext(this);

	@state()
	inlineStyles = { width: undefined };

	render() {
		return html`<div style=${styleMap(this.inlineStyles)}>umb-property-editor-ui-block-list</div>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIBlockListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-list': UmbPropertyEditorUIBlockListElement;
	}
}
