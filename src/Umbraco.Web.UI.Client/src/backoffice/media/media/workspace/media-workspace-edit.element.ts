import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbMediaWorkspaceContext } from './media-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-media-workspace-edit')
export class UmbMediaWorkspaceEditElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];

	@state()
	_key?: string;

	#umbWorkspaceContext?: UmbMediaWorkspaceContext;

	constructor() {
		super();

		this.consumeContext<UmbMediaWorkspaceContext>('umbWorkspaceContext', (instance) => {
			this.#umbWorkspaceContext = instance;
			this.#observeKey();
		});
	}

	#observeKey() {
		this.observe(this.#umbWorkspaceContext?.data, (data) => (this._key = data.key));
	}

	render() {
		return html`<umb-workspace-content alias="Umb.Workspace.Media">
			${this._key
				? html`
						<umb-workspace-action-menu
							slot="action-menu"
							entity-type="media"
							unique="${this._key}"></umb-workspace-action-menu>
				  `
				: nothing}
		</umb-workspace-content>`;
	}
}

export default UmbMediaWorkspaceEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-workspace-edit': UmbMediaWorkspaceEditElement;
	}
}
