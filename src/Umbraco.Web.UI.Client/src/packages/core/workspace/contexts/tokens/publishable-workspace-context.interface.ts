import type { UmbWorkspaceActionExecutionOptions } from './workspace-action-execution-options.interface.js';
import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';

export interface UmbPublishableWorkspaceContext extends UmbWorkspaceContext {
	saveAndPublish(options?: UmbWorkspaceActionExecutionOptions): Promise<void>;
	publish(): Promise<void>;
	unpublish(): Promise<void>;
}
