import { detailHandlers } from './detail.handlers.js';
import { itemHandlers } from './item.handlers.js';
import { eventsHandlers } from './events.handlers.js';
import { deliveryHandlers } from './delivery.handlers.js';

// NOTE: eventsHandlers must be registered before detailHandlers because
// GET /webhook/events would otherwise be matched by GET /webhook/:id.
export const handlers = [...eventsHandlers, ...itemHandlers, ...detailHandlers, ...deliveryHandlers];
