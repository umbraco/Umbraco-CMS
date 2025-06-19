import { handlers as dataTypeHandlers } from './handlers/data-type/index.js';
import { handlers as documentTypeHandlers } from './handlers/document-type/index.js';
import { handlers as installHandlers } from './handlers/install.handlers.js';
import * as manifestsHandlers from './handlers/manifests.handlers.js';
import { handlers as publishedStatusHandlers } from './handlers/published-status.handlers.js';
import * as serverHandlers from './handlers/server.handlers.js';
import { handlers as upgradeHandlers } from './handlers/upgrade.handlers.js';
import { handlers as userHandlers } from './handlers/user/index.js';
import { handlers as telemetryHandlers } from './handlers/telemetry.handlers.js';
import { handlers as examineManagementHandlers } from './handlers/examine-management.handlers.js';
import { handlers as modelsBuilderHandlers } from './handlers/modelsbuilder.handlers.js';
import { handlers as profileHandlers } from './handlers/performance-profiling.handlers.js';
import { handlers as healthCheckHandlers } from './handlers/health-check.handlers.js';
import { handlers as languageHandlers } from './handlers/language/index.js';
import { handlers as redirectManagementHandlers } from './handlers/redirect-management.handlers.js';
import { handlers as packageHandlers } from './handlers/package.handlers.js';
import { handlers as configHandlers } from './handlers/config.handlers.js';

export const handlers = [
	serverHandlers.serverRunningHandler,
	...serverHandlers.serverInformationHandlers,
	...manifestsHandlers.manifestEmptyHandlers,
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
