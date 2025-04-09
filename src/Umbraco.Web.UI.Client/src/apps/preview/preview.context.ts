import { UmbBooleanState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDocumentPreviewRepository } from '@umbraco-cms/backoffice/document';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';

const UMB_LOCALSTORAGE_SESSION_KEY = 'umb:previewSessions';

export class UmbPreviewContext extends UmbContextBase<UmbPreviewContext> {
	#culture?: string | null;
	#serverUrl: string = '';
	#webSocket?: WebSocket;
	#unique?: string | null;

	#iframeReady = new UmbBooleanState(false);
	public readonly iframeReady = this.#iframeReady.asObservable();

	#previewUrl = new UmbStringState(undefined);
	public readonly previewUrl = this.#previewUrl.asObservable();

	#documentPreviewRepository = new UmbDocumentPreviewRepository(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_PREVIEW_CONTEXT);

		this.consumeContext(UMB_SERVER_CONTEXT, (instance) => {
			this.#serverUrl = instance.getServerUrl();

			const params = new URLSearchParams(window.location.search);

			this.#culture = params.get('culture');
			this.#unique = params.get('id');

			if (!this.#unique) {
				console.error('No unique ID found in query string.');
				return;
			}

			this.#setPreviewUrl();
		});
	}

	#configureWebSocket() {
		if (this.#webSocket && this.#webSocket.readyState < 2) return;

		const url = `${this.#serverUrl.replace('https://', 'wss://')}/umbraco/PreviewHub`;

		this.#webSocket = new WebSocket(url);

		this.#webSocket.addEventListener('open', () => {
			// NOTE: SignalR protocol handshake; it requires a terminating control character.
			const endChar = String.fromCharCode(30);
			this.#webSocket?.send(`{"protocol":"json","version":1}${endChar}`);
		});

		this.#webSocket.addEventListener('message', (event: MessageEvent<string>) => {
			if (!event?.data) return;

			// NOTE: Strip the terminating control character, (from SignalR).
			const data = event.data.substring(0, event.data.length - 1);
			const json = JSON.parse(data) as { type: number; target: string; arguments: Array<string> };

			if (json.type === 1 && json.target === 'refreshed') {
				const pageId = json.arguments?.[0];
				if (pageId === this.#unique) {
					this.#setPreviewUrl({ rnd: Math.random() });
				}
			}
		});
	}

	#getSessionCount(): number {
		return Math.max(Number(localStorage.getItem(UMB_LOCALSTORAGE_SESSION_KEY)), 0) || 0;
	}

	#setPreviewUrl(args?: { serverUrl?: string; unique?: string | null; culture?: string | null; rnd?: number }) {
		const host = args?.serverUrl || this.#serverUrl;
		const path = args?.unique || this.#unique;
		const params = new URLSearchParams();
		const culture = args?.culture || this.#culture;

		if (culture) params.set('culture', culture);
		if (args?.rnd) params.set('rnd', args.rnd.toString());

		this.#previewUrl.setValue(`${host}/${path}?${params}`);
	}

	#setSessionCount(sessions: number) {
		localStorage.setItem(UMB_LOCALSTORAGE_SESSION_KEY, sessions.toString());
	}

	checkSession() {
		const sessions = this.#getSessionCount();
		if (sessions > 0) return;

		umbConfirmModal(this._host, {
			headline: `Preview website?`,
			content: `You have ended preview mode, do you want to enable it again to view the latest saved version of your website?`,
			cancelLabel: 'View published version',
			confirmLabel: 'Preview latest version',
		})
			.then(() => {
				this.restartSession();
			})
			.catch(() => {
				this.exitSession();
			});
	}

	async exitPreview(sessions: number = 0) {
		this.#setSessionCount(sessions);

		// We are good to end preview mode.
		if (sessions <= 0) {
			await this.#documentPreviewRepository.exit();
		}

		if (this.#webSocket) {
			this.#webSocket.close();
			this.#webSocket = undefined;
		}

		const url = this.#previewUrl.getValue() as string;
		window.location.replace(url);
	}

	async exitSession() {
		let sessions = this.#getSessionCount();
		sessions--;
		this.exitPreview(sessions);
	}

	iframeLoaded(iframe: HTMLIFrameElement) {
		if (!iframe) return;
		this.#configureWebSocket();
		this.#iframeReady.setValue(true);
	}

	getIFrameWrapper(): HTMLElement | undefined {
		return this.getHostElement().shadowRoot?.querySelector('#wrapper') as HTMLElement;
	}

	openWebsite() {
		const url = this.#previewUrl.getValue() as string;
		window.open(url, '_blank');
	}

	reloadIFrame(iframe: HTMLIFrameElement) {
		const document = iframe.contentDocument;
		if (!document) return;

		document.location.reload();
	}

	async restartSession() {
		await this.#documentPreviewRepository.enter();
		this.startSession();
	}

	startSession() {
		let sessions = this.#getSessionCount();
		sessions++;
		this.#setSessionCount(sessions);
	}

	async updateIFrame(args?: { culture?: string; className?: string; height?: string; width?: string }) {
		if (!args) return;

		const wrapper = this.getIFrameWrapper();
		if (!wrapper) return;

		const scaleIFrame = () => {
			if (wrapper.className === 'fullsize') {
				wrapper.style.transform = '';
			} else {
				const wScale = document.body.offsetWidth / (wrapper.offsetWidth + 30);
				const hScale = document.body.offsetHeight / (wrapper.offsetHeight + 30);
				const scale = Math.min(wScale, hScale, 1); // get the lowest ratio, but not higher than 1
				wrapper.style.transform = `scale(${scale})`;
			}
		};

		window.addEventListener('resize', scaleIFrame);
		wrapper.addEventListener('transitionend', scaleIFrame);

		if (args.culture) {
			this.#iframeReady.setValue(false);

			const params = new URLSearchParams(window.location.search);
			params.set('culture', args.culture);
			const newRelativePathQuery = window.location.pathname + '?' + params.toString();
			history.pushState(null, '', newRelativePathQuery);

			this.#setPreviewUrl({ culture: args.culture });
		}

		if (args.className) wrapper.className = args.className;
		if (args.height) wrapper.style.height = args.height;
		if (args.width) wrapper.style.width = args.width;
	}
}

export const UMB_PREVIEW_CONTEXT = new UmbContextToken<UmbPreviewContext>('UmbPreviewContext');
