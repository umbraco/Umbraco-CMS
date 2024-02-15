import type { UmbBlockGridTypeAreaType } from '../../index.js';
import { UMB_BLOCK_GRID_DEFAULT_LAYOUT_STYLESHEET } from '../../context/block-grid-manager.context.js';
import { UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_MODAL } from '../../components/block-grid-area-config-entry/index.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, customElement, property, css, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import {
	UmbPropertyValueChangeEvent,
	type UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';

@customElement('umb-property-editor-ui-block-grid-areas-config')
export class UmbPropertyEditorUIBlockGridAreasConfigElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	// local vars:
	#defaultAreaGridColumns: number = 12;
	#valueOfAreaGridColumns?: number | null;

	#workspaceModal: UmbModalRouteRegistrationController;

	@property({ type: Array })
	public get value(): Array<UmbBlockGridTypeAreaType> {
		return this._value;
	}
	public set value(value: Array<UmbBlockGridTypeAreaType>) {
		this._value = value ?? [];
	}
	@state()
	private _value: Array<UmbBlockGridTypeAreaType> = [];

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		const defaultAreaGridColumns = config?.getValueByAlias('defaultAreaGridColumns');
		if (typeof defaultAreaGridColumns === 'number' && defaultAreaGridColumns > 0) {
			this.#defaultAreaGridColumns = defaultAreaGridColumns ?? null;
		} else {
			this.#defaultAreaGridColumns = 12;
		}
		this.#gotAreaColumns();
	}

	@state()
	private _workspacePath?: string;

	@state()
	private _styleElement?: HTMLLinkElement;

	@state()
	private _areaGridColumns?: number;

	constructor() {
		super();

		this.#workspaceModal = new UmbModalRouteRegistrationController(this, UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_MODAL)
			.addAdditionalPath('block-grid-area-type')
			.onSetup(() => {
				return { data: { entityType: 'block-grid-area-type', preset: {} }, modal: { size: 'large' } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._workspacePath = routeBuilder({});
			});

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (context) => {
			this.observe(
				await context.propertyValueByAlias<number | undefined>('areaGridColumns'),
				(value) => {
					// Value can be undefined, but 'undefined > 0' is still valid JS and will return false. [NL]
					this.#valueOfAreaGridColumns = (value as number) > 0 ? value : null;
					this.#gotAreaColumns();
				},
				'observeAreaGridColumns',
			);

			this.observe(
				await context.propertyValueByAlias<string | undefined>('layoutStylesheet'),
				(stylesheet) => {
					if (this._styleElement && this._styleElement.href === stylesheet) return;
					this._styleElement = document.createElement('link');
					this._styleElement.setAttribute('rel', 'stylesheet');
					this._styleElement.setAttribute('href', stylesheet ?? UMB_BLOCK_GRID_DEFAULT_LAYOUT_STYLESHEET);
				},
				'observeStylesheet',
			);
		});
	}

	#gotAreaColumns() {
		if (!this.#defaultAreaGridColumns || this.#valueOfAreaGridColumns === undefined) return;
		this._areaGridColumns = this.#valueOfAreaGridColumns ?? this.#defaultAreaGridColumns;
	}

	#addNewArea() {
		if (!this._areaGridColumns) return;
		const halfGridColumns = this._areaGridColumns * 0.5;
		const columnSpan = halfGridColumns === Math.round(halfGridColumns) ? halfGridColumns : this._areaGridColumns;

		this._value = [
			...this._value,
			{
				key: UmbId.new(),
				alias: '', // TODO: Should we auto generate something here?
				columnSpan: columnSpan,
				rowSpan: 1,
				minAllowed: 0,
				maxAllowed: undefined,
				specifiedAllowance: [],
			},
		];
		this.requestUpdate('_value');
		this.dispatchEvent(new UmbPropertyValueChangeEvent());

		//TODO: open area edit workspace
	}

	// TODO: Needs localizations:
	render() {
		return this._areaGridColumns
			? html`${this._styleElement}
					<div
						class="umb-block-grid__area-container"
						style="--umb-block-grid--area-grid-columns: ${this._areaGridColumns}">
						${repeat(
							this.value,
							(area) => area.key,
							(area) =>
								html`<umb-block-area-config-entry
									class="umb-block-grid__area"
									.workspacePath=${this._workspacePath}
									.key=${area.key}></umb-block-area-config-entry>`,
						)}
					</div>
					<uui-button id="add-button" look="placeholder" label=${'Add area'} @click=${this.#addNewArea}></uui-button>`
			: '';
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbPropertyEditorUIBlockGridAreasConfigElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-areas-config': UmbPropertyEditorUIBlockGridAreasConfigElement;
	}
}
