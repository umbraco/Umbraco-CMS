import { auditLogHandlers } from './audit-log.handlers.js';
import { recycleBinHandlers } from './recycle-bin.handlers.js';
import { treeHandlers } from './tree.handlers.js';
import { itemHandlers } from './item.handlers.js';
import { permissionHandlers } from './permission.handlers.js';
import { publishingHandlers } from './publishing.handlers.js';
import { detailHandlers } from './detail.handlers.js';
import { domainHandlers } from './domain.handlers.js';
import { collectionHandlers } from './collection.handlers.js';
import { urlHandlers } from './url.handlers.js';

export const handlers = [
	...auditLogHandlers,
	...recycleBinHandlers,
	...permissionHandlers,
	...treeHandlers,
	...itemHandlers,
	...publishingHandlers,
	...urlHandlers,
	...detailHandlers,
	...domainHandlers,
	...collectionHandlers,
];
