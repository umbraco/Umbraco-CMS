import { detailHandlers } from './detail.handlers.js';
import { itemHandlers } from './item.handlers.js';
import { handlers as currentHandlers } from './current.handlers.js';
import { handlers as setUserGroupsHandlers } from './set-user-groups.handlers.js';
import { handlers as enableHandlers } from './enable.handlers.js';
import { handlers as disableHandlers } from './disable.handlers.js';
import { handlers as changePasswordHandlers } from './change-password.handlers.js';
import { handlers as unlockHandlers } from './unlock.handlers.js';
import { handlers as inviteHandlers } from './invite.handlers.js';
import { handlers as filterHandlers } from './filter.handlers.js';
import { handlers as mfaHandlers } from './mfa.handlers.js';

export const handlers = [
	...itemHandlers,
	...currentHandlers,
	...enableHandlers,
	...disableHandlers,
	...setUserGroupsHandlers,
	...changePasswordHandlers,
	...unlockHandlers,
	...filterHandlers,
	...inviteHandlers,
	...detailHandlers,
	...mfaHandlers,
];
