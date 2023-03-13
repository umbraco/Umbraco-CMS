import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

@customElement('umb-workspace-package')
export class UmbWorkspacePackageElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			.header {
				display: flex;
				font-size: var(--uui-type-h5-size);
			}
		`,
	];

	@property()
	entityKey?: string;

	@state()
	_package?: any;

	connectedCallback(): void {
		super.connectedCallback();
		if (this.entityKey) this._getPackageData();
	}

	private _getPackageData() {
		//TODO

		this._package = {
			key: this.entityKey,
			name: 'A created package',
		};
	}

	private _navigateBack() {
		window.history.pushState({}, '', '/section/packages/view/installed');
	}

	private _renderHeader() {
		return html`<div class="header" slot="header">
			<uui-button compact @click="${this._navigateBack}">
				<uui-icon name="umb:arrow-left"></uui-icon>
				${this._package.name ?? 'Package name'}
			</uui-button>
		</div>`;
	}

	render() {
		return html`<umb-workspace-layout alias="Umb.Workspace.Package"> ${this._renderHeader()} </umb-workspace-layout> `;
	}
}

export default UmbWorkspacePackageElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-package': UmbWorkspacePackageElement;
	}
}
