import type { UmbBlockGridScalableContainerContext } from '../../context/block-grid-scale-manager/block-grid-scale-manager.controller.js';
import { UMB_BLOCK_GRID_AREA_TYPE_ENTRIES_CONTEXT } from './block-grid-area-type-entries.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockGridAreaTypeEntriesContext
	extends UmbContextBase<UmbBlockGridAreaTypeEntriesContext>
	implements UmbBlockGridScalableContainerContext
{
	#layoutColumns?: number;
	setLayoutColumns(layoutColumns: number) {
		this.#layoutColumns = layoutColumns;
	}

	getLayoutColumns() {
		return this.#layoutColumns;
	}

	getLayoutContainerElement() {
		return this.getHostElement().shadowRoot?.querySelector('.umb-block-grid__area-container') as
			| HTMLElement
			| undefined;
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_GRID_AREA_TYPE_ENTRIES_CONTEXT);
	}
}
