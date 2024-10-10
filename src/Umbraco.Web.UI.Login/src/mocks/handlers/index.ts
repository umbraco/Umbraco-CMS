import { handlers as loginHandlers } from './login.handlers.js';
import type { HttpHandler } from "msw";

const handlers: HttpHandler[] = [...loginHandlers];

export { handlers };
