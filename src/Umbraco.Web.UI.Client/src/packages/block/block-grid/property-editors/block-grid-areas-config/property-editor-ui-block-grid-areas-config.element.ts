import { UMB_BLOCK_GRID_DEFAULT_LAYOUT_STYLESHEET, type UmbBlockGridTypeAreaType } from '../../index.js';
import { UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_MODAL } from '../../components/block-grid-area-config-entry/constants.js';
import { UmbBlockGridAreaTypeEntriesContext } from './block-grid-area-type-entries.context.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, customElement, property, state, repeat,css } from '@umbraco-cms/backoffice/external/lit';
import type {
	UmbPropertyEditorUiElement,
	UmbPropertyEditorConfigCollection,
} from '@umbraco-cms/backoffice/property-editor';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { incrementString } from '@umbraco-cms/backoffice/utils';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

import '../../components/block-grid-area-config-entry/block-grid-area-config-entry.element.js';
@customElement('umb-property-editor-ui-block-grid-areas-config')
export class UmbPropertyEditorUIBlockGridAreasConfigElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	//
	#context = new UmbBlockGridAreaTypeEntriesContext(this);
	// local vars:
	#defaultAreaGridColumns: number = 12;
	#valueOfAreaGridColumns?: number | null;

	@property({ type: Array })
	public set value(value: Array<UmbBlockGridTypeAreaType>) {
		this._value = value ?? [];
	}
	public get value(): Array<UmbBlockGridTypeAreaType> {
		return this._value;
	}

	@state()
	private _value: Array<UmbBlockGridTypeAreaType> = [];

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

	@state()
	private _draggedIndex?: number = undefined;

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_MODAL)
			.addAdditionalPath('block-grid-area-type')
			.onSetup(() => {
				if (!this._areaGridColumns) return false;
				const halfGridColumns = this._areaGridColumns * 0.5;
				const columnSpan = halfGridColumns === Math.round(halfGridColumns) ? halfGridColumns : this._areaGridColumns;

				return {
					data: {
						entityType: 'block-grid-area-type',
						preset: { columnSpan, alias: this.#generateUniqueAreaAlias('area') },
					},
					modal: { size: 'large' },
				};
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
		this.#context.setLayoutColumns(this._areaGridColumns);
	}

	#generateUniqueAreaAlias(alias: string) {
		while (this._value.find((area) => area.alias === alias)) {
			alias = incrementString(alias);
		}
		return alias;
	}

	#handleDrop(targetIndex: number) {
		if (!this._draggedIndex || this._draggedIndex === targetIndex) return;
	
		const newValue = [...this.value];
		const [movedItem] = newValue.splice(this._draggedIndex, 1);
		newValue.splice(targetIndex, 0, movedItem);
	
		this.value = newValue;
		this._draggedIndex = undefined;
	}

	override render() {
		
		return this._areaGridColumns
			? html`${this._styleElement}
					<div
						class="umb-block-grid__area-container"
						part="area-container"
						style="--umb-block-grid--area-grid-columns: ${this._areaGridColumns}">
						${repeat(
							this.value,
							(area) => area.key,
							(area, index) =>
								html`<umb-block-area-config-entry
									class="umb-block-grid__area"
									draggable="true"
									.workspacePath=${this._workspacePath}
									.areaGridColumns=${this._areaGridColumns}
									@dragstart=${(e: any) => {
										this._draggedIndex = index;
										e.dataTransfer.effectAllowed = 'move';
									}}
									@dragover=${(e: any) => e.preventDefault()}
									@drop=${() => this.#handleDrop(index)}
									.key=${area.key}></umb-block-area-config-entry>`,
						)}
					</div>
					<uui-button look="placeholder" label=${'Add area'} href=${this._workspacePath + 'create'}></uui-button>`
			: '';
	}

	static override styles = [
		UmbTextStyles,
		css`
			.umb-block-grid__area{
				cursor: move;
			}
		`
	]
}

export default UmbPropertyEditorUIBlockGridAreasConfigElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-areas-config': UmbPropertyEditorUIBlockGridAreasConfigElement;
	}
}
