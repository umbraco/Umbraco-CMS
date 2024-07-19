import { folderHandlers } from './folder.handlers.js';
import { treeHandlers } from './tree.handlers.js';
import { detailHandlers } from './detail.handlers.js';
import { itemHandlers } from './item.handlers.js';
import { moveHandlers } from './move.handlers.js';
import { copyHandlers } from './copy.handlers.js';

export const handlers = [
	...treeHandlers,
	...itemHandlers,
	...folderHandlers,
	...detailHandlers,
	...moveHandlers,
	...copyHandlers,
];
