import type { UmbBlockElementManager } from './block-element-manager.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbElementPropertyDatasetContext } from '@umbraco-cms/backoffice/content';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';

export class UmbBlockElementPropertyDatasetContext
	extends UmbElementPropertyDatasetContext
	implements UmbPropertyDatasetContext
{
	name;
	culture;
	segment;

	getName(): string {
		return 'Block';
	}

	constructor(host: UmbControllerHost, elementManager: UmbBlockElementManager, variantId?: UmbVariantId) {
		// The controller alias, is a very generic name cause we want only one of these for this controller host.
		super(host, elementManager, variantId);

		this.name = elementManager.name;
		this.culture = createObservablePart(elementManager.variantId, (v) => v?.culture);
		this.segment = createObservablePart(elementManager.variantId, (v) => v?.segment);
	}
}
