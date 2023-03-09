import { handlers as dataTypeHandlers } from './domains/data-type.handlers';
import { handlers as documentTypeHandlers } from './domains/document-type.handlers';
import { handlers as installHandlers } from './domains/install.handlers';
import * as manifestsHandlers from './domains/manifests.handlers';
import { handlers as publishedStatusHandlers } from './domains/published-status.handlers';
import * as serverHandlers from './domains/server.handlers';
import { handlers as upgradeHandlers } from './domains/upgrade.handlers';
import { handlers as userHandlers } from './domains/user.handlers';
import { handlers as telemetryHandlers } from './domains/telemetry.handlers';
import { handlers as usersHandlers } from './domains/users.handlers';
import { handlers as userGroupsHandlers } from './domains/user-groups.handlers';
import { handlers as examineManagementHandlers } from './domains/examine-management.handlers';
import { handlers as modelsBuilderHandlers } from './domains/modelsbuilder.handlers';
import { handlers as healthCheckHandlers } from './domains/health-check.handlers';
import { handlers as profilingHandlers } from './domains/performance-profiling.handlers';
import { handlers as documentHandlers } from './domains/document.handlers';
import { handlers as mediaHandlers } from './domains/media.handlers';
import { handlers as dictionaryHandlers } from './domains/dictionary.handlers';
import { handlers as mediaTypeHandlers } from './domains/media-type.handlers';
import { handlers as memberGroupHandlers } from './domains/member-group.handlers';
import { handlers as memberHandlers } from './domains/member.handlers';
import { handlers as memberTypeHandlers } from './domains/member-type.handlers';
import { handlers as templateHandlers } from './domains/template.handlers';
import { handlers as languageHandlers } from './domains/language.handlers';
import { handlers as cultureHandlers } from './domains/culture.handlers';
import { handlers as redirectManagementHandlers } from './domains/redirect-management.handlers';
import { handlers as logViewerHandlers } from './domains/log-viewer.handlers';
import { handlers as packageHandlers } from './domains/package.handlers';

const handlers = [
	serverHandlers.serverVersionHandler,
	...installHandlers,
	...upgradeHandlers,
	...userHandlers,
	...documentHandlers,
	...mediaHandlers,
	...dataTypeHandlers,
	...documentTypeHandlers,
	...telemetryHandlers,
	...publishedStatusHandlers,
	...usersHandlers,
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
