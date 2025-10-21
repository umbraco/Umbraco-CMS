import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbBooleanState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDocumentPreviewRepository } from '@umbraco-cms/backoffice/document';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { HubConnectionBuilder, type HubConnection } from '@umbraco-cms/backoffice/external/signalr';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

const UMB_LOCALSTORAGE_SESSION_KEY = 'umb:previewSessions';

interface UmbPreviewIframeArgs {
	culture?: string;
	height?: string;
	segment?: string;
	width?: string;
	wrapperClass?: string;
}

interface UmbPreviewUrlArgs {
	culture?: string | null;
	rnd?: number;
	segment?: string | null;
	serverUrl?: string;
	unique?: string | null;
}

export class UmbPreviewContext extends UmbContextBase {
	#unique?: string | null;
	#culture?: string | null;
	#segment?: string | null;
	#serverUrl: string = '';
	#connection?: HubConnection;

	#iframeReady = new UmbBooleanState(false);
	public readonly iframeReady = this.#iframeReady.asObservable();

	#previewUrl = new UmbStringState(undefined);
	public readonly previewUrl = this.#previewUrl.asObservable();

	#documentPreviewRepository = new UmbDocumentPreviewRepository(this);

	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	#localize = new UmbLocalizationController(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_PREVIEW_CONTEXT);

		this.consumeContext(UMB_SERVER_CONTEXT, (instance) => {
			const params = new URLSearchParams(window.location.search);

			this.#unique = params.get('id');
			this.#culture = params.get('culture');
			this.#segment = params.get('segment');

			if (!this.#unique) {
				console.error('No unique ID found in query string.');
				return;
			}

			const serverUrl = instance?.getServerUrl();

			if (!serverUrl) {
				console.error('No server URL found in context');
				return;
			}

			this.#serverUrl = serverUrl;

			this.#setPreviewUrl({ serverUrl });

			this.#initHubConnection(serverUrl);
		});

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
			this.#notificationContext = instance;
		});
	}

	async #initHubConnection(serverUrl: string) {
		const previewHubUrl = `${serverUrl}/umbraco/PreviewHub`;

		// Make sure that no previous connection exists.
		if (this.#connection) {
			await this.#connection.stop();
			this.#connection = undefined;
		}

		this.#connection = new HubConnectionBuilder().withUrl(previewHubUrl).build();

		this.#connection.on('refreshed', (payload) => {
			if (payload === this.#unique) {
				this.#setPreviewUrl({ rnd: Math.random() });
			}
		});

		this.#connection.onclose(() => {
			this.#notificationContext?.peek('warning', {
				data: {
					headline: this.#localize.term('general_preview'),
					message: this.#localize.term('preview_connectionLost'),
				},
			});
		});

		try {
			await this.#connection.start();
		} catch (error) {
			console.error('The SignalR connection could not be established', error);
			this.#notificationContext?.peek('warning', {
				data: {
					headline: this.#localize.term('general_preview'),
					message: this.#localize.term('preview_connectionFailed'),
				},
			});
		}
	}

	async #getPublishedUrl(): Promise<string | null> {
		if (!this.#unique) return null;

		// NOTE: We should be reusing `UmbDocumentUrlRepository` here, but the preview app doesn't register the `itemStore` extensions, so can't resolve/consume `UMB_DOCUMENT_URL_STORE_CONTEXT`. [LK]
		const { data } = await tryExecute(this, DocumentService.getDocumentUrls({ query: { id: [this.#unique] } }));

		if (!data?.length) return null;
		const urlInfo = this.#culture ? data[0].urlInfos.find((x) => x.culture === this.#culture) : data[0].urlInfos[0];

		if (!urlInfo?.url) return null;
		return urlInfo.url.startsWith('/') ? `${this.#serverUrl}${urlInfo.url}` : urlInfo.url;
	}

	#getSessionCount(): number {
		return Math.max(Number(localStorage.getItem(UMB_LOCALSTORAGE_SESSION_KEY)), 0) || 0;
	}

	#setPreviewUrl(args?: UmbPreviewUrlArgs) {
		const host = args?.serverUrl || this.#serverUrl;
		const unique = args?.unique || this.#unique;

		if (!unique) {
			throw new Error('No unique ID found in query string.');
		}

		const url = new URL(unique, host);
		const params = new URLSearchParams(url.search);

		const culture = args?.culture || this.#culture;
		const segment = args?.segment || this.#segment;

		const cultureParam = 'culture';
		const rndParam = 'rnd';
		const segmentParam = 'segment';

		if (culture) {
			params.set(cultureParam, culture);
		} else {
			params.delete(cultureParam);
		}

		if (args?.rnd) {
			params.set(rndParam, args.rnd.toString());
		} else {
			params.delete(rndParam);
		}

		if (segment) {
			params.set(segmentParam, segment);
		} else {
			params.delete(segmentParam);
		}

		const previewUrl = new URL(`${url.pathname}?${params.toString()}`, host);
		const previewUrlString = previewUrl.toString();

		this.#previewUrl.setValue(previewUrlString);
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

		if (this.#connection) {
			await this.#connection.stop();
			this.#connection = undefined;
		}

		let url = await this.#getPublishedUrl();

		if (!url) {
			url = this.#previewUrl.getValue() as string;
		}

		window.location.replace(url);
	}

	async exitSession() {
		let sessions = this.#getSessionCount();
		sessions--;
		this.exitPreview(sessions);
	}

	iframeLoaded(iframe: HTMLIFrameElement) {
		if (!iframe) return;
		this.#iframeReady.setValue(true);
	}

	getIFrameWrapper(): HTMLElement | undefined {
		return this.getHostElement().shadowRoot?.querySelector('#wrapper') as HTMLElement;
	}

	async openWebsite() {
		let url = await this.#getPublishedUrl();

		if (!url) {
			url = this.#previewUrl.getValue() as string;
		}

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

	#currentArgs: UmbPreviewIframeArgs = {};
	async updateIFrame(args?: UmbPreviewIframeArgs) {
		const mergedArgs = { ...this.#currentArgs, ...args };

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

		this.#iframeReady.setValue(false);

		const params = new URLSearchParams(window.location.search);

		if (mergedArgs.culture) {
			params.set('culture', mergedArgs.culture);
		} else {
			params.delete('culture');
		}

		if (mergedArgs.segment) {
			params.set('segment', mergedArgs.segment);
		} else {
			params.delete('segment');
		}

		const newRelativePathQuery = window.location.pathname + '?' + params.toString();
		history.pushState(null, '', newRelativePathQuery);

		this.#currentArgs = mergedArgs;

		this.#setPreviewUrl({ culture: mergedArgs.culture, segment: mergedArgs.segment });

		if (mergedArgs.wrapperClass) wrapper.className = mergedArgs.wrapperClass;
		if (mergedArgs.height) wrapper.style.height = mergedArgs.height;
		if (mergedArgs.width) wrapper.style.width = mergedArgs.width;
	}
}

export const UMB_PREVIEW_CONTEXT = new UmbContextToken<UmbPreviewContext>('UmbPreviewContext');
