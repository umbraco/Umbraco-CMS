import { handlers as dataTypeHandlers } from './domains/data-type.handlers';
import { handlers as documentTypeHandlers } from './domains/document-type.handlers';
import { handlers as treeHandlers } from './domains/entity.handlers';
import { handlers as installHandlers } from './domains/install.handlers';
import * as manifestsHandlers from './domains/manifests.handlers';
import { handlers as contentHandlers } from './domains/node.handlers';
import { handlers as publishedStatusHandlers } from './domains/published-status.handlers';
import * as serverHandlers from './domains/server.handlers';
import { handlers as upgradeHandlers } from './domains/upgrade.handlers';
import { handlers as userHandlers } from './domains/user.handlers';
import { handlers as propertyEditorHandlers } from './domains/property-editor.handlers';

const handlers = [
	serverHandlers.serverVersionHandler,
	...contentHandlers,
	...installHandlers,
	...upgradeHandlers,
	...userHandlers,
	...dataTypeHandlers,
	...documentTypeHandlers,
	...treeHandlers,
	...propertyEditorHandlers,
	...manifestsHandlers.default,
	...publishedStatusHandlers,
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

switch (import.meta.env.MODE) {
	case 'development':
		handlers.push(manifestsHandlers.manifestDevelopmentHandler);
		break;

	default:
		handlers.push(manifestsHandlers.manifestEmptyHandler);
}

export { handlers };
