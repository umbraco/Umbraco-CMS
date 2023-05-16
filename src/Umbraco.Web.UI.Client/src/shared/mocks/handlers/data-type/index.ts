import { folderHandlers } from './folder.handlers';
import { treeHandlers } from './tree.handlers';
import { detailHandlers } from './detail.handlers';
import { itemHandlers } from './item.handlers';
import { moveHandlers } from './move.handlers';
import { copyHandlers } from './copy.handlers';

export const handlers = [
	...treeHandlers,
	...itemHandlers,
	...folderHandlers,
	...moveHandlers,
	...copyHandlers,
	...detailHandlers,
];
