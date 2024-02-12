import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/document';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbDocumentWorkspaceHasCollectionCondition extends UmbBaseController implements UmbExtensionCondition {
	config: DocumentWorkspaceHasCollectionConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<DocumentWorkspaceHasCollectionConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			this.observe(context.structure.ownerContentType(), (contentType) => {
				// TODO: [LK] Replace this check once the `.collection` is available from the Management API.
				if (contentType?.unique === 'simple-document-type-id') {
					this.permitted = true;
					this.#onChange();
				}
			});
		});
	}
}

export type DocumentWorkspaceHasCollectionConditionConfig =
	UmbConditionConfigBase<'Umb.Condition.DocumentWorkspaceHasCollection'>;
