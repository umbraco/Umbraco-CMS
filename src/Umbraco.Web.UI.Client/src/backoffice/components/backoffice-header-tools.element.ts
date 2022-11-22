import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbModalService } from '@umbraco-cms/services';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-backoffice-header-tools')
export class UmbBackofficeHeaderTools extends UmbContextConsumerMixin(LitElement) {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			#tools {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
			}

			.tool {
				font-size: 18px;
			}
		`,
	];

	constructor() {
		super();
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});
	}

	private _handleUserClick() {
		this._modalService?.userSettings();
	}

	private _modalService?: UmbModalService;

	render() {
		return html`
			<div id="tools">
				<uui-button class="tool" look="primary" label="Search" compact>
					<uui-icon name="search"></uui-icon>
				</uui-button>
				<uui-button class="tool" look="primary" label="Help" compact>
					<uui-icon name="favorite"></uui-icon>
				</uui-button>
				<uui-button @click=${this._handleUserClick} look="primary" style="font-size: 14px;" label="User" compact>
					<uui-avatar name="Mr Rabbit"></uui-avatar>
				</uui-button>
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header-tools': UmbBackofficeHeaderTools;
	}
}
