import { folderHandlers } from './folder.handlers';
import { treeHandlers } from './tree.handlers';
import { detailHandlers } from './detail.handlers';
import { itemHandlers } from './item.handlers';
import { moveHandlers } from './move.handlers';

export const handlers = [...treeHandlers, ...itemHandlers, ...folderHandlers, ...moveHandlers, ...detailHandlers];
