import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { observeMultiple, type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbVariantPropertyGuardManager, UmbVariantPropertyGuardRule } from '@umbraco-cms/backoffice/property';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbVariantId, type UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

/**
 * Base for controllers that make invariant (shared) properties read-only on variant content
 * when the current user lacks the invariant-for-variant permission.
 */
export abstract class UmbContentInvariantForVariantGuardControllerBase extends UmbControllerBase {
	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			if (!context) return;

			this.observe(
				context.hasAccessToInvariantForVariant,
				(hasAccess) => {
					// Invariant (shared) properties are guarded on every variant, the default language included.
					if (hasAccess === false) {
						this._preventEditInvariantForVariant();
					}
				},
				'_observeHasAccessToInvariantForVariant',
			);
		});
	}

	/**
	 * Installs the write guards that make invariant (shared) properties read-only. Called when the current user lacks the invariant-for-variant permission.
	 */
	protected abstract _preventEditInvariantForVariant(): Promise<void>;

	protected _observeAndApplyRule(args: {
		propertiesObservable: Observable<Array<UmbPropertyTypeModel>>;
		variantOptionsObservable: Observable<Array<UmbEntityVariantOptionModel>>;
		variesByCultureObservable: Observable<boolean | undefined>;
		propertyWriteGuard: UmbVariantPropertyGuardManager;
	}) {
		this.observe(
			observeMultiple([args.propertiesObservable, args.variantOptionsObservable, args.variesByCultureObservable]),
			([properties, variantOptions, variesByCulture]) => {
				// Invariant content types have no concept of data shared across languages - there is only a
				// single dataset, so the invariant-for-variant permission is meaningless here and must never
				// restrict editing it.
				if (variesByCulture === false) return;
				if (properties.length === 0) return;
				if (variantOptions.length === 0) return;

				// Apply the guard rule to every variant — the user lacks the invariant-for-variant
				// permission, so invariant properties must be read-only regardless of which variant
				// tab is being viewed (default language included).
				variantOptions.forEach((variantOption) => {
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

		const unique = `UMB_PREVENT_EDIT_INVARIANT_FOR_VARIANT_DATASET=${args.datasetVariantId.toString()}_PROPERTY_${propertyVariantId.toString()}`;

		const rule: UmbVariantPropertyGuardRule = {
			unique,
			message: 'You do not have permission to edit shared (invariant) properties on this content.',
			variantId: propertyVariantId,
			datasetVariantId: args.datasetVariantId,
			permitted: false,
		};

		return rule;
	}
}
