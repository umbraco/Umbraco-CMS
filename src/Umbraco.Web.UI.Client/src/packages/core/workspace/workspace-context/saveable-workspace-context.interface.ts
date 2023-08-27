import { UmbDatasetContext } from '../dataset-context/dataset-context.interface.js';
import type { UmbWorkspaceContextInterface } from './workspace-context.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbSaveableWorkspaceContextInterface<EntityType = unknown>
	extends UmbWorkspaceContextInterface<EntityType> {
	//getData(): EntityType | undefined;
	save(): Promise<void>;


	// Dataset methods:
	createDatasetContext(host: UmbControllerHost): UmbDatasetContext;
}
