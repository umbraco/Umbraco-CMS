import { folderHandlers } from './folder.handlers.js';
import { treeHandlers } from './tree.handlers.js';
import { detailHandlers } from './detail.handlers.js';
import { itemHandlers } from './item.handlers.js';
import { snippetHandlers } from './snippet.handlers.js';
import { renameHandlers } from './rename.handlers.js';

export const handlers = [
	...treeHandlers,
	...itemHandlers,
	...folderHandlers,
	...snippetHandlers,
	...renameHandlers,
	...detailHandlers,
];
