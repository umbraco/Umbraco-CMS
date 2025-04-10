import type { UmbBlockElementManager } from './block-element-manager.js';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from './block-workspace.context-token.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbElementPropertyDatasetContext } from '@umbraco-cms/backoffice/content';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

export class UmbBlockElementPropertyDatasetContext
	extends UmbElementPropertyDatasetContext
	implements UmbPropertyDatasetContext
{
	readonly name;
	readonly culture;
	readonly segment;
	readonly getName;

	constructor(host: UmbControllerHost, elementManager: UmbBlockElementManager, variantId?: UmbVariantId) {
		// The controller alias, is a very generic name cause we want only one of these for this controller host.
		super(host, elementManager, variantId);

		// Ugly, but we just inherit these from the workspace context: [NL]
		this.name = elementManager.name;
		this.getName = elementManager.getName;
		this.culture = createObservablePart(elementManager.variantId, (v) => v?.culture);
		this.segment = createObservablePart(elementManager.variantId, (v) => v?.segment);

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (workspace) => {
			this.observe(
				workspace.readOnlyGuard.isPermittedForVariant(elementManager.getVariantId()),
				(isReadOnly) => {
					this._readOnly.setValue(isReadOnly);
				},
				'umbObserveReadOnlyStates',
			);
		});
	}
}
