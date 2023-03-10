import { TemplateResult } from 'lit';
import { UmbModalToken } from '@umbraco-cms/modal';

export interface UmbContextDebuggerModalData {
	content: TemplateResult | string;
}

export const UMB_CONTEXT_DEBUGGER_MODAL_TOKEN = new UmbModalToken<UmbContextDebuggerModalData>(
	'Umb.Modal.ContextDebugger',
	{
		type: 'sidebar',
		size: 'small',
	}
);
