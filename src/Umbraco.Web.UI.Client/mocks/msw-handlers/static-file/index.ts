import { treeHandlers } from './tree.handlers.js';
import { itemHandlers } from './item.handlers.js';

export const handlers = [...treeHandlers, ...itemHandlers];
