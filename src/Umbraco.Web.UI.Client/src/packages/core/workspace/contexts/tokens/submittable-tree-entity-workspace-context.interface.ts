import type { UmbSubmittableWorkspaceContext } from './submittable-workspace-context.interface.js';
import type { UmbTreeEntityWorkspaceContext } from './tree-entity-workspace-context.interface.js';

export interface UmbSubmittableTreeEntityWorkspaceContext
	extends UmbTreeEntityWorkspaceContext,
		UmbSubmittableWorkspaceContext {}
