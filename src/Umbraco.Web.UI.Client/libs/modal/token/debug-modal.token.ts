import { TemplateResult } from 'lit';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbContextDebuggerModalData {
	content: TemplateResult | string;
}

export const UMB_CONTEXT_DEBUGGER_MODAL = new UmbModalToken<UmbContextDebuggerModalData>('Umb.Modal.ContextDebugger', {
	type: 'sidebar',
	size: 'small',
});
