import { handlers as contentHandlers } from './domains/content.handlers';
import { handlers as installHandlers } from './domains/install.handlers';
import { handlers as manifestsHandlers } from './domains/manifests.handlers';
import * as serverHandlers from './domains/server.handlers';
import { handlers as upgradeHandlers } from './domains/upgrade.handlers';
import { handlers as userHandlers } from './domains/user.handlers';

const handlers = [
	serverHandlers.serverVersionHandler,
	...contentHandlers,
	...installHandlers,
	...upgradeHandlers,
	...manifestsHandlers,
	...userHandlers,
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

export { handlers };
