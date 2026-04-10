import { UMB_PREVIEW_CONTEXT } from '@umbraco-cms/backoffice/preview';
import { UmbBooleanState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

interface UmbPreviewIframeArgs {
	culture?: string;
	height?: string;
	segment?: string;
	width?: string;
	wrapperClass?: string;
}

/**
 * Lightweight adapter that provides UMB_PREVIEW_CONTEXT for the visual editor.
 * Delegates iframe management to the VE element's existing DOM and state,
 * allowing registered previewApp extensions to work inside the visual editor.
 */
export class UmbVisualEditorPreviewContext extends UmbContextBase {
	#currentArgs: UmbPreviewIframeArgs = {};
	#resizeController?: AbortController;

	#serverUrl: string;
	#getUnique: () => string | null | undefined;
	#onUrlChange: (url: string) => void;

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

	constructor(
		host: UmbControllerHost,
		config: {
			serverUrl: string;
			getUnique: () => string | null | undefined;
			onUrlChange: (url: string) => void;
		},
	) {
		super(host, UMB_PREVIEW_CONTEXT);
		this.#serverUrl = config.serverUrl;
		this.#getUnique = config.getUnique;
		this.#onUrlChange = config.onUrlChange;
		this.#unique.setValue(config.getUnique() ?? undefined);
	}

	override hostDisconnected() {
		super.hostDisconnected();
		this.#resizeController?.abort();
	}

	setIframeReady(ready: boolean) {
		this.#iframeReady.setValue(ready);
		if (ready) {
			this.#setupScaling();
		}
	}

	iframeLoaded(iframe: HTMLIFrameElement) {
		if (!iframe) return;
		this.setIframeReady(true);
	}

	getIFrameWrapper(): HTMLElement | undefined {
		return this.getHostElement().shadowRoot?.querySelector('#wrapper') as HTMLElement;
	}

	async updateIFrame(args?: UmbPreviewIframeArgs) {
		const mergedArgs = { ...this.#currentArgs, ...args };
		const wrapper = this.getIFrameWrapper();
		if (!wrapper) return;

		const urlWillChange =
			(args?.culture !== undefined && mergedArgs.culture !== this.#currentArgs.culture) ||
			(args?.segment !== undefined && mergedArgs.segment !== this.#currentArgs.segment);

		if (urlWillChange) {
			this.#iframeReady.setValue(false);
		}

		this.#currentArgs = mergedArgs;

		if (mergedArgs.culture) {
			this.#culture.setValue(mergedArgs.culture);
		} else {
			this.#culture.setValue(undefined);
		}

		if (mergedArgs.segment) {
			this.#segment.setValue(mergedArgs.segment);
		} else {
			this.#segment.setValue(undefined);
		}

		if (mergedArgs.wrapperClass) wrapper.className = mergedArgs.wrapperClass;
		if (mergedArgs.height) wrapper.style.height = mergedArgs.height;
		if (mergedArgs.width) wrapper.style.width = mergedArgs.width;

		// Rebuild preview URL with culture/segment params and notify the VE element
		if (urlWillChange) {
			const unique = this.#getUnique();
			if (unique && this.#serverUrl) {
				const url = new URL(unique, this.#serverUrl);
				url.searchParams.set('rnd', Date.now().toString());
				if (mergedArgs.culture) url.searchParams.set('culture', mergedArgs.culture);
				if (mergedArgs.segment) url.searchParams.set('segment', mergedArgs.segment);
				const urlString = url.toString();
				this.#previewUrl.setValue(urlString);
				this.#onUrlChange(urlString);
			}
		}
	}

	async openWebsite() {
		const unique = this.#getUnique();
		if (!unique || !this.#serverUrl) return;
		const url = new URL(unique, this.#serverUrl);
		if (this.#culture.getValue()) url.searchParams.set('culture', this.#culture.getValue()!);
		if (this.#segment.getValue()) url.searchParams.set('segment', this.#segment.getValue()!);
		window.open(url.toString(), '_blank');
	}

	async exitPreview() {
		// No-op in the visual editor — it is not a standalone preview window.
		// The user exits by navigating away from the visual editor workspace view.
	}

	reloadIFrame(iframe: HTMLIFrameElement) {
		iframe.contentDocument?.location.reload();
	}

	#setupScaling() {
		this.#resizeController?.abort();
		this.#resizeController = new AbortController();
		const signal = this.#resizeController.signal;

		const wrapper = this.getIFrameWrapper();
		if (!wrapper) return;

		const scaleIFrame = () => {
			if (wrapper.className === 'fullsize') {
				wrapper.style.transform = '';
			} else {
				const container = wrapper.parentElement;
				if (!container) return;
				const wScale = container.offsetWidth / (wrapper.offsetWidth + 30);
				const hScale = container.offsetHeight / (wrapper.offsetHeight + 30);
				const scale = Math.min(wScale, hScale, 1);
				wrapper.style.transform = `scale(${scale})`;
			}
		};

		window.addEventListener('resize', scaleIFrame, { signal });
		wrapper.addEventListener('transitionend', scaleIFrame, { signal });
		scaleIFrame();
	}
}
