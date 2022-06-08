import { setupWorker } from 'msw';
import { handlers } from './handlers';

const worker = setupWorker(...handlers);

export const startMockServiceWorker = () => worker.start({
  onUnhandledRequest: (req) => {
    if (req.url.pathname.startsWith('/node_modules/')) return;
    if (req.url.pathname.startsWith('/src/')) return;
    if (req.destination === 'image') return;

    console.warn('Found an unhandled %s request to %s', req.method, req.url.href);
  },
});