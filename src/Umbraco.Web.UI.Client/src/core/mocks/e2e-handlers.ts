import { handlers as dataTypeHandlers } from './domains/data-type.handlers';
import { handlers as documentTypeHandlers } from './domains/document-type.handlers';
import { handlers as installHandlers } from './domains/install.handlers';
import * as manifestsHandlers from './domains/manifests.handlers';
import { handlers as publishedStatusHandlers } from './domains/published-status.handlers';
import * as serverHandlers from './domains/server.handlers';
import { handlers as upgradeHandlers } from './domains/upgrade.handlers';
import { handlers as userHandlers } from './domains/user.handlers';
import { handlers as telemetryHandlers } from './domains/telemetry.handlers';
import { handlers as examineManagementHandlers } from './domains/examine-management.handlers';
import { handlers as modelsBuilderHandlers } from './domains/modelsbuilder.handlers';
import { handlers as profileHandlers } from './domains/performance-profiling.handlers';
import { handlers as redirectManagementHandlers } from './domains/redirect-management.handlers';

export const handlers = [
	serverHandlers.serverRunningHandler,
	serverHandlers.serverVersionHandler,
	manifestsHandlers.manifestDevelopmentHandler,
	...installHandlers,
	...upgradeHandlers,
	...userHandlers,
	...dataTypeHandlers,
	...documentTypeHandlers,
	...manifestsHandlers.default,
	...telemetryHandlers,
	...publishedStatusHandlers,
	...examineManagementHandlers,
	...modelsBuilderHandlers,
	...profileHandlers,
	...redirectManagementHandlers,
];
