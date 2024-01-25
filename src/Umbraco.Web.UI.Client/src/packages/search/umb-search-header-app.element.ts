import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { CSSResultGroup} from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbModalManagerContext} from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-search-header-app')
export class UmbSearchHeaderAppElement extends UmbLitElement {
	private _modalContext?: UmbModalManagerContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (_instance) => {
			this._modalContext = _instance;
		});
	}

	#onSearchClick() {
		alert('implement search search modal');
	}

	render() {
		return html`
			<uui-button @click=${this.#onSearchClick} look="primary" label="search" compact>
				<uui-icon name="icon-search"></uui-icon>
			</uui-button>
		`;
	}

	static styles: CSSResultGroup = [
		UmbTextStyles,
		css`
			uui-button {
				font-size: 18px;
				--uui-button-background-color: transparent;
			}
		`,
	];
}

export default UmbSearchHeaderAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-search-header-app': UmbSearchHeaderAppElement;
	}
}
