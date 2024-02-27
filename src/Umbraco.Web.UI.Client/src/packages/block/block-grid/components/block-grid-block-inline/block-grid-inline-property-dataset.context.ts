import type { UmbBlockGridEntryContext } from '../../context/block-grid-entry.context.js';
import { UMB_BLOCK_GRID_INLINE_PROPERTY_DATASET_CONTEXT } from './block-grid-inline-property-dataset.context-token.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbBlockGridInlinePropertyDatasetContext extends UmbBaseController implements UmbPropertyDatasetContext {
	#entryContext: UmbBlockGridEntryContext;

	// default data:

	getVariantId() {
		return UmbVariantId.CreateInvariant();
	}
	getEntityType() {
		return this.#entryContext.getEntityType();
	}
	getUnique() {
		return this.#entryContext.getEntityId();
	}

	getName(): string | undefined {
		return 'TODO: get label';
	}
	readonly name: Observable<string | undefined> = 'TODO: get label observable' as any;

	constructor(host: UmbControllerHost, entryContext: UmbBlockGridEntryContext) {
		// The controller alias, is a very generic name cause we want only one of these for this controller host.
		super(host, UMB_PROPERTY_DATASET_CONTEXT.toString());
		this.#entryContext = entryContext;

		this.provideContext(UMB_BLOCK_GRID_INLINE_PROPERTY_DATASET_CONTEXT, this);
	}

	/**
	 * TODO: Write proper JSDocs here.
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return await this.#entryContext.propertyValueByAlias<ReturnType>(propertyAlias);
	}

	/**
	 * TODO: Write proper JSDocs here.
	 */
	async setPropertyValue(propertyAlias: string, value: unknown) {
		return this.#entryContext.setPropertyValue(propertyAlias, value);
	}
}
