import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbVariantDatasetWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UMB_VARIANT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import type { UmbVariantOptionModel, UmbVariantModel } from '@umbraco-cms/backoffice/variant';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbLanguageAccessWorkspaceContext extends UmbContextBase<unknown> {
	#workspaceContext?: UmbVariantDatasetWorkspaceContext;
	#currentUserAllowedLanguages?: Array<string>;
	#currentUserHasAccessToAllLanguages?: boolean;
	#variantOptions?: UmbVariantOptionModel<UmbVariantModel>[];

	constructor(host: UmbControllerHost) {
		super(host, 'UmbLanguageAccessWorkspaceContext');

		this.consumeContext(UMB_VARIANT_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(instance.variantOptions, (variantOptions) => {
				this.#variantOptions = variantOptions;
				this.#checkForLanguageAccess();
			});
		});

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(context.languages, (languages) => {
				this.#currentUserAllowedLanguages = languages;
				this.#checkForLanguageAccess();
			});

			this.observe(context.hasAccessToAllLanguages, (hasAccessToAllLanguages) => {
				this.#currentUserHasAccessToAllLanguages = hasAccessToAllLanguages;
				this.#checkForLanguageAccess();
			});
		});
	}

	async #checkForLanguageAccess() {
		if (!this.#workspaceContext) return;

		// find all disallowed languages
		const disallowedLanguages = this.#variantOptions?.filter((variant) => {
			if (this.#currentUserHasAccessToAllLanguages) {
				return false;
			}

			if (!variant.culture) {
				return false;
			}

			return !this.#currentUserAllowedLanguages?.includes(variant.culture);
		});

		// create a list of variantIds for the disallowed languages
		const variantIds = disallowedLanguages?.map((variant) => new UmbVariantId(variant.culture, variant.segment)) || [];

		// create a list of states for the disallowed languages
		const identifier = 'UMB_CULTURE_';
		const readOnlyStates = variantIds.map((variantId) => {
			return {
				unique: identifier + variantId.culture,
				variantId,
				message: 'You do not have permission to edit to this culture',
			};
		});

		// remove all previous states before adding new ones
		const uniques = this.#variantOptions?.map((variant) => identifier + variant.culture) || [];
		this.#workspaceContext.readOnlyState?.removeStates(uniques);

		// add new states
		this.#workspaceContext.readOnlyState?.addStates(readOnlyStates);
	}
}

export { UmbLanguageAccessWorkspaceContext as api };
