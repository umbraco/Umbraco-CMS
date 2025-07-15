import { recycleBinHandlers } from './recycle-bin.handlers.js';
import { treeHandlers } from './tree.handlers.js';
import { itemHandlers } from './item.handlers.js';
import { permissionHandlers } from './permission.handlers.js';
import { publishingHandlers } from './publishing.handlers.js';
import { detailHandlers } from './detail.handlers.js';
import { domainHandlers } from './domain.handlers.js';
import { collectionHandlers } from './collection.handlers.js';

export const handlers = [
	...recycleBinHandlers,
	...permissionHandlers,
	...treeHandlers,
	...itemHandlers,
	...publishingHandlers,
	...detailHandlers,
	...domainHandlers,
	...collectionHandlers,
];
