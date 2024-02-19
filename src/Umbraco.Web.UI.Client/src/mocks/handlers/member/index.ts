import { detailHandlers } from './detail.handlers.js';
import { itemHandlers } from './item.handlers.js';
import { collectionHandlers } from './collection.handlers.js';

export const handlers = [...itemHandlers, ...collectionHandlers, ...detailHandlers];
