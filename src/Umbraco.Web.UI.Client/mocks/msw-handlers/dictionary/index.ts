import { treeHandlers } from './tree.handlers.js';
import { detailHandlers } from './detail.handlers.js';
import { itemHandlers } from './item.handlers.js';
import { importExportHandlers } from './import-export.handlers.js';
import { uploadHandlers } from './upload.handlers.js';

export const handlers = [
	...treeHandlers,
	...itemHandlers,
	...uploadHandlers,
	...importExportHandlers,
	...detailHandlers,
];
