const { http, HttpResponse, ws } = window.MockServiceWorker;

const HUB_PATH = '/umbraco/serverEventHub';
const RECORD_SEPARATOR = '\x1e';
const PING_MESSAGE = `{"type":6}${RECORD_SEPARATOR}`;
const PING_INTERVAL_MS = 15000;

const serverEventHub = ws.link(`*${HUB_PATH}`);

export const handlers = [
	http.post(`${HUB_PATH}/negotiate`, () => {
		return HttpResponse.json({
			negotiateVersion: 1,
			connectionId: 'mock-connection-id',
			connectionToken: 'mock-connection-token',
			availableTransports: [{ transport: 'WebSockets', transferFormats: ['Text', 'Binary'] }],
		});
	}),

	serverEventHub.addEventListener('connection', ({ client }) => {
		const pingInterval = setInterval(() => client.send(PING_MESSAGE), PING_INTERVAL_MS);

		client.addEventListener('message', (event) => {
			if (typeof event.data === 'string' && event.data.includes('"protocol"')) {
				client.send(`{}${RECORD_SEPARATOR}`);
			}
		});

		client.addEventListener('close', () => clearInterval(pingInterval));
	}),
];
