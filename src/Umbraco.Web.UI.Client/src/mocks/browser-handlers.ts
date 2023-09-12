import { handlers as dataTypeHandlers } from './handlers/data-type/index.js';
import { handlers as relationTypeHandlers } from './handlers/relation-type.handlers.js';
import { handlers as documentTypeHandlers } from './handlers/document-type.handlers.js';
import { handlers as installHandlers } from './handlers/install.handlers.js';
import * as manifestsHandlers from './handlers/manifests.handlers.js';
import { handlers as publishedStatusHandlers } from './handlers/published-status.handlers.js';
import * as serverHandlers from './handlers/server.handlers.js';
import { handlers as upgradeHandlers } from './handlers/upgrade.handlers.js';
import { handlers as userHandlers } from './handlers/user.handlers.js';
import { handlers as telemetryHandlers } from './handlers/telemetry.handlers.js';
import { handlers as userGroupsHandlers } from './handlers/user-group.handlers.js';
import { handlers as examineManagementHandlers } from './handlers/examine-management.handlers.js';
import { handlers as modelsBuilderHandlers } from './handlers/modelsbuilder.handlers.js';
import { handlers as healthCheckHandlers } from './handlers/health-check.handlers.js';
import { handlers as profilingHandlers } from './handlers/performance-profiling.handlers.js';
import { handlers as documentHandlers } from './handlers/document.handlers.js';
import { handlers as mediaHandlers } from './handlers/media.handlers.js';
import { handlers as dictionaryHandlers } from './handlers/dictionary.handlers.js';
import { handlers as mediaTypeHandlers } from './handlers/media-type.handlers.js';
import { handlers as memberGroupHandlers } from './handlers/member-group.handlers.js';
import { handlers as memberHandlers } from './handlers/member.handlers.js';
import { handlers as memberTypeHandlers } from './handlers/member-type.handlers.js';
import { handlers as templateHandlers } from './handlers/template.handlers.js';
import { handlers as languageHandlers } from './handlers/language.handlers.js';
import { handlers as cultureHandlers } from './handlers/culture.handlers.js';
import { handlers as redirectManagementHandlers } from './handlers/redirect-management.handlers.js';
import { handlers as logViewerHandlers } from './handlers/log-viewer.handlers.js';
import { handlers as packageHandlers } from './handlers/package.handlers.js';
import { handlers as rteEmbedHandlers } from './handlers/rte-embed.handlers.js';
import { handlers as stylesheetHandlers } from './handlers/stylesheet.handlers.js';
import { handlers as partialViewsHandlers } from './handlers/partial-views.handlers.js';
import { handlers as tagHandlers } from './handlers/tag-handlers.js';
import { handlers as configHandlers } from './handlers/config.handlers.js';
import { handlers as scriptHandlers } from './handlers/scripts.handlers.js';

const handlers = [
	serverHandlers.serverVersionHandler,
	...installHandlers,
	...upgradeHandlers,
	...userHandlers,
	...documentHandlers,
	...mediaHandlers,
	...dataTypeHandlers,
	...relationTypeHandlers,
	...documentTypeHandlers,
	...telemetryHandlers,
	...publishedStatusHandlers,
	...userGroupsHandlers,
	...mediaTypeHandlers,
	...memberGroupHandlers,
	...memberHandlers,
	...memberTypeHandlers,
	...examineManagementHandlers,
	...modelsBuilderHandlers,
	...healthCheckHandlers,
	...profilingHandlers,
	...dictionaryHandlers,
	...templateHandlers,
	...languageHandlers,
	...cultureHandlers,
	...redirectManagementHandlers,
	...logViewerHandlers,
	...packageHandlers,
	...rteEmbedHandlers,
	...stylesheetHandlers,
	...partialViewsHandlers,
	...tagHandlers,
	...configHandlers,
	...scriptHandlers,
];

switch (import.meta.env.VITE_UMBRACO_INSTALL_STATUS) {
	case 'must-install':
		handlers.push(serverHandlers.serverMustInstallHandler);
		break;
	case 'must-upgrade':
		handlers.push(serverHandlers.serverMustUpgradeHandler);
		break;
	default:
		handlers.push(serverHandlers.serverRunningHandler);
}

switch (import.meta.env.VITE_UMBRACO_EXTENSION_MOCKS) {
	case 'on':
		handlers.push(manifestsHandlers.manifestDevelopmentHandler);
		break;

	default:
		handlers.push(manifestsHandlers.manifestEmptyHandler);
}

export { handlers };
