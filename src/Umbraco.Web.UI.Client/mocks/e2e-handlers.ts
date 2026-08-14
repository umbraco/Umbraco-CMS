import { handlers as dataTypeHandlers } from './msw-handlers/data-type/index.js';
import { handlers as documentTypeHandlers } from './msw-handlers/document-type/index.js';
import { handlers as installHandlers } from './msw-handlers/install.handlers.js';
import * as manifestsHandlers from './msw-handlers/manifests.handlers.js';
import { handlers as publishedStatusHandlers } from './msw-handlers/published-status.handlers.js';
import * as serverHandlers from './msw-handlers/server.handlers.js';
import { handlers as upgradeHandlers } from './msw-handlers/upgrade.handlers.js';
import { handlers as userHandlers } from './msw-handlers/user/index.js';
import { handlers as telemetryHandlers } from './msw-handlers/telemetry.handlers.js';
import { handlers as examineManagementHandlers } from './msw-handlers/examine-management.handlers.js';
import { handlers as modelsBuilderHandlers } from './msw-handlers/modelsbuilder.handlers.js';
import { handlers as profileHandlers } from './msw-handlers/performance-profiling.handlers.js';
import { handlers as healthCheckHandlers } from './msw-handlers/health-check.handlers.js';
import { handlers as languageHandlers } from './msw-handlers/language/index.js';
import { handlers as redirectManagementHandlers } from './msw-handlers/redirect-management.handlers.js';
import { handlers as packageHandlers } from './msw-handlers/package.handlers.js';
import { handlers as configHandlers } from './msw-handlers/config.handlers.js';

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
