import { handlers as recycleBinHandlers } from './recycle-bin.handlers.js';
import { handlers as treeHandlers } from './tree.handlers.js';
import { handlers as documentHandlers } from './document.handlers.js';

export const handlers = [...recycleBinHandlers, ...treeHandlers, ...documentHandlers];
