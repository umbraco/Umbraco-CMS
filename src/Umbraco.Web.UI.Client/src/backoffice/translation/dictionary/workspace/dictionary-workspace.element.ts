import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbWorkspaceEntityElement } from '../../../../backoffice/shared/components/workspace/workspace-entity-element.interface';
import { UmbWorkspaceDictionaryContext } from './dictionary-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-dictionary-workspace')
export class UmbWorkspaceDictionaryElement extends UmbLitElement implements UmbWorkspaceEntityElement {
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
	_unique?: string;

	public load(entityKey: string) {
		this.#workspaceContext.load(entityKey);
		this._unique = entityKey;
	}

	public create(parentKey: string | null) {
		this.#workspaceContext.createScaffold(parentKey);
	}

	@state()
	private _name?: string | null = '';

	#workspaceContext = new UmbWorkspaceDictionaryContext(this);

	async connectedCallback() {
		super.connectedCallback();

		this.observe(this.#workspaceContext.name, (name) => {
			this._name = name;
		});
	}

	// TODO. find a way where we don't have to do this for all workspaces.
	#handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this.#workspaceContext?.setName(target.value);
			}
		}
	}

	render() {
		return html`
			<umb-workspace-layout alias="Umb.Workspace.Dictionary">
				<div id="header" slot="header">
					<uui-button href="/section/translation/dashboard" label="Back to list" compact>
						<uui-icon name="umb:arrow-left"></uui-icon>
					</uui-button>
					<uui-input .value=${this._name} @input="${this.#handleInput}" label="Dictionary name"></uui-input>
				</div>
			</umb-workspace-layout>
		`;
	}
}

export default UmbWorkspaceDictionaryElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-workspace': UmbWorkspaceDictionaryElement;
	}
}
