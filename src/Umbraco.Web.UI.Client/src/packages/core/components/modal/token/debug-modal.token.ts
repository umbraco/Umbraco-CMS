import { TemplateResult } from 'lit';
import { UmbModalToken } from 'src/libs/modal';

export interface UmbContextDebuggerModalData {
	content: TemplateResult | string;
}

export const UMB_CONTEXT_DEBUGGER_MODAL = new UmbModalToken<UmbContextDebuggerModalData>('Umb.Modal.ContextDebugger', {
	type: 'sidebar',
	size: 'small',
});
