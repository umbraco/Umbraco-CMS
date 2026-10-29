import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// The Example Workspace Context Controller:
export class ExampleExtremeConditionsWorkspaceContext extends UmbControllerBase {
	constructor(host: UmbControllerHost) {
		super(host);
		console.info('[Extreme Conditions] Workspace Context was created, its conditions are permitted.');
	}
}

// Declare a api export, so Extension Registry can initialize this class:
export { ExampleExtremeConditionsWorkspaceContext as api };
