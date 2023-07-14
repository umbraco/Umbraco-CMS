import { startMockServiceWorker } from "./mocks/index.js";

//NOTE: Don't know why typescript is complaining about this, but it works.
const isMocking = import.meta.env.VITE_UMBRACO_AUTH_USE_MSW;

if (isMocking) {
  startMockServiceWorker();
}
