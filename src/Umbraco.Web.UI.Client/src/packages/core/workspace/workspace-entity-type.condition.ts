import { UMB_WORKSPACE_CONTEXT } from './workspace-context/index.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/controller-api';
import {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbWorkspaceEntityTypeCondition extends UmbBaseController implements UmbExtensionCondition {
	config: WorkspaceEntityTypeConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<WorkspaceEntityTypeConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;
		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			this.permitted = context.getEntityType().toLowerCase() === this.config.match.toLowerCase();
			this.#onChange();
		});
	}
}

export type WorkspaceEntityTypeConditionConfig = UmbConditionConfigBase<'Umb.Condition.WorkspaceEntityType'> & {
	/**
	 * Define the workspace that this extension should be available in
	 *
	 * @example
	 * "Document"
	 */
	match: string;
};

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Workspace Entity Type Condition',
	alias: 'Umb.Condition.WorkspaceEntityType',
	api: UmbWorkspaceEntityTypeCondition,
};
