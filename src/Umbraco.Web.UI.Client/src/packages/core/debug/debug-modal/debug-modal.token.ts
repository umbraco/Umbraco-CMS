import { UmbModalToken } from '../../modal/token/modal-token.js';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';

export interface UmbContextDebuggerModalData {
	content: TemplateResult | string;
}

export const UMB_CONTEXT_DEBUGGER_MODAL = new UmbModalToken<UmbContextDebuggerModalData>('Umb.Modal.ContextDebugger', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
