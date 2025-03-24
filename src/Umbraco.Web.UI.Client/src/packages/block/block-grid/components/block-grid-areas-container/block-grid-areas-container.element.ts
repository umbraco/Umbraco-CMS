import type { UmbBlockGridTypeAreaType } from '../../types.js';
import { UMB_BLOCK_GRID_ENTRY_CONTEXT } from '../block-grid-entry/constants.js';
import { UMB_BLOCK_GRID_MANAGER_CONTEXT } from '../../block-grid-manager/block-grid-manager.context-token.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';

import '../block-grid-entries/index.js';
/**
 * @description
 * This element is used to render the block grid areas.
 */
@customElement('umb-block-grid-areas-container')
export class UmbBlockGridAreasContainerElement extends UmbLitElement {
	//
	@state()
	_styleElement?: HTMLLinkElement;

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
					// Do not re-render stylesheet if its the same href.
					if (!stylesheet || this._styleElement?.href === stylesheet) return;
					this._styleElement = document.createElement('link');
					this._styleElement.rel = 'stylesheet';
					this._styleElement.href = stylesheet;
				},
				'observeStylesheet',
			);
		});
	}

	override render() {
		return this._areas && this._areas.length > 0
			? html` ${this._styleElement}
					<div
						class="umb-block-grid__area-container"
						part="area-container"
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

	static override styles = [
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
