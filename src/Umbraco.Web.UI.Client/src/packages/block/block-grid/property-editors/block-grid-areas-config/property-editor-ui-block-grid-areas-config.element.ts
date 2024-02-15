import type { UmbBlockGridTypeAreaType } from '../../index.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, customElement, property, css, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbId } from '@umbraco-cms/backoffice/id';

@customElement('umb-property-editor-ui-block-grid-areas-config')
export class UmbPropertyEditorUIBlockGridAreasConfigElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	// local vars:
	#defaultAreaGridColumns: number = 12;
	#valueOfAreaGridColumns?: number | null;

	@property({ type: Array })
	value: Array<UmbBlockGridTypeAreaType> = [];

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
	_areaGridColumns?: number;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (context) => {
			this.observe(await context.propertyValueByAlias('areaGridColumns'), (value) => {
				// Value can be undefined, but 'undefined > 0' is still valid JS and will return false. [NL]
				this.#valueOfAreaGridColumns = (value as number) > 0 ? (value as number) : null;
				this.#gotAreaColumns();
			});
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

		this.value.push({
			key: UmbId.new(),
			alias: '', // TODO: Should we auto generate something here?
			columnSpan: columnSpan,
			rowSpan: 1,
			minAllowed: 0,
			maxAllowed: undefined,
			specifiedAllowance: [],
		});

		//TODO: vm.openAreaOverlay(newArea);
	}

	render() {
		return this._areaGridColumns
			? html`<div
						class="umb-block-grid__area-container"
						style="--umb-block-grid--area-grid-columns: ${this._areaGridColumns}">
						${repeat(
							this.value,
							(area) => area.key,
							(area) =>
								html`<umb-block-grid-area-placeholder
									class="umb-block-grid__area"
									.areaKey=${area.key}></umb-block-grid-area-placeholder>`,
						)}
					</div>
					<uui-button
						id="add-button"
						look="placeholder"
						label=${this.localize.term('blockEditor_addBlock')}
						@click=${this.#addNewArea}>
					</uui-button>`
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
