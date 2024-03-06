import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../workspace/media-workspace.context-token.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaWorkspaceHasCollectionCondition
	extends UmbConditionBase<MediaWorkspaceHasCollectionConditionConfig>
	implements UmbExtensionCondition
{
	constructor(
		host: UmbControllerHost,
		args: UmbConditionControllerArguments<MediaWorkspaceHasCollectionConditionConfig>,
	) {
		super(host, args);

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context.contentTypeCollection,
				(collection) => {
					this.permitted = !!collection?.unique;
				},
				'observeCollection',
			);
		});
	}
}

export type MediaWorkspaceHasCollectionConditionConfig = UmbConditionConfigBase<
	typeof UMB_MEDIA_WORKSPACE_HAS_COLLECTION_CONDITION
>;

export const UMB_MEDIA_WORKSPACE_HAS_COLLECTION_CONDITION = 'Umb.Condition.MediaWorkspaceHasCollection';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Media Workspace Has Collection Condition',
	alias: UMB_MEDIA_WORKSPACE_HAS_COLLECTION_CONDITION,
	api: UmbMediaWorkspaceHasCollectionCondition,
};
