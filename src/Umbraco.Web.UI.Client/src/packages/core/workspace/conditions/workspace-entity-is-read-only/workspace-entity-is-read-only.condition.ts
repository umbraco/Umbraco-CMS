import type { UmbWorkspaceEntityIsReadOnlyConditionConfig } from '../types.js';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '../../entity-detail/entity-detail-workspace.context-token.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const ObserveSymbol = Symbol();

export class UmbWorkspaceEntityIsReadOnlyCondition
	extends UmbConditionBase<UmbWorkspaceEntityIsReadOnlyConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbWorkspaceEntityIsReadOnlyConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context?.readOnly,
				(isReadOnly) => {
					if (isReadOnly !== undefined) {
						this.permitted = isReadOnly === (this.config.match ?? true);
					}
				},
				ObserveSymbol,
			);
		});
	}
}
