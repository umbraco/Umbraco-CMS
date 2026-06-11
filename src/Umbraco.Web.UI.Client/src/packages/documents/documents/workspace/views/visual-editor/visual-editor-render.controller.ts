import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { VisualEditorService } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbVisualEditorRenderInput {
	unique: string;
	culture?: string;
	segment?: string;
	values: Array<{ alias: string; value: unknown; culture?: string; segment?: string }>;
}

const DEBOUNCE_MS = 500;

/**
 * Debounces visual editor edits and requests a server-side partial re-render, posting the resulting
 * HTML to the guest via the supplied callback. Latest-wins: a newer request aborts the in-flight one,
 * so stale HTML never overwrites newer DOM. On failure the last good DOM is kept (caller is notified).
 */
export class UmbVisualEditorRenderController extends UmbControllerBase {
	#getInput: () => UmbVisualEditorRenderInput | undefined;
	#postRender: (html: string) => void;
	#onError?: () => void;

	#timer?: ReturnType<typeof setTimeout>;
	#abort?: AbortController;

	constructor(
		host: UmbControllerHost,
		args: {
			getInput: () => UmbVisualEditorRenderInput | undefined;
			postRender: (html: string) => void;
			onError?: () => void;
		},
	) {
		super(host);
		this.#getInput = args.getInput;
		this.#postRender = args.postRender;
		this.#onError = args.onError;
	}

	/** Schedule a debounced re-render. Repeated calls within the debounce window collapse to one request. */
	requestRender() {
		if (this.#timer) clearTimeout(this.#timer);
		this.#timer = setTimeout(() => this.#render(), DEBOUNCE_MS);
	}

	async #render() {
		const input = this.#getInput();
		if (!input?.unique) return;

		this.#abort?.abort();
		const abort = new AbortController();
		this.#abort = abort;

		const { data, error } = await tryExecute(
			this,
			VisualEditorService.postVisualEditorRender({
				body: {
					unique: input.unique,
					culture: input.culture,
					segment: input.segment,
					values: input.values.map((v) => ({
						alias: v.alias,
						value: v.value,
						culture: v.culture,
						segment: v.segment,
					})),
				},
			}),
			{ abortSignal: abort.signal },
		);

		if (abort.signal.aborted) return; // A newer render superseded this one.

		if (error || !data) {
			console.error('[VisualEditor] Render failed', error);
			this.#onError?.();
			return;
		}

		this.#postRender(data.html);
	}

	override destroy() {
		if (this.#timer) clearTimeout(this.#timer);
		this.#abort?.abort();
		super.destroy();
	}
}
