import { handlers as detailHandlers } from './detail.handlers.js';
import { handlers as treeHandlers } from './tree.handlers.js';

export const relationTypeHandlers = [...detailHandlers, ...treeHandlers];
