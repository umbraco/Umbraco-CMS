import { UMB_DOCUMENT_CONFIGURATION_CONTEXT } from '../global-contexts/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbVariantPropertyGuardRule } from '@umbraco-cms/backoffice/property';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export abstract class UmbDocumentAllowEditInvariantFromNonDefaultControllerBase extends UmbControllerBase {
	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_DOCUMENT_CONFIGURATION_CONTEXT, async (context) => {
			const config = await context?.getDocumentConfiguration();
			const allowEditInvariantFromNonDefault = config?.allowEditInvariantFromNonDefault ?? true;

			if (allowEditInvariantFromNonDefault === false) {
				this._preventEditInvariantFromNonDefault();
			}
		});
	}

	protected abstract _preventEditInvariantFromNonDefault(): Promise<void>;

	protected _createRule(args: { datasetVariantId: UmbVariantId }) {
		// always target invariant properties
		const propertyVariantId = UmbVariantId.CreateInvariant();

		const unique = `UMB_PREVENT_EDIT_INVARIANT_FROM_NON_DEFAULT_DATASET=${args.datasetVariantId.toString()}_PROPERTY_${propertyVariantId.toString()}`;

		const rule: UmbVariantPropertyGuardRule = {
			unique,
			message: 'Shared properties can only be edited in the default language',
			variantId: propertyVariantId,
			datasetVariantId: args.datasetVariantId,
			permitted: false,
		};

		return rule;
	}
}
