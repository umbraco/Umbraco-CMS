import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../workspace/document-workspace.context-token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbDocumentWorkspaceHasCollectionCondition extends UmbControllerBase implements UmbExtensionCondition {
	config: DocumentWorkspaceHasCollectionConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<DocumentWorkspaceHasCollectionConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.observe(
				context.contentTypeHasCollection,
				(hasCollection) => {
					this.permitted = hasCollection;
					this.#onChange();
				},
				'observeCollection',
			);
		});
	}
}

export type DocumentWorkspaceHasCollectionConditionConfig = UmbConditionConfigBase<
	typeof UMB_DOCUMENT_WORKSPACE_HAS_COLLECTION_CONDITION
>;

export const UMB_DOCUMENT_WORKSPACE_HAS_COLLECTION_CONDITION = 'Umb.Condition.DocumentWorkspaceHasCollection';

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Document Workspace Has Collection Condition',
	alias: UMB_DOCUMENT_WORKSPACE_HAS_COLLECTION_CONDITION,
	api: UmbDocumentWorkspaceHasCollectionCondition,
};
