import type { Editor } from '../externals.js';
import { resolveStylesheetHref } from './resolve-stylesheet-href.function.js';
import { UMB_TIPTAP_RTE_CONTEXT } from './tiptap-rte.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * The default root path for the stylesheets on the server.
 * This is used as a fallback if the server configuration is not available.
 */
const DEFAULT_STYLESHEET_ROOT_PATH = '/css';

export class UmbTiptapRteContext extends UmbContextBase {
	#editor?: Editor;

	#serverUrl = '';

	readonly #stylesheetRootPath = new UmbStringState(undefined);
	stylesheetRootPath = this.#stylesheetRootPath.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_TIPTAP_RTE_CONTEXT);

		this.consumeContext(UMB_SERVER_CONTEXT, (serverContext) => {
			// Absolute in split-dev-server setups (e.g. `VITE_UMBRACO_API_URL`) so stylesheet
			// requests hit the real Umbraco server instead of resolving against the Vite
			// client's own origin; a no-op prefix in production, where it equals `location.origin`.
			// Kept separate from `stylesheetRootPath` below, which stays origin-relative so
			// `resolveStylesheetHref` can still detect whether a configured stylesheet already includes it.
			this.#serverUrl = serverContext?.getServerUrl() ?? '';

			const serverConnection = serverContext?.getServerConnection();
			if (!serverConnection) {
				this.#stylesheetRootPath.setValue(DEFAULT_STYLESHEET_ROOT_PATH);
				return;
			}
			this.observe(serverConnection.umbracoCssPath, (umbracoCssPath) => {
				this.#stylesheetRootPath.setValue(umbracoCssPath ?? DEFAULT_STYLESHEET_ROOT_PATH);
			});
		});
	}

	/**
	 * Resolves a configured stylesheet path to an href on the Umbraco server.
	 * @param {string} stylesheet The configured stylesheet path, relative to the stylesheet root path.
	 * @returns {string} The href to use for the stylesheet link.
	 */
	public resolveStylesheetHref(stylesheet: string): string {
		return resolveStylesheetHref(
			stylesheet,
			this.#stylesheetRootPath.getValue() ?? DEFAULT_STYLESHEET_ROOT_PATH,
			this.#serverUrl,
		);
	}

	public getEditor(): Editor | undefined {
		return this.#editor;
	}

	public setEditor(editor?: Editor) {
		this.#editor = editor;
	}
}
