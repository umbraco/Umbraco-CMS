import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_MODAL_CONTEXT_TOKEN, UmbModalContext, UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS } from '../insert-sidebar/manifest';
import { UMB_MODAL_TEMPLATING_INSERT_VALUE_SIDEBAR_MODAL } from '../insert-sidebar/insert-choose-type-sidebar.element';

export const UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_MODAL = new UmbModalToken<{ hidePartialView: boolean }>(
	UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS,
	{
		type: 'sidebar',
		size: 'small',
	}
);

@customElement('umb-templating-insert-menu')
export class UmbTemplatingInsertMenuElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			#insert-menu {
				margin: 0;
				padding: 0;
				margin-top: var(--uui-size-space-3);
				background-color: var(--uui-color-surface);
				box-shadow: var(--uui-shadow-depth-3);
				min-width: 150px;
			}

			#insert-menu > li,
			ul {
				padding: 0;
				width: 100%;
				list-style: none;
			}

			ul {
				transform: translateX(-100px);
			}

			.insert-menu-item {
				width: 100%;
			}

			umb-button-with-dropdown {
				--umb-button-with-dropdown-symbol-expand-margin-left: 0;
			}

			uui-icon[name='umb:add'] {
				margin-right: var(--uui-size-4);
			}
		`,
	];

	private _modalContext?: UmbModalContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	#openChooseTypeModal = () => {
		this._modalContext?.open(UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_MODAL, {
			hidePartialView: this.hidePartialView,
		});
	};

	#openInsertValueSidebar() {
		this._modalContext?.open(UMB_MODAL_TEMPLATING_INSERT_VALUE_SIDEBAR_MODAL);
	}

	@property()
	hidePartialView = false;

	render() {
		return html`
			<uui-button-group>
				<uui-button look="secondary" @click=${this.#openChooseTypeModal}>
					<uui-icon name="umb:add"></uui-icon>Insert</uui-button
				>
				<umb-button-with-dropdown
					look="secondary"
					compact
					placement="bottom-start"
					id="insert-button"
					label="open insert menu">
					<ul id="insert-menu" slot="dropdown">
						<li>
							<uui-menu-item
								class="insert-menu-item"
								target="_blank"
								label="Value"
								title="Value"
								@click=${this.#openInsertValueSidebar}>
							</uui-menu-item>
						</li>
						${this.hidePartialView
							? ''
							: html` <li>
									<uui-menu-item class="insert-menu-item" label="Partial view" title="Partial view"> </uui-menu-item>
							  </li>`}
						<li>
							<uui-menu-item class="insert-menu-item" label="Dictionary item" title="Dictionary item"> </uui-menu-item>
						</li>
						<li>
							<uui-menu-item class="insert-menu-item" label="Macro" title="Macro"> </uui-menu-item>
						</li>
					</ul>
				</umb-button-with-dropdown>
			</uui-button-group>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-insert-menu': UmbTemplatingInsertMenuElement;
	}
}
