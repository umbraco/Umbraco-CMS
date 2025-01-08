import { detailHandlers } from './detail.handlers.js';
import { itemHandlers } from './item.handlers.js';

export const handlers = [...itemHandlers, ...detailHandlers];
