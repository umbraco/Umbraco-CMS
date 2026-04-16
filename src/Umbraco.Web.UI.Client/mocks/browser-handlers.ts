import { handlers as backofficeHandlers } from './msw-handlers/backoffice/backoffice.handlers.js';
import { handlers as configHandlers } from './msw-handlers/config.handlers.js';
import { handlers as cultureHandlers } from './msw-handlers/culture.handlers.js';
import { handlers as dataTypeHandlers } from './msw-handlers/data-type/index.js';
import { handlers as dictionaryHandlers } from './msw-handlers/dictionary/index.js';
import { handlers as documentHandlers } from './msw-handlers/document/index.js';
import { handlers as documentTypeHandlers } from './msw-handlers/document-type/index.js';
import { handlers as dynamicRootHandlers } from './msw-handlers/dynamic-root.handlers.js';
import { handlers as examineManagementHandlers } from './msw-handlers/examine-management.handlers.js';
import { handlers as healthCheckHandlers } from './msw-handlers/health-check.handlers.js';
import { handlers as installHandlers } from './msw-handlers/install.handlers.js';
import { handlers as languageHandlers } from './msw-handlers/language/index.js';
import { handlers as logViewerHandlers } from './msw-handlers/log-viewer.handlers.js';
import { handlers as mediaHandlers } from './msw-handlers/media/index.js';
import { handlers as mediaTypeHandlers } from './msw-handlers/media-type/index.js';
import { handlers as memberGroupHandlers } from './msw-handlers/member-group/index.js';
import { handlers as memberHandlers } from './msw-handlers/member/index.js';
import { handlers as memberTypeHandlers } from './msw-handlers/member-type/index.js';
import { handlers as modelsBuilderHandlers } from './msw-handlers/modelsbuilder.handlers.js';
import { handlers as objectTypeHandlers } from './msw-handlers/object-type/index.js';
import { handlers as packageHandlers } from './msw-handlers/package.handlers.js';
import { handlers as partialViewHandlers } from './msw-handlers/partial-view/index.js';
import { handlers as profilingHandlers } from './msw-handlers/performance-profiling.handlers.js';
import { handlers as publishedStatusHandlers } from './msw-handlers/published-status.handlers.js';
import { handlers as redirectManagementHandlers } from './msw-handlers/redirect-management.handlers.js';
import { handlers as relationTypeHandlers } from './msw-handlers/relation-type/index.js';
import { handlers as relationHandlers } from './msw-handlers/relation/index.js';
import { handlers as rteEmbedHandlers } from './msw-handlers/rte-embed.handlers.js';
import { handlers as scriptHandlers } from './msw-handlers/script/index.js';
import { handlers as staticFileHandlers } from './msw-handlers/static-file/index.js';
import { handlers as stylesheetHandlers } from './msw-handlers/stylesheet/index.js';
import { handlers as tagHandlers } from './msw-handlers/tag-handlers.js';
import { handlers as telemetryHandlers } from './msw-handlers/telemetry.handlers.js';
import { handlers as templateHandlers } from './msw-handlers/template/index.js';
import { handlers as umbracoNewsHandlers } from './msw-handlers/umbraco-news.handlers.js';
import { handlers as upgradeHandlers } from './msw-handlers/upgrade.handlers.js';
import { handlers as userGroupsHandlers } from './msw-handlers/user-group/index.js';
import { handlers as userHandlers } from './msw-handlers/user/index.js';
import * as manifestsHandlers from './msw-handlers/manifests.handlers.js';
import * as serverHandlers from './msw-handlers/server.handlers.js';
import { handlers as documentBlueprintHandlers } from './msw-handlers/document-blueprint/index.js';
import { handlers as temporaryFileHandlers } from './msw-handlers/temporary-file/index.js';
import { handlers as segmentHandlers } from './msw-handlers/segment.handlers.js';

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
	...umbracoNewsHandlers,
	...upgradeHandlers,
	...userGroupsHandlers,
	...userHandlers,
	...documentBlueprintHandlers,
	...temporaryFileHandlers,
	...segmentHandlers,
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
