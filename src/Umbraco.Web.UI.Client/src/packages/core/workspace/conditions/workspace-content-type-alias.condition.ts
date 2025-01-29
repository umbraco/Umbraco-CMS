import { UmbConditionBase } from '../../extension-registry/conditions/condition-base.controller.js';
import type { UmbWorkspaceContentTypeAliasConditionConfig } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';

const ObserveSymbol = Symbol();

/**
 * Condition to apply workspace extension based on a content type alias
 */
export class UmbWorkspaceContentTypeAliasCondition
	extends UmbConditionBase<UmbWorkspaceContentTypeAliasConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<UmbWorkspaceContentTypeAliasConditionConfig>,
	) {
		super(host, args);

		let permissionCheck: ((contentTypeAliases: string[]) => boolean) | undefined = undefined;
		if (this.config.match) {
			permissionCheck = (contentTypeAliases: string[]) => contentTypeAliases.includes(this.config.match!);
		} else if (this.config.oneOf) {
			permissionCheck = (contentTypeAliases: string[]) =>
				contentTypeAliases.some((item) => this.config.oneOf!.includes(item));
		}

		if (permissionCheck !== undefined) {
			this.consumeContext(UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT, (context) => {
				this.observe(
					context.structure.contentTypeAliases,
					(contentTypeAliases) => {
						this.permitted = contentTypeAliases ? permissionCheck!(contentTypeAliases) : false;
					},
					ObserveSymbol,
				);
			});
		} else {
			throw new Error(
				'Condition `Umb.Condition.WorkspaceContentTypeAlias` could not be initialized properly. Either "match" or "oneOf" must be defined',
			);
		}
	}
}

export const manifest: UmbExtensionManifest = {
	type: 'condition',
	name: 'Workspace Content Type Alias Condition',
	alias: 'Umb.Condition.WorkspaceContentTypeAlias',
	api: UmbWorkspaceContentTypeAliasCondition,
};
