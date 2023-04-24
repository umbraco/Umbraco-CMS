import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS } from '../../modals/manifests';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	UMB_MODAL_CONTEXT_TOKEN,
	UMB_PARTIAL_VIEW_PICKER_MODAL,
	UmbModalContext,
	UmbModalHandler,
	UmbModalToken,
} from '@umbraco-cms/backoffice/modal';
import {
	ChooseInsertTypeModalResult,
	CodeSnippetType,
	UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_MODAL,
} from '../../modals/insert-choose-type-sidebar.element';
import { getInsertPartialSnippet } from '../../utils';

export const UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_MODAL = new UmbModalToken<{ hidePartialView: boolean }>(
	UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS,
	{
		type: 'sidebar',
		size: 'small',
	}
);

@customElement('umb-templating-insert-menu')
export class UmbTemplatingInsertMenuElement extends UmbLitElement {
	@property()
	value = '';

	private _modalContext?: UmbModalContext;

	#openModal?: UmbModalHandler;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this._modalContext = instance;
		});
	}

	#openChooseTypeModal = () => {
		this.#openModal = this._modalContext?.open(UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_MODAL, {
			hidePartialView: this.hidePartialView,
		});
		this.#openModal?.onSubmit().then((closedModal: ChooseInsertTypeModalResult) => {
			this.value = closedModal.value;
			this.#dispatchInsertEvent();
		});
	};

	#openInsertValueSidebar() {
		this.#openModal = this._modalContext?.open(UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_MODAL);
		this.#openModal?.onSubmit().then((value) => {
			this.value = value;
			this.#dispatchInsertEvent();
		});
	}

	#openInsertPartialViewSidebar() {
		this.#openModal = this._modalContext?.open(UMB_PARTIAL_VIEW_PICKER_MODAL);
		this.#openModal?.onSubmit().then((value) => {
			this.value = getInsertPartialSnippet(value.selection?.[0]) ?? '';
			this.#dispatchInsertEvent();
		});
	}

	#dispatchInsertEvent() {
		this.dispatchEvent(new CustomEvent('insert', { bubbles: true, cancelable: true, composed: false }));
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
									<uui-menu-item
										class="insert-menu-item"
										label="Partial view"
										title="Partial view"
										@click=${this.#openInsertPartialViewSidebar}>
									</uui-menu-item>
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-insert-menu': UmbTemplatingInsertMenuElement;
	}
}
