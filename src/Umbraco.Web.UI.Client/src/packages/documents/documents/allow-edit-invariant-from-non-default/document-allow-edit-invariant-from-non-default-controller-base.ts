import { UMB_DOCUMENT_CONFIGURATION_CONTEXT } from '../global-contexts/document-configuration.context.js';
import type { UmbDocumentVariantModel } from '../types.js';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { observeMultiple, type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbVariantPropertyGuardManager, UmbVariantPropertyGuardRule } from '@umbraco-cms/backoffice/property';
import { UmbVariantId, type UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

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

	protected _observeAndApplyRule(args: {
		propertiesObservable: Observable<Array<UmbPropertyTypeModel>>;
		variantOptionsObservable: Observable<UmbEntityVariantOptionModel<UmbDocumentVariantModel>[]>;
		propertyWriteGuard: UmbVariantPropertyGuardManager;
	}) {
		this.observe(
			observeMultiple([args.propertiesObservable, args.variantOptionsObservable]),
			([properties, variantOptions]) => {
				if (properties.length === 0) return;
				if (variantOptions.length === 0) return;

				variantOptions.forEach((variantOption) => {
					// Do not add a rule for the default language. It is always permitted to edit.
					if (variantOption.language.isDefault) return;

					const datasetVariantId = UmbVariantId.CreateFromPartial(variantOption);
					const rule: UmbVariantPropertyGuardRule = this._createRule({ datasetVariantId });

					args.propertyWriteGuard.addRule(rule);
				});
			},
		);
	}

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
