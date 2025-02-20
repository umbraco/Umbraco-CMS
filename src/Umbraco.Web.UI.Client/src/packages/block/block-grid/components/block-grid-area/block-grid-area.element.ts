import { UMB_BLOCK_GRID_ENTRIES_CONTEXT } from '../../context/block-grid-entries.context-token.js';
import { UmbBlockGridEntriesElement } from '../block-grid-entries/index.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @description
 * This element is used to render a single block grid area.
 */
@customElement('umb-block-grid-area')
export class UmbBlockGridAreasContainerElement extends UmbBlockGridEntriesElement {
	//
	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_GRID_ENTRIES_CONTEXT, (context) => {
			this.observe(
				context.layoutColumns,
				(layoutColumns) => {
					this.layoutColumns = layoutColumns;
				},
				'observeParentEntriesLayoutColumns',
			);
		}).skipHost();
	}

	override connectedCallback(): void {
		super.connectedCallback();
		// eslint-disable-next-line wc/no-self-class
		this.classList.add('umb-block-grid__area');
	}
}

export default UmbBlockGridAreasContainerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-area': UmbBlockGridAreasContainerElement;
	}
}
