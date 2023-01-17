import { UmbLitElement } from '@umbraco-cms/element';
import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UMB_NOTIFICATION_SERVICE_CONTEXT_ALIAS } from 'src/core/notification';

@customElement('umb-language-workspace')
export class UmbLanguageWorkspaceElement extends UmbLitElement {
	constructor() {
		super();
		this.consumeContext(UMB_NOTIFICATION_SERVICE_CONTEXT_ALIAS, (service) => {
			console.log(
				'🚀 ~ file: language-workspace.element.ts:11 ~ UmbLanguageWorkspaceElement ~ this.consumeContext ~ service',
				service
			);
			service.peek('positive', { data: { message: 'Language Workspace' } });
		});
	}
	render() {
		return html` <div>Language Workspace</div> `;
	}
}

export default UmbLanguageWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-workspace': UmbLanguageWorkspaceElement;
	}
}
