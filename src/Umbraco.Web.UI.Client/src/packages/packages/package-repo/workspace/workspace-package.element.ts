import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-workspace-package')
export class UmbWorkspacePackageElement extends UmbLitElement {
	@property()
	entityId?: string;

	@state()
	_package?: any;

	override connectedCallback(): void {
		super.connectedCallback();
		if (this.entityId) this._getPackageData();
	}

	private _getPackageData() {
		//TODO

		this._package = {
			id: this.entityId,
			name: 'A created package',
		};
	}

	private _navigateBack() {
		window.history.pushState({}, '', 'section/packages/view/installed');
	}

	private _renderHeader() {
		return html`<div class="header" slot="header">
			<uui-button compact @click="${this._navigateBack}">
				<uui-icon name="icon-arrow-left"></uui-icon>
				<span data-mark="input:workspace-name">${this._package.name ?? 'Package name'}</span>
			</uui-button>
		</div>`;
	}

	override render() {
		return html`<umb-workspace-editor> ${this._renderHeader()} </umb-workspace-editor> `;
	}

	static override styles = [
		UmbTextStyles,
		css`
			.header {
				display: flex;
				font-size: var(--uui-type-h5-size);
			}
		`,
	];
}

export { UmbWorkspacePackageElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-package': UmbWorkspacePackageElement;
	}
}
