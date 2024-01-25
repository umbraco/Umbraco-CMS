import { UMB_WORKSPACE_CONTEXT, type UmbWorkspaceContextInterface } from './workspace-context/index.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type {
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

		let permissionCheck: ((context: UmbWorkspaceContextInterface) => boolean) | undefined = undefined;
		if (this.config.match) {
			permissionCheck = (context: UmbWorkspaceContextInterface) => context.workspaceAlias === this.config.match;
		} else if (this.config.oneOf) {
			permissionCheck = (context: UmbWorkspaceContextInterface) =>
				this.config.oneOf!.indexOf(context.workspaceAlias) !== -1;
		}

		if (permissionCheck !== undefined) {
			this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
				this.permitted = permissionCheck!(context);
				this.#onChange();
			});
		} else {
			throw new Error(
				'Condition `Umb.Condition.WorkspaceAlias` could not be initialized properly. Either "match" or "oneOf" must be defined',
			);
		}
	}
}

export type WorkspaceAliasConditionConfig = UmbConditionConfigBase<'Umb.Condition.WorkspaceAlias'> & {
	/**
	 * Define the workspace that this extension should be available in
	 *
	 * @example
	 * "Umb.Workspace.Document"
	 */
	match?: string;
	/**
	 * Define one or more workspaces that this extension should be available in
	 *
	 * @example
	 * ["Umb.Workspace.Document", "Umb.Workspace.Media"]
	 */
	oneOf?: Array<string>;
};

export const manifest: ManifestCondition = {
	type: 'condition',
	name: 'Workspace Alias Condition',
	alias: 'Umb.Condition.WorkspaceAlias',
	api: UmbWorkspaceAliasCondition,
};
