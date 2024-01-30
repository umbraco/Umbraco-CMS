import { recycleBinHandlers } from './recycle-bin.handlers.js';
import { treeHandlers } from './tree.handlers.js';
import { itemHandlers } from './item.handlers.js';
import { detailHandlers } from './detail.handlers.js';

export const handlers = [...recycleBinHandlers, ...treeHandlers, ...itemHandlers, ...detailHandlers];
