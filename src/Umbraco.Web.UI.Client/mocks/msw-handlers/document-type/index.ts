import { configurationHandlers } from './configuration.handlers.js';
import { treeHandlers } from './tree.handlers.js';
import { detailHandlers } from './detail.handlers.js';
import { itemHandlers } from './item.handlers.js';
import { folderHandlers } from './folder.handlers.js';
import { structureHandlers } from './structure.handlers.js';
import { moveHandlers } from './move.handlers.js';
import { copyHandlers } from './copy.handlers.js';

export const handlers = [
	...configurationHandlers,
	...treeHandlers,
	...itemHandlers,
	...folderHandlers,
	...structureHandlers,
	...detailHandlers,
	...moveHandlers,
	...copyHandlers,
];
