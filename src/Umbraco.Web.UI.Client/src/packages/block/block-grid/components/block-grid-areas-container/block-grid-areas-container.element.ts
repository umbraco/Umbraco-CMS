import { UMB_BLOCK_GRID_MANAGER_CONTEXT } from '../../context/block-grid-manager.context.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_BLOCK_GRID_ENTRY_CONTEXT, type UmbBlockGridTypeAreaType } from '@umbraco-cms/backoffice/block-grid';
import { css, customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';

import '../block-grid-entries/index.js';
/**
 * @element umb-block-grid-areas-container
 * @description
 * This element is used to render the block grid areas.
 */
@customElement('umb-block-grid-areas-container')
export class UmbBlockGridAreasContainerElement extends UmbLitElement {
	//
	#styleElement?: HTMLLinkElement;

	@state()
	_areas?: Array<UmbBlockGridTypeAreaType> = [];

	@state()
	_areaGridColumns?: number;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_GRID_ENTRY_CONTEXT, (context) => {
			this.observe(
				context.areas,
				(areas) => {
					this._areas = areas;
				},
				'observeAreas',
			);
			this.observe(
				context.areaGridColumns,
				(areaGridColumns) => {
					this._areaGridColumns = areaGridColumns;
					//this.requestUpdate('_areaGridColumns');
				},
				'observeAreaGridColumns',
			);
		});
		this.consumeContext(UMB_BLOCK_GRID_MANAGER_CONTEXT, (manager) => {
			this.observe(
				manager.layoutStylesheet,
				(stylesheet) => {
					this.#styleElement = document.createElement('link');
					this.#styleElement.setAttribute('rel', 'stylesheet');
					this.#styleElement.setAttribute('href', stylesheet);
				},
				'observeStylesheet',
			);
		});
	}

	render() {
		return this._areas && this._areas.length > 0
			? html` ${this.#styleElement}
					<div
						class="umb-block-grid__area-container"
						style="--umb-block-grid--area-grid-columns: ${this._areaGridColumns}">
						${repeat(
							this._areas,
							(area) => area.key,
							(area) =>
								html`<umb-block-grid-entries
									part="area"
									class="umb-block-grid__area"
									.areaKey=${area.key}
									.layoutColumns=${area.columnSpan}></umb-block-grid-entries>`,
						)}
					</div>`
			: '';
	}

	static styles = [
		css`
			:host {
				display: block;
				width: 100%;
			}
		`,
	];
}

export default UmbBlockGridAreasContainerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-areas-container': UmbBlockGridAreasContainerElement;
	}
}
