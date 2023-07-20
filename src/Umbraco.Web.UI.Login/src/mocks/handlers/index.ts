import { handlers as backofficeHandlers } from './backoffice.handlers.ts';
import { handlers as loginHandlers } from './login.handlers.js';

const handlers = [...backofficeHandlers, ...loginHandlers];

export { handlers };
