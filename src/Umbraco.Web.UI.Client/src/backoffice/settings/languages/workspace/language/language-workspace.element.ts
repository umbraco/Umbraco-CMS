import { html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from 'src/core/notification';

import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-language-workspace')
export class UmbLanguageWorkspaceElement extends UmbLitElement {
	constructor() {
		super();
		this.consumeContext(UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (service) => {
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
