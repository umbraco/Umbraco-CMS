import { handlers as recycleBinHandlers } from './recycle-bin.handlers.js';
import { handlers as documentHandlers } from './document.handlers.js';

export const handlers = [...recycleBinHandlers, ...documentHandlers];
