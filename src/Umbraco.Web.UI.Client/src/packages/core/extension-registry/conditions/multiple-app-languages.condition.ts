import { UmbConditionBase } from './condition-base.controller.js';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export type UmbMultipleAppLanguageConditionConfig = UmbConditionConfigBase;

export class UmbMultipleAppLanguageCondition
	extends UmbConditionBase<UmbMultipleAppLanguageConditionConfig>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbMultipleAppLanguageConditionConfig>) {
		super(host, args);

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (context) => {
			this.observe(
				context.moreThanOneLanguage,
				(moreThanOneLanguage) => {
					this.permitted = moreThanOneLanguage;
				},
				'observeLanguages',
			);
		});
	}
}

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Multiple App Languages Condition',
	alias: 'Umb.Condition.MultipleAppLanguages',
	api: UmbMultipleAppLanguageCondition,
};
