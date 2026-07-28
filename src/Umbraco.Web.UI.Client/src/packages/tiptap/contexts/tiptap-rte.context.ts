import type { Editor } from '../externals.js';
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

	#stylesheetRootPath = new UmbStringState(undefined);
	stylesheetRootPath = this.#stylesheetRootPath.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_TIPTAP_RTE_CONTEXT);

		this.consumeContext(UMB_SERVER_CONTEXT, (serverContext) => {
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

	public getEditor(): Editor | undefined {
		return this.#editor;
	}

	public setEditor(editor?: Editor) {
		this.#editor = editor;
	}
}
