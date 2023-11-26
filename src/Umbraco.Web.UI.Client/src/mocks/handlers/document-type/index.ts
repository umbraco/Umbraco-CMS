import { handlers as treeHandlers } from './tree.handlers.js';
import { handlers as detailHandlers } from './detail.handlers.js';
import { handlers as itemHandlers } from './item.handlers.js';

export const handlers = [...treeHandlers, ...itemHandlers, ...detailHandlers];
