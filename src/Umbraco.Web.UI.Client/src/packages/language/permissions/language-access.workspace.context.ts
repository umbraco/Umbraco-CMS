import { UMB_LANGUAGE_ACCESS_WORKSPACE_CONTEXT } from './language-access.workspace.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import type { UmbEntityVariantOptionModel, UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';

const READ_ONLY_RULE_PREFIX = 'UMB_LANGUAGE_PERMISSION_';
const PROPERTY_WRITE_RULE_PREFIX = 'UMB_LANGUAGE_PERMISSION_PROPERTY_';
const VARIANT_WRITE_RULE_PREFIX = 'UMB_LANGUAGE_PERMISSION_VARIANT_';

export class UmbLanguageAccessWorkspaceContext extends UmbContextBase {
	#workspaceContext?: typeof UMB_CONTENT_WORKSPACE_CONTEXT.TYPE;
	#currentUserAllowedLanguages?: Array<string>;
	#currentUserHasAccessToAllLanguages?: boolean;
	#currentUserHasAccessToInvariantForVariant?: boolean;
	#variantOptions?: UmbEntityVariantOptionModel<UmbEntityVariantModel>[];
	#contentTypeProperties?: Array<UmbPropertyTypeModel>;

	constructor(host: UmbControllerHost) {
		super(host, UMB_LANGUAGE_ACCESS_WORKSPACE_CONTEXT);

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(instance?.variantOptions, (variantOptions) => {
				this.#variantOptions = variantOptions;
				this.#checkForLanguageAccess();
			});
			this.observe(instance?.structure.contentTypeProperties, (properties) => {
				this.#contentTypeProperties = properties;
				this.#checkForLanguageAccess();
			});
		});

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(context?.languages, (languages) => {
				this.#currentUserAllowedLanguages = languages;
				this.#checkForLanguageAccess();
			});

			this.observe(context?.hasAccessToAllLanguages, (hasAccessToAllLanguages) => {
				this.#currentUserHasAccessToAllLanguages = hasAccessToAllLanguages;
				this.#checkForLanguageAccess();
			});

			this.observe(context?.hasAccessToInvariantForVariant, (hasAccessToInvariantForVariant) => {
				this.#currentUserHasAccessToInvariantForVariant = hasAccessToInvariantForVariant;
				this.#checkForLanguageAccess();
			});
		});
	}

	async #checkForLanguageAccess() {
		if (!this.#workspaceContext) return;

		// find all disallowed language variants
		const disallowedVariants =
			this.#variantOptions?.filter((variant) => {
				if (this.#currentUserHasAccessToAllLanguages) {
					return false;
				}

				if (!variant.culture) {
					return false;
				}

				return !this.#currentUserAllowedLanguages?.includes(variant.culture);
			}) ?? [];

		const datasetVariantIds = disallowedVariants.map((variant) => new UmbVariantId(variant.culture, variant.segment));

		// always clear any previously installed rules from both guards before re-applying, so that
		// switching between "has invariant access" and "no invariant access" leaves no orphaned rules
		this.#clearPreviousRules();

		// Regardless of invariant access, a culture the user has no access to must never be saved or published.
		const variantWriteRules = datasetVariantIds.map((variantId) => ({
			unique: VARIANT_WRITE_RULE_PREFIX + variantId.toString(),
			variantId,
			permitted: false,
			message: 'You do not have permission to edit this culture',
		}));
		this.#workspaceContext.variantWriteGuard?.addRules(variantWriteRules);

		if (this.#currentUserHasAccessToInvariantForVariant) {
			// The user is allowed to edit invariant (shared) property data on variant content. Don't
			// lock the whole dataset read-only — that would cascade down to invariant properties via
			// the dataset → property read-only propagation. Instead install property-level write-deny
			// rules on culture- or segment-varying properties, leaving invariant properties editable.
			const variantProperties =
				this.#contentTypeProperties?.filter((prop) => prop.variesByCulture || prop.variesBySegment) ?? [];

			const propertyRules = datasetVariantIds.flatMap((datasetVariantId) =>
				variantProperties.map((prop) => ({
					unique: this.#propertyRuleUnique(datasetVariantId.culture, prop.unique),
					variantId: new UmbVariantId(
						prop.variesByCulture ? datasetVariantId.culture : null,
						prop.variesBySegment ? datasetVariantId.segment : null,
					),
					propertyType: { unique: prop.unique },
					datasetVariantId,
					permitted: false,
					message: 'You do not have permission to edit this culture',
				})),
			);

			this.#workspaceContext.propertyWriteGuard?.addRules(propertyRules);
		} else {
			// The user has no permission to edit invariant-for-variant data, so fall back to locking
			// the entire disallowed culture dataset read-only (original behavior).
			const readOnlyRules = datasetVariantIds.map((variantId) => {
				return {
					unique: READ_ONLY_RULE_PREFIX + variantId.culture,
					variantId,
					message: 'You do not have permission to edit to this culture',
				};
			});

			this.#workspaceContext.readOnlyGuard?.addRules(readOnlyRules);
		}
	}

	#clearPreviousRules() {
		const cultures = this.#variantOptions?.map((variant) => variant.culture) ?? [];

		const readOnlyIdentifiers = cultures.map((culture) => READ_ONLY_RULE_PREFIX + culture);
		this.#workspaceContext?.readOnlyGuard?.removeRules(readOnlyIdentifiers);

		const propertyIdentifiers = cultures.flatMap(
			(culture) => this.#contentTypeProperties?.map((prop) => this.#propertyRuleUnique(culture, prop.unique)) ?? [],
		);
		this.#workspaceContext?.propertyWriteGuard?.removeRules(propertyIdentifiers);

		const variantWriteIdentifiers =
			this.#variantOptions?.map(
				(variant) => VARIANT_WRITE_RULE_PREFIX + new UmbVariantId(variant.culture, variant.segment).toString(),
			) ?? [];
		this.#workspaceContext?.variantWriteGuard?.removeRules(variantWriteIdentifiers);
	}

	#propertyRuleUnique(culture: string | null | undefined, propertyUnique: string) {
		return `${PROPERTY_WRITE_RULE_PREFIX}${culture}_${propertyUnique}`;
	}
}

export { UmbLanguageAccessWorkspaceContext as api };
