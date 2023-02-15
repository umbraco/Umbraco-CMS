import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UmbWorkspaceEntityElement } from '../../../../backoffice/shared/components/workspace/workspace-entity-element.interface';
import { UmbWorkspaceMediaTypeContext } from './media-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-media-type-workspace')
export class UmbMediaTypeWorkspaceElement extends UmbLitElement implements UmbWorkspaceEntityElement {
	static styles = [
		UUITextStyles,
		css`
			#header {
				display: flex;
				padding: 0 var(--uui-size-space-6);
				gap: var(--uui-size-space-4);
				width: 100%;
			}
			uui-input {
				width: 100%;
			}
		`,
	];

	@state()
	private _unique?: string;

	@state()
	private _mediaTypeName?: string | null = '';

	@property()
	id!: string;

	#workspaceContext = new UmbWorkspaceMediaTypeContext(this);

	public load(entityKey: string) {
		this.#workspaceContext.load(entityKey);
		this._unique = entityKey;
	}

	public create() {
		this.#workspaceContext.createScaffold();
	}

	async connectedCallback() {
		super.connectedCallback();

		this.observe(this.#workspaceContext.name, (name) => {
			this._mediaTypeName = name;
		});
	}

	// TODO. find a way where we don't have to do this for all Workspaces.
	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext.setName(target.value);
			}
		}
	}

	render() {
		return html`<umb-workspace-layout alias="Umb.Workspace.MediaType">
			<uui-input id="header" slot="header" .value=${this._unique} @input="${this.#handleInput}"></uui-input>
		</umb-workspace-layout>`;
	}
}

export default UmbMediaTypeWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-workspace-': UmbMediaTypeWorkspaceElement;
	}
}
