import type { UmbWorkspaceActionExecutionOptions } from './workspace-action-execution-options.interface.js';
import type { UmbSubmittableWorkspaceContext } from './submittable-workspace-context.interface.js';

export interface UmbSaveableWorkspaceContext extends UmbSubmittableWorkspaceContext {
	requestSave(options?: UmbWorkspaceActionExecutionOptions): Promise<void>;
}
