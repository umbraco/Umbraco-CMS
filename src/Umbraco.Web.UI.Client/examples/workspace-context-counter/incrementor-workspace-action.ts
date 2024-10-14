import { UmbWorkspaceActionBase, type UmbWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import { EXAMPLE_COUNTER_CONTEXT } from './counter-workspace-context';

// The Example Incrementor Workspace Action Controller:
export class ExampleIncrementorWorkspaceAction extends UmbWorkspaceActionBase implements UmbWorkspaceAction {
	// This method is executed
	override async execute() {
		const context = await this.getContext(EXAMPLE_COUNTER_CONTEXT);
		context.increment();
	}
}

// Declare a api export, so Extension Registry can initialize this class:
export const api = ExampleIncrementorWorkspaceAction;
