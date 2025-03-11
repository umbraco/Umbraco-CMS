import { handlers as backofficeHandlers } from './handlers/backoffice/backoffice.handlers.js';
import { handlers as configHandlers } from './handlers/config.handlers.js';
import { handlers as cultureHandlers } from './handlers/culture.handlers.js';
import { handlers as dataTypeHandlers } from './handlers/data-type/index.js';
import { handlers as dictionaryHandlers } from './handlers/dictionary/index.js';
import { handlers as documentHandlers } from './handlers/document/index.js';
import { handlers as documentTypeHandlers } from './handlers/document-type/index.js';
import { handlers as dynamicRootHandlers } from './handlers/dynamic-root.handlers.js';
import { handlers as examineManagementHandlers } from './handlers/examine-management.handlers.js';
import { handlers as healthCheckHandlers } from './handlers/health-check.handlers.js';
import { handlers as installHandlers } from './handlers/install.handlers.js';
import { handlers as languageHandlers } from './handlers/language/index.js';
import { handlers as logViewerHandlers } from './handlers/log-viewer.handlers.js';
import { handlers as mediaHandlers } from './handlers/media/index.js';
import { handlers as mediaTypeHandlers } from './handlers/media-type/index.js';
import { handlers as memberGroupHandlers } from './handlers/member-group/index.js';
import { handlers as memberHandlers } from './handlers/member/index.js';
import { handlers as memberTypeHandlers } from './handlers/member-type/index.js';
import { handlers as modelsBuilderHandlers } from './handlers/modelsbuilder.handlers.js';
import { handlers as objectTypeHandlers } from './handlers/object-type/index.js';
import { handlers as packageHandlers } from './handlers/package.handlers.js';
import { handlers as partialViewHandlers } from './handlers/partial-view/index.js';
import { handlers as profilingHandlers } from './handlers/performance-profiling.handlers.js';
import { handlers as publishedStatusHandlers } from './handlers/published-status.handlers.js';
import { handlers as redirectManagementHandlers } from './handlers/redirect-management.handlers.js';
import { handlers as relationTypeHandlers } from './handlers/relation-type/index.js';
import { handlers as relationHandlers } from './handlers/relation/index.js';
import { handlers as rteEmbedHandlers } from './handlers/rte-embed.handlers.js';
import { handlers as scriptHandlers } from './handlers/script/index.js';
import { handlers as staticFileHandlers } from './handlers/static-file/index.js';
import { handlers as stylesheetHandlers } from './handlers/stylesheet/index.js';
import { handlers as tagHandlers } from './handlers/tag-handlers.js';
import { handlers as telemetryHandlers } from './handlers/telemetry.handlers.js';
import { handlers as templateHandlers } from './handlers/template/index.js';
import { handlers as upgradeHandlers } from './handlers/upgrade.handlers.js';
import { handlers as userGroupsHandlers } from './handlers/user-group/index.js';
import { handlers as userHandlers } from './handlers/user/index.js';
import * as manifestsHandlers from './handlers/manifests.handlers.js';
import * as serverHandlers from './handlers/server.handlers.js';
import { handlers as documentBlueprintHandlers } from './handlers/document-blueprint/index.js';
import { handlers as temporaryFileHandlers } from './handlers/temporary-file/index.js';

const handlers = [
	...backofficeHandlers,
	...configHandlers,
	...cultureHandlers,
	...dataTypeHandlers,
	...dictionaryHandlers,
	...documentHandlers,
	...documentTypeHandlers,
	...dynamicRootHandlers,
	...examineManagementHandlers,
	...healthCheckHandlers,
	...installHandlers,
	...languageHandlers,
	...logViewerHandlers,
	...mediaHandlers,
	...mediaTypeHandlers,
	...memberGroupHandlers,
	...memberHandlers,
	...memberTypeHandlers,
	...modelsBuilderHandlers,
	...objectTypeHandlers,
	...packageHandlers,
	...partialViewHandlers,
	...profilingHandlers,
	...publishedStatusHandlers,
	...redirectManagementHandlers,
	...relationTypeHandlers,
	...relationHandlers,
	...rteEmbedHandlers,
	...scriptHandlers,
	...staticFileHandlers,
	...stylesheetHandlers,
	...tagHandlers,
	...telemetryHandlers,
	...templateHandlers,
	...upgradeHandlers,
	...userGroupsHandlers,
	...userHandlers,
	...documentBlueprintHandlers,
	...temporaryFileHandlers,
	...serverHandlers.serverInformationHandlers,
	serverHandlers.serverRunningHandler,
	...manifestsHandlers.manifestEmptyHandlers,
];

/* TODO: find solution to run with different handlers across vite mocks and web-test-runner mocks
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
		handlers.push(...manifestsHandlers.manifestDevelopmentHandlers);
		break;

	default:
		handlers.push(...manifestsHandlers.manifestEmptyHandlers);
}
*/

export { handlers };
