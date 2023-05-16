import { handlers as dataTypeHandlers } from './handlers/data-type';
import { handlers as documentTypeHandlers } from './handlers/document-type.handlers';
import { handlers as installHandlers } from './handlers/install.handlers';
import * as manifestsHandlers from './handlers/manifests.handlers';
import { handlers as publishedStatusHandlers } from './handlers/published-status.handlers';
import * as serverHandlers from './handlers/server.handlers';
import { handlers as upgradeHandlers } from './handlers/upgrade.handlers';
import { handlers as userHandlers } from './handlers/user.handlers';
import { handlers as telemetryHandlers } from './handlers/telemetry.handlers';
import { handlers as examineManagementHandlers } from './handlers/examine-management.handlers';
import { handlers as modelsBuilderHandlers } from './handlers/modelsbuilder.handlers';
import { handlers as profileHandlers } from './handlers/performance-profiling.handlers';
import { handlers as healthCheckHandlers } from './handlers/health-check.handlers';
import { handlers as languageHandlers } from './handlers/language.handlers';
import { handlers as redirectManagementHandlers } from './handlers/redirect-management.handlers';
import { handlers as packageHandlers } from './handlers/package.handlers';
import { handlers as configHandlers } from './handlers/config.handlers';

export const handlers = [
	serverHandlers.serverRunningHandler,
	serverHandlers.serverVersionHandler,
	manifestsHandlers.manifestEmptyHandler,
	...installHandlers,
	...upgradeHandlers,
	...userHandlers,
	...dataTypeHandlers,
	...documentTypeHandlers,
	...telemetryHandlers,
	...publishedStatusHandlers,
	...examineManagementHandlers,
	...modelsBuilderHandlers,
	...profileHandlers,
	...healthCheckHandlers,
	...languageHandlers,
	...redirectManagementHandlers,
	...packageHandlers,
	...configHandlers,
];
