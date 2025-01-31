import { recycleBinHandlers } from './recycle-bin.handlers.js';
import { treeHandlers } from './tree.handlers.js';
import { itemHandlers } from './item.handlers.js';
import { detailHandlers } from './detail.handlers.js';
import { collectionHandlers } from './collection.handlers.js';
import { imagingHandlers } from './imaging.handlers.js';

export const handlers = [
	...recycleBinHandlers,
	...treeHandlers,
	...itemHandlers,
	...detailHandlers,
	...collectionHandlers,
	...imagingHandlers,
];
