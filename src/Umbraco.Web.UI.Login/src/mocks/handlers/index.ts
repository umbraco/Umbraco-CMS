import { handlers as backofficeHandlers } from './backoffice.handlers.js';
import { handlers as loginHandlers } from './login.handlers.js';

const handlers = [...backofficeHandlers, ...loginHandlers];

export { handlers };
