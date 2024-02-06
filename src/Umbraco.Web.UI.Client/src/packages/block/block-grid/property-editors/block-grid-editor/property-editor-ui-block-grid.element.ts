import { html, customElement, property, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbBlockGridLayoutModel, UmbBlockTypeBaseModel, UmbBlockTypeGroup } from '@umbraco-cms/backoffice/block';
import type { NumberRangeValueType } from '@umbraco-cms/backoffice/models';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_ALIAS } from './manifests';

/**
 * @element umb-property-editor-ui-block-grid
 */
@customElement('umb-property-editor-ui-block-grid')
export class UmbPropertyEditorUIBlockGridElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property()
	value = '';

	@state()
	private _limitMin?: number;
	@state()
	private _limitMax?: number;

	@state()
	private _blocks?: Array<UmbBlockTypeBaseModel>;

	@state()
	private _blockGroups?: Array<UmbBlockTypeGroup>;

	@state()
	private _rootLayouts: Array<UmbBlockGridLayoutModel> = [];

	@state()
	private _directRoute?: string;

	@state()
	private _createButtonLabel = this.localize.term('blockEditor_addBlock');

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		const validationLimit = config.getValueByAlias<NumberRangeValueType>('validationLimit');

		this._limitMin = validationLimit?.min;
		this._limitMax = validationLimit?.max;

		this._blocks = config.getValueByAlias<Array<UmbBlockTypeBaseModel>>('blocks') ?? [];
		this._blockGroups = config.getValueByAlias<Array<UmbBlockTypeGroup>>('blockGroups') ?? [];

		const customCreateButtonLabel = config.getValueByAlias<string>('createLabel');
		if (customCreateButtonLabel) {
			this._createButtonLabel = customCreateButtonLabel;
		} else if (this._blocks.length === 1) {
			this._createButtonLabel = this.localize.term('blockEditor_addThis', [this._blocks[0].label]);
		}

		//const useInlineEditingAsDefault = config.getValueByAlias<boolean>('useInlineEditingAsDefault');

		//this.#context.setInlineEditingMode(useInlineEditingAsDefault);
		//config.useSingleBlockMode
		//config.useLiveEditing
		//config.useInlineEditingAsDefault
		this.style.maxWidth = config.getValueByAlias<string>('maxPropertyWidth') ?? '';

		//this.#context.setEditorConfiguration(config);
	}

	#context = new UmbBlockGridManagerContext(this);

	constructor() {
		super();

		// TODO: Prevent initial notification from these observes:
		this.observe(this.#context.layouts, (layouts) => {
			this._value = { ...this._value, layout: { [UMB_BLOCK_GRID_PROPERTY_EDITOR_ALIAS]: layouts } };
			// Notify that the value has changed.
			//console.log('layout changed', this._value);
			// TODO: idea: consider inserting an await here, so other changes could appear first? Maybe some mechanism to only fire change event onces?
			this._rootLayouts = layouts;
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

	render() {
		return html`<umb-property-editor-ui-block-grid-entries
			.layoutEntries=${this._rootLayouts}></umb-property-editor-ui-block-grid-entries>`;
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

export default UmbPropertyEditorUIBlockGridElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid': UmbPropertyEditorUIBlockGridElement;
	}
}
