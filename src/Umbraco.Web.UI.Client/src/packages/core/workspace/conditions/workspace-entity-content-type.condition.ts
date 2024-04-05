import { UmbConditionBase } from '../../extension-registry/conditions/condition-base.controller.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import { UMB_ENTITY_WITH_CONTENT_TYPE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

/**
 * Condition to apply extension based on a entities content type unique
 */
export class UmbWorkspaceEntityContentTypeCondition extends UmbConditionBase<WorkspaceEntityContentTypeConditionConfig> implements UmbExtensionCondition {
	constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<WorkspaceEntityContentTypeConditionConfig>) {
		super(host, args);

		let permissionCheck: ((contentTypeUnique: string) => boolean) | undefined = undefined;
		if (this.config.match) {
			permissionCheck = (contentTypeUnique: string) => contentTypeUnique === this.config.match;
		} else if (this.config.oneOf) {
			permissionCheck = (contentTypeUnique: string) => this.config.oneOf!.indexOf(contentTypeUnique) !== -1;
		}

		if (permissionCheck !== undefined) {
			
			this.consumeContext(UMB_ENTITY_WITH_CONTENT_TYPE_WORKSPACE_CONTEXT, (context) => {

				this.observe(context.contentTypeUnique,(contentTypeUnique)=> {
					this.permitted = contentTypeUnique ? permissionCheck(contentTypeUnique) : false;
				},
				'workspaceContentTypeUniqueConditionObserver');
			});

		} else {
			throw new Error(
				'Condition `Umb.Condition.EntityContentType` could not be initialized properly. Either "match" or "oneOf" must be defined',
			);
		}
	}
}

export type WorkspaceEntityContentTypeConditionConfig = UmbConditionConfigBase<'Umb.Condition.EntityContentType'> & {
	/**
	 * Define the unique content type key where this extension should be available in
	 *
	 * @example
	 * "a1eb4175-3ec1-40ea-8dda-083df6648973"
	 */
	match?: string;
	/**
	 * Define one or more content type keys that this extension should be available in
	 *
	 * @example
	 * ["a1eb4175-3ec1-40ea-8dda-083df6648973", "2ac00e5d-8763-42d9-a38c-adaaee02cfae"]
	 */
	oneOf?: Array<string>;
};

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Workspace Entity Content Type Condition',
	alias: 'Umb.Condition.EntityContentType',
	api: UmbWorkspaceEntityContentTypeCondition,
};
