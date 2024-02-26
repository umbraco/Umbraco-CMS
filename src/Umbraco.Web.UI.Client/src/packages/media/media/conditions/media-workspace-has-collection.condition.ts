import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../workspace/media-workspace.context-token.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbMediaWorkspaceHasCollectionCondition extends UmbBaseController implements UmbExtensionCondition {
	config: MediaWorkspaceHasCollectionConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<MediaWorkspaceHasCollectionConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context.contentTypeCollection,
				(collection) => {
					this.permitted = !!collection?.id;
					this.#onChange();
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
