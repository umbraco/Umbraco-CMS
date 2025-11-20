import type { UmbWorkspaceContentTypeUniqueConditionConfig } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from 'src/packages/content/content/workspace/constants.js';

const ObserveSymbol = Symbol();

/**
 * Condition to apply workspace extension based on a content type unique (GUID)
 */
export class UmbWorkspaceContentTypeUniqueCondition
	extends UmbConditionBase<UmbWorkspaceContentTypeUniqueConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbWorkspaceContentTypeUniqueConditionConfig>,
	) {
		super(host, args);

		let permissionCheck: ((contentTypeUniques: string[]) => boolean) | undefined = undefined;
		if (this.config.match) {
			permissionCheck = (contentTypeUniques: string[]) => contentTypeUniques.includes(this.config.match!);
		} else if (this.config.oneOf) {
			permissionCheck = (contentTypeUniques: string[]) =>
				contentTypeUniques.some((item) => this.config.oneOf!.includes(item));
		}

		if (permissionCheck !== undefined) {
			this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (context) => {
				this.observe(
					context?.structure.contentTypeUniques,
					(contentTypeUniques) => {
						const result = contentTypeUniques ? permissionCheck!(contentTypeUniques) : false;
						this.permitted = result;
					},
					ObserveSymbol,
				);
			});
		} else {
			throw new Error(
				'Condition `Umb.Condition.WorkspaceContentTypeUnique` could not be initialized properly. Either "match" or "oneOf" must be defined',
			);
		}
	}
}

export const manifest: UmbExtensionManifest = {
	type: 'condition',
	name: 'Workspace Content Type Unique Condition',
	alias: 'Umb.Condition.WorkspaceContentTypeUnique',
	api: UmbWorkspaceContentTypeUniqueCondition,
};
