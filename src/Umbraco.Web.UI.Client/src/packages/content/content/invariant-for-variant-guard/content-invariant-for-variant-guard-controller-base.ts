import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { observeMultiple, type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbVariantPropertyGuardManager, UmbVariantPropertyGuardRule } from '@umbraco-cms/backoffice/property';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbVariantId, type UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

type UmbRuleUnique = string | symbol;

interface UmbRestrictionGuard<IncomingRuleType> {
	addRule(rule: IncomingRuleType): UmbRuleUnique;
	removeRule(unique: UmbRuleUnique): void;
}

/**
 * Base for controllers that make invariant (shared) properties read-only on variant content
 * when the current user lacks the invariant-for-variant permission.
 */
export abstract class UmbContentInvariantForVariantGuardControllerBase extends UmbControllerBase {
	#isRestricted = false;
	#restrictionObserverAliases = new Set<string>();
	#restrictionRuleUniques = new Map<UmbRestrictionGuard<never>, Set<UmbRuleUnique>>();
	#restrictionObserverCount = 0;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			if (!context) return;

			this.observe(
				context.hasAccessToInvariantForVariant,
				(hasAccess) => {
					if (hasAccess === false) {
						// Invariant (shared) properties are guarded on every variant, the default language included.
						this.#isRestricted = true;
						this._preventEditInvariantForVariant();
					} else if (hasAccess === true) {
						this.#removeRestriction();
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

	/**
	 * Observes a source for as long as the current user lacks the invariant-for-variant permission. The observer is
	 * removed when the permission is granted.
	 * @template T
	 * @param {Observable<T>} source - The source to observe.
	 * @param {(value: T) => void} callback - The callback to run for each value.
	 */
	protected _observeWhileRestricted<T>(source: Observable<T>, callback: (value: T) => void) {
		if (!this.#isRestricted) return;
		const alias = `_observeInvariantForVariantRestriction_${this.#restrictionObserverCount++}`;
		this.#restrictionObserverAliases.add(alias);
		this.observe(source, callback, alias);
	}

	/**
	 * Adds a guard rule that is removed when the current user is granted the invariant-for-variant permission.
	 * @template IncomingRuleType
	 * @param {UmbRestrictionGuard<IncomingRuleType>} guard - The guard manager to add the rule to.
	 * @param {IncomingRuleType} rule - The rule to add. It must have a unique, so it can be removed again.
	 */
	protected _addRestrictionRule<IncomingRuleType extends { unique?: UmbRuleUnique }>(
		guard: UmbRestrictionGuard<IncomingRuleType>,
		rule: IncomingRuleType & { unique: UmbRuleUnique },
	) {
		if (!this.#isRestricted) return;
		guard.addRule(rule);
		const uniques = this.#restrictionRuleUniques.get(guard) ?? new Set<UmbRuleUnique>();
		uniques.add(rule.unique);
		this.#restrictionRuleUniques.set(guard, uniques);
	}

	#removeRestriction() {
		this.#isRestricted = false;
		this.#restrictionObserverAliases.forEach((alias) => this.removeUmbControllerByAlias(alias));
		this.#restrictionObserverAliases.clear();
		this.#restrictionRuleUniques.forEach((uniques, guard) => uniques.forEach((unique) => guard.removeRule(unique)));
		this.#restrictionRuleUniques.clear();
	}

	protected _observeAndApplyRule(args: {
		propertiesObservable: Observable<Array<UmbPropertyTypeModel>>;
		variantOptionsObservable: Observable<Array<UmbEntityVariantOptionModel>>;
		variesByCultureObservable: Observable<boolean | undefined>;
		propertyWriteGuard: UmbVariantPropertyGuardManager;
	}) {
		this._observeWhileRestricted(
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
					this._addRestrictionRule(args.propertyWriteGuard, this._createRule({ datasetVariantId }));
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
