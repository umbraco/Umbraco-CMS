import { UmbPreviewRepository } from '../repository/index.js';
import { UMB_PREVIEW_CONTEXT } from './preview.context-token.js';
import { HubConnectionBuilder } from '@umbraco-cms/backoffice/external/signalr';
import { UmbBooleanState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import type { HubConnection } from '@umbraco-cms/backoffice/external/signalr';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

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
	#connection?: HubConnection;
	#currentArgs: UmbPreviewIframeArgs = {};
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	#resizeController?: AbortController;
	#serverUrl: string = '';

	#previewRepository = new UmbPreviewRepository(this);

	#localize = new UmbLocalizationController(this);

	#culture = new UmbStringState(undefined);
	public readonly culture = this.#culture.asObservable();

	#iframeReady = new UmbBooleanState(false);
	public readonly iframeReady = this.#iframeReady.asObservable();

	#previewUrl = new UmbStringState(undefined);
	public readonly previewUrl = this.#previewUrl.asObservable();

	#segment = new UmbStringState(undefined);
	public readonly segment = this.#segment.asObservable();

	#unique = new UmbStringState(undefined);
	public readonly unique = this.#unique.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_PREVIEW_CONTEXT);

		this.consumeContext(UMB_SERVER_CONTEXT, (serverContext) => {
			const params = new URLSearchParams(window.location.search);

			if (!params.has('id')) {
				console.error('No unique ID found in query string.');
				return;
			}

			this.#unique.setValue(params.get('id') ?? undefined);
			this.#culture.setValue(params.get('culture') ?? undefined);
			this.#segment.setValue(params.get('segment') ?? undefined);

			this.#currentArgs.culture = this.#culture.getValue();
			this.#currentArgs.segment = this.#segment.getValue();

			const serverUrl = serverContext?.getServerUrl();

			if (!serverUrl) {
				console.error('No server URL found in context');
				return;
			}

			this.#serverUrl = serverUrl;

			this.#setPreviewUrl({ serverUrl });

			this.#initHubConnection(serverUrl);
		});

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (notificationContext) => {
			this.#notificationContext = notificationContext;
		});
	}

	override hostDisconnected() {
		super.hostDisconnected();

		// Clean up event listeners
		this.#resizeController?.abort();

		// Clean up SignalR connection
		if (this.#connection) {
			this.#connection.stop();
			this.#connection = undefined;
		}
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
			if (payload === this.#unique.getValue()) {
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
		const unique = this.#unique.getValue();
		if (!unique) return null;

		const culture = this.#culture.getValue();
		const urlInfo = await this.#previewRepository.getPublishedUrl(unique, culture);

		if (!urlInfo?.url) return null;
		return urlInfo.url.startsWith('/') ? `${this.#serverUrl}${urlInfo.url}` : urlInfo.url;
	}

	#setPreviewUrl(args?: UmbPreviewUrlArgs) {
		const host = args?.serverUrl || this.#serverUrl;
		const unique = args?.unique || this.#unique.getValue();

		if (!unique) {
			throw new Error('No unique ID found in query string.');
		}

		const url = new URL(unique, host);
		const params = new URLSearchParams(url.search);

		const culture = args && 'culture' in args ? args.culture : this.#culture.getValue();
		const segment = args && 'segment' in args ? args.segment : this.#segment.getValue();

		const cultureParam = 'culture';
		const rndParam = 'rnd';
		const segmentParam = 'segment';

		if (culture) {
			params.set(cultureParam, culture);
			this.#culture.setValue(culture);
		} else {
			params.delete(cultureParam);
			this.#culture.setValue(undefined);
		}

		if (args?.rnd) {
			params.set(rndParam, args.rnd.toString());
		} else {
			params.delete(rndParam);
		}

		if (segment) {
			params.set(segmentParam, segment);
			this.#segment.setValue(segment);
		} else {
			params.delete(segmentParam);
			this.#segment.setValue(undefined);
		}

		const previewUrl = new URL(`${url.pathname}?${params.toString()}`, host);
		const previewUrlString = previewUrl.toString();

		this.#previewUrl.setValue(previewUrlString);
	}

	async exitPreview() {
		await this.#previewRepository.exit();

		// Stop SignalR connection without waiting - window will close anyway
		if (this.#connection) {
			this.#connection.stop();
			this.#connection = undefined;
		}

		// Close the preview window
		// This ensures that subsequent "Save and Preview" actions will create a new preview session
		window.close();
	}

	iframeLoaded(iframe: HTMLIFrameElement) {
		if (!iframe) return;
		this.#iframeReady.setValue(true);
		this.#setupScaling();
	}

	#setupScaling() {
		// Clean up old event listeners before adding new ones
		this.#resizeController?.abort();
		this.#resizeController = new AbortController();
		const signal = this.#resizeController.signal;

		const wrapper = this.getIFrameWrapper();
		if (!wrapper) return;

		const scaleIFrame = () => {
			if (wrapper.className === 'fullsize') {
				wrapper.style.transform = '';
			} else {
				const wScale = document.body.offsetWidth / (wrapper.offsetWidth + 30);
				const hScale = document.body.offsetHeight / (wrapper.offsetHeight + 30);
				const scale = Math.min(wScale, hScale, 1);
				wrapper.style.transform = `scale(${scale})`;
			}
		};

		// Set up listeners once per iframe load
		window.addEventListener('resize', scaleIFrame, { signal });
		wrapper.addEventListener('transitionend', scaleIFrame, { signal });

		// Initial scale
		scaleIFrame();
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

	async updateIFrame(args?: UmbPreviewIframeArgs) {
		const mergedArgs = { ...this.#currentArgs, ...args };

		const wrapper = this.getIFrameWrapper();
		if (!wrapper) return;

		// Check if URL will change (culture or segment changed)
		const urlWillChange =
			(args?.culture !== undefined && mergedArgs.culture !== this.#currentArgs.culture) ||
			(args?.segment !== undefined && mergedArgs.segment !== this.#currentArgs.segment);

		// Only show loading spinner if iframe will reload
		if (urlWillChange) {
			this.#iframeReady.setValue(false);
		}

		const params = new URLSearchParams(window.location.search);

		params.delete('culture');
		if (mergedArgs.culture) {
			params.set('culture', mergedArgs.culture);
		}

		params.delete('segment');
		if (mergedArgs.segment) {
			params.set('segment', mergedArgs.segment);
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
