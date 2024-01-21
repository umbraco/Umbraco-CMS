import { recycleBinHandlers } from './recycle-bin.handlers.js';
import { treeHandlers } from './tree.handlers.js';
import { itemHandlers } from './item.handlers.js';
import { permissionHandlers } from './permission.handlers.js';
import { publishingHandlers } from './publishing.handlers.js';
import { detailHandlers } from './detail.handlers.js';
import { allowedTypesHandlers } from './allowed-types.handlers.js';

export const handlers = [
	...recycleBinHandlers,
	...permissionHandlers,
	...treeHandlers,
	...itemHandlers,
	...publishingHandlers,
	...allowedTypesHandlers,
	...detailHandlers,
];
