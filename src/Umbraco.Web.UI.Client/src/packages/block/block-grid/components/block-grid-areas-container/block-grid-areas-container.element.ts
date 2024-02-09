import { UMB_BLOCK_GRID_ENTRY_CONTEXT, type UmbBlockGridLayoutAreaItemModel } from '@umbraco-cms/backoffice/block';
import { css, customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import '../block-grid-entries/index.js';

/**
 * @element umb-block-grid-area-container
 */
@customElement('umb-block-grid-area-container')
export class UmbBlockGridAreaContainerElement extends UmbLitElement {
	//

	@state()
	_areas?: Array<UmbBlockGridLayoutAreaItemModel> = [];

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
		});
	}

	render() {
		return this._areas
			? repeat(
					this._areas,
					(area) => area.key,
					(area) => html` <umb-block-grid-entries .areaKey=${area.key}> </umb-block-grid-entries>`,
			  )
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

export default UmbBlockGridAreaContainerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-area-container': UmbBlockGridAreaContainerElement;
	}
}
