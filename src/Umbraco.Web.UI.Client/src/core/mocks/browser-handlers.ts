import { handlers as dataTypeHandlers } from './handlers/data-type';
import { handlers as relationTypeHandlers } from './handlers/relation-type.handlers';
import { handlers as documentTypeHandlers } from './handlers/document-type.handlers';
import { handlers as installHandlers } from './handlers/install.handlers';
import * as manifestsHandlers from './handlers/manifests.handlers';
import { handlers as publishedStatusHandlers } from './handlers/published-status.handlers';
import * as serverHandlers from './handlers/server.handlers';
import { handlers as upgradeHandlers } from './handlers/upgrade.handlers';
import { handlers as userHandlers } from './handlers/user.handlers';
import { handlers as telemetryHandlers } from './handlers/telemetry.handlers';
import { handlers as userGroupsHandlers } from './handlers/user-group.handlers';
import { handlers as examineManagementHandlers } from './handlers/examine-management.handlers';
import { handlers as modelsBuilderHandlers } from './handlers/modelsbuilder.handlers';
import { handlers as healthCheckHandlers } from './handlers/health-check.handlers';
import { handlers as profilingHandlers } from './handlers/performance-profiling.handlers';
import { handlers as documentHandlers } from './handlers/document.handlers';
import { handlers as mediaHandlers } from './handlers/media.handlers';
import { handlers as dictionaryHandlers } from './handlers/dictionary.handlers';
import { handlers as mediaTypeHandlers } from './handlers/media-type.handlers';
import { handlers as memberGroupHandlers } from './handlers/member-group.handlers';
import { handlers as memberHandlers } from './handlers/member.handlers';
import { handlers as memberTypeHandlers } from './handlers/member-type.handlers';
import { handlers as templateHandlers } from './handlers/template.handlers';
import { handlers as languageHandlers } from './handlers/language.handlers';
import { handlers as cultureHandlers } from './handlers/culture.handlers';
import { handlers as redirectManagementHandlers } from './handlers/redirect-management.handlers';
import { handlers as logViewerHandlers } from './handlers/log-viewer.handlers';
import { handlers as packageHandlers } from './handlers/package.handlers';
import { handlers as rteEmbedHandlers } from './handlers/rte-embed.handlers';
import { handlers as stylesheetHandlers } from './handlers/stylesheet.handlers';
import { handlers as partialViewsHandlers } from './handlers/partial-views.handlers';
import { handlers as tagHandlers } from './handlers/tag-handlers';
import { handlers as configHandlers } from './handlers/config.handlers';

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
