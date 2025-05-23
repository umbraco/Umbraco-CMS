import { detailHandlers } from './detail.handlers.js';
import { itemHandlers } from './item.handlers.js';
import { filterHandlers } from './filter.handlers.js';

export const handlers = [...itemHandlers, ...filterHandlers, ...detailHandlers];
