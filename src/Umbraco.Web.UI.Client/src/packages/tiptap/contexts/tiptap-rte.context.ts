import { UMB_TIPTAP_RTE_CONTEXT } from './tiptap-rte.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbTiptapRteContext extends UmbContextBase {
	#editor?: Editor;

	constructor(host: UmbControllerHost) {
		super(host, UMB_TIPTAP_RTE_CONTEXT);
	}

	public getEditor(): Editor | undefined {
		return this.#editor;
	}

	public setEditor(editor?: Editor) {
		this.#editor = editor;
	}
}
