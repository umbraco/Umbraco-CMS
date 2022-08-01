import { handlers as contentHandlers } from './domains/content.handlers';
import { handlers as installHandlers } from './domains/install.handlers';
import { handlers as manifestsHandlers } from './domains/manifests.handlers';
import { handlers as serverHandlers } from './domains/server.handlers';
import { handlers as upgradeHandlers } from './domains/upgrade.handlers';
import { handlers as userHandlers } from './domains/user.handlers';
import { handlers as dataTypeHandlers } from './domains/data-type.handlers';

export const handlers = [
	...serverHandlers,
	...contentHandlers,
	...installHandlers,
	...upgradeHandlers,
	...manifestsHandlers,
	...userHandlers,
	...dataTypeHandlers,
];
