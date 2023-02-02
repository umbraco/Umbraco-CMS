import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/element';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-api';
import { ManifestEntityAction } from 'libs/extensions-registry/entity-action.models';

@customElement('umb-workspace-action-menu')
export class UmbWorkspaceActionMenuElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			#action-menu-popover {
				display: block;
			}
			#action-menu-dropdown {
				overflow: hidden;
				z-index: -1;
				background-color: var(--uui-combobox-popover-background-color, var(--uui-color-surface));
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				width: 100%;
				height: 100%;
				box-sizing: border-box;
				box-shadow: var(--uui-shadow-depth-3);
				width: 500px;
			}
		`,
	];

	private _entityType = '';
	@property({ type: String, attribute: 'entity-type' })
	public get entityType() {
		return this._entityType;
	}
	public set entityType(value) {
		const oldValue = this._entityType;
		this._entityType = value;
		if (oldValue !== this._entityType) {
			this.#observeEntityActions();
			this.requestUpdate('entityType', oldValue);
		}
	}

	@state()
	private _entityActions?: Array<ManifestEntityAction>;

	@state()
	private _actionMenuIsOpen = false;

	#observeEntityActions() {
		// TODO: filter on entity type
		this.observe(umbExtensionsRegistry.extensionsOfType('entityAction'), (actions) => {
			this._entityActions = actions;
		});
	}

	#close() {
		this._actionMenuIsOpen = false;
	}

	#open() {
		this._actionMenuIsOpen = true;
	}

	render() {
		return html` ${this.#renderActionsMenu()} `;
	}

	#renderActionsMenu() {
		return html`
			<uui-popover  id="action-menu-popover" .open=${this._actionMenuIsOpen} @close=${this.#close}>
				<uui-button slot="trigger" label="Actions" @click=${this.#open}></uui-button>
				<div id="action-menu-dropdown" slot="popover">
					<uui-scroll-container>
						${this._entityActions?.map((manifest) => html`<umb-entity-action .manifest=${manifest}></umb-entity-action>`)}
					</uui-scroll-container>
				</div>
			</uui-popover>
			</div>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-action-menu': UmbWorkspaceActionMenuElement;
	}
}
