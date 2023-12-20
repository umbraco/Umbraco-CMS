import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
import { UmbBlockTypeBase } from '../../types.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { appendToFrozenArray } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-input-block-type')
export class UmbInputBlockTypeElement<BlockType extends UmbBlockTypeBase = UmbBlockTypeBase> extends UmbLitElement {
	//

	@property({ type: Array, attribute: false })
	public get value() {
		return this._items;
	}
	public set value(items) {
		this._items = items ?? [];
	}

	@state()
	private _items: Array<BlockType> = [];

	@state()
	private _workspacePath?: string;

	#blockTypeWorkspaceModalRegistration;

	constructor() {
		super();

		this.#blockTypeWorkspaceModalRegistration = new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('block-type')
			.onSetup(() => {
				return { data: { entityType: 'block-type', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				const newpath = routeBuilder({});
				this._workspacePath = newpath;
			});
	}

	create() {
		//TODO: make flow of picking a element type first, and then:
		this.#blockTypeWorkspaceModalRegistration.open({}, 'create/element-type-key');
		// TODO: Move to on submit:
		this.getHostElement().dispatchEvent(new UmbChangeEvent());
	}

	requestRemoveItem(contentTypeKey: string) {
		alert('request remove ' + contentTypeKey);
		this.getHostElement().dispatchEvent(new UmbChangeEvent());
	}

	protected getFormElement() {
		return undefined;
	}

	render() {
		return html`
			${this._items ? repeat(this._items, (item) => item.contentElementTypeKey, this.#renderItem) : ''}
			${this.#renderButton()}
		`;
	}

	#renderButton() {
		return html`
			<uui-button id="add-button" look="placeholder" @click=${() => this.create()} label="open">
				<uui-icon name="icon-add"></uui-icon>
				Add
			</uui-button>
		`;
	}

	#renderItem = (item: BlockType) => {
		return html`
			<uui-card-block-type href="${this._workspacePath}/edit/${item.contentElementTypeKey}">
				<uui-action-bar slot="actions">
					<uui-button label="Copy media">
						<uui-icon name="icon-documents"></uui-icon>
					</uui-button>
					<uui-button @click=${() => this.requestRemoveItem(item.contentElementTypeKey)} label="Remove block">
						<uui-icon name="icon-trash"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</uui-card-block-type>
		`;
	};

	static styles = [
		css`
			:host {
				display: grid;
				gap: var(--uui-size-space-3);
				grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
			}

			#add-button {
				text-align: center;
				height: 160px;
			}

			uui-icon {
				display: block;
				margin: 0 auto;
			}
		`,
	];
}

export default UmbInputBlockTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-block-type': UmbInputBlockTypeElement;
	}
}
