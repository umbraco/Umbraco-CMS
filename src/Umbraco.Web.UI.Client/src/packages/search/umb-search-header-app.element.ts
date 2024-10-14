import { UMB_SEARCH_MODAL } from './search-modal/search-modal.token.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbHeaderAppButtonElement } from '@umbraco-cms/backoffice/components';

@customElement('umb-search-header-app')
export class UmbSearchHeaderAppElement extends UmbHeaderAppButtonElement {
	async #onSearchClick() {
		const context = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		context.open(this, UMB_SEARCH_MODAL);
	}

	override render() {
		return html`
			<uui-button @click=${this.#onSearchClick} look="primary" label="search" compact>
				<uui-icon name="icon-search"></uui-icon>
			</uui-button>
		`;
	}

	static override styles = UmbHeaderAppButtonElement.styles;
}

export default UmbSearchHeaderAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-search-header-app': UmbSearchHeaderAppElement;
	}
}
