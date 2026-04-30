import { UMB_LANGUAGE_ACCESS_WORKSPACE_CONTEXT } from './language-access.workspace.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import type { UmbEntityVariantOptionModel, UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';

export class UmbLanguageAccessWorkspaceContext extends UmbContextBase {
	#workspaceContext?: typeof UMB_CONTENT_WORKSPACE_CONTEXT.TYPE;
	#currentUserAllowedLanguages?: Array<string>;
	#currentUserHasAccessToAllLanguages?: boolean;
	#variantOptions?: UmbEntityVariantOptionModel<UmbEntityVariantModel>[];

	constructor(host: UmbControllerHost) {
		super(host, UMB_LANGUAGE_ACCESS_WORKSPACE_CONTEXT);

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(instance?.variantOptions, (variantOptions) => {
				this.#variantOptions = variantOptions;
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

		const identifier = 'UMB_LANGUAGE_PERMISSION_';
		const nameRules = variantIds.map((variantId) => ({
			unique: identifier + variantId.culture,
			variantId,
			message: 'You do not have permission to edit this culture',
			permitted: false,
		}));
		const propertyRules = variantIds.map((variantId) => ({
			unique: identifier + variantId.culture,
			datasetVariantId: variantId,
			message: 'You do not have permission to edit this culture',
			permitted: false,
		}));

		// remove all previous rules before adding new ones
		const uniques = this.#variantOptions?.map((variant) => identifier + variant.culture) || [];
		this.#workspaceContext.propertyWriteGuard.removeRules(uniques);
		this.#workspaceContext.nameWriteGuard.removeRules(uniques);

		// add new rules
		this.#workspaceContext.propertyWriteGuard.addRules(propertyRules);
		this.#workspaceContext.nameWriteGuard.addRules(nameRules);
	}
}

export { UmbLanguageAccessWorkspaceContext as api };
