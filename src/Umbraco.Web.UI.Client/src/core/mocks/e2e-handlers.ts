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
import { handlers as healthCheckHandlers } from './domains/health-check.handlers';
import { handlers as languageHandlers } from './domains/language.handlers';
import { handlers as redirectManagementHandlers } from './domains/redirect-management.handlers';
import { handlers as packageHandlers } from './domains/package.handlers';

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
];
