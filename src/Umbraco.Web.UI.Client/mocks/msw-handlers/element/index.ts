import { recycleBinHandlers } from './recycle-bin.handlers.js';
import { treeHandlers } from './tree.handlers.js';
import { itemHandlers } from './item.handlers.js';
import { publishingHandlers } from './publishing.handlers.js';
import { detailHandlers } from './detail.handlers.js';
import { folderHandlers } from './folder.handlers.js';
import { moveCopyHandlers } from './move-copy.handlers.js';

export const handlers = [
	...recycleBinHandlers,
	...treeHandlers,
	...itemHandlers,
	...publishingHandlers,
	...detailHandlers,
	...folderHandlers,
	...moveCopyHandlers,
];
