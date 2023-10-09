import { UMB_DICTIONARY_WORKSPACE_CONTEXT } from './dictionary-workspace.context.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
@customElement('umb-dictionary-workspace-editor')
export class UmbDictionaryWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _name?: string | null = '';

	#workspaceContext?: typeof UMB_DICTIONARY_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_DICTIONARY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#observeName();
		});
	}

	#observeName() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.name, (name) => (this._name = name));
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
			<umb-workspace-editor alias="Umb.Workspace.Dictionary">
				<div id="header" slot="header">
					<uui-button href="section/dictionary/dashboard" label=${this.localize.term('general_backToOverview')} compact>
						<uui-icon name="umb:arrow-left"></uui-icon>
					</uui-button>
					<uui-input
						.value=${this._name ?? ''}
						@input="${this.#handleInput}"
						label="${this.localize.term('general_dictionary')} ${this.localize.term('general_name')}"></uui-input>
				</div>
			</umb-workspace-editor>
		`;
	}

	static styles = [
		css`
			#header {
				display: flex;
				gap: var(--uui-size-space-4);
				width: 100%;
			}
			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbDictionaryWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-workspace-editor': UmbDictionaryWorkspaceEditorElement;
	}
}
