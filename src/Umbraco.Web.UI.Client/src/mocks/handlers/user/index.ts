import { handlers as detailHandlers } from './detail.handlers.js';
import { handlers as itemHandlers } from './item.handlers.js';
import { handlers as currentHandlers } from './current.handlers.js';
import { handlers as setUserGroupsHandlers } from './set-user-groups.handlers.js';

export const handlers = [...itemHandlers, ...currentHandlers, ...setUserGroupsHandlers, ...detailHandlers];
