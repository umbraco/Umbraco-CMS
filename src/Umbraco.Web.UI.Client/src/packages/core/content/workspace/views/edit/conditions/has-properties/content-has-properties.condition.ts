import { UMB_CONTENT_WORKSPACE_CONTEXT } from '../../../../content-workspace.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class UmbContentHasPropertiesWorkspaceCondition
	extends UmbConditionBase<UmbConditionConfigBase>
	implements UmbExtensionCondition
{
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<UmbConditionConfigBase>) {
		super(host, args);

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context.structure.contentTypes,
				(contentTypes) => {
					const hasProperties = contentTypes.some((contentType) => contentType.properties.length > 0);
					this.permitted = hasProperties;
				},
				'contentTypesObserver',
			);
		});
	}
}

export { UmbContentHasPropertiesWorkspaceCondition as api };
