import { UmbBaseController } from '@umbraco-cms/backoffice/controller-api';
import type { UmbWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import { EXAMPLE_COUNTER_CONTEXT } from './counter-workspace-context';

// The Example Incrementor Workspace Action Controller:
export class ExampleIncrementorWorkspaceAction extends UmbBaseController implements UmbWorkspaceAction {

	// This method is executed
	async execute() {
		await this.consumeContext(EXAMPLE_COUNTER_CONTEXT, (context) => {
			context.increment();
		}).asPromise();
	}
}

// Declare a api export, so Extension Registry can initialize this class:
export const api = ExampleIncrementorWorkspaceAction;
