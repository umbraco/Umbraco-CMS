import { UMB_WORKSPACE_CONTEXT } from './workspace-context/index.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/controller-api';
import {
	ManifestCondition,
	UmbConditionConfigBase,
	UmbConditionControllerArguments,
	UmbExtensionCondition,
} from '@umbraco-cms/backoffice/extension-api';

export class UmbWorkspaceAliasCondition extends UmbBaseController implements UmbExtensionCondition {
	config: WorkspaceAliasConditionConfig;
	permitted = false;
	#onChange: () => void;

	constructor(args: UmbConditionControllerArguments<WorkspaceAliasConditionConfig>) {
		super(args.host);
		this.config = args.config;
		this.#onChange = args.onChange;
		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			this.permitted = context.workspaceAlias === this.config.match;
			this.#onChange();
		});
	}
}

export type WorkspaceAliasConditionConfig = UmbConditionConfigBase<'Umb.Condition.WorkspaceAlias'> & {
	/**
	 * Define the workspace that this extension should be available in
	 *
	 * @examples
	 * "Umb.Workspace.Document"
	 */
	match: string;
};

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Workspace Alias Condition',
	alias: 'Umb.Condition.WorkspaceAlias',
	class: UmbWorkspaceAliasCondition,
};
