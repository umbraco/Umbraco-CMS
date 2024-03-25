import type { UmbSaveableWorkspaceContext } from '../../contexts/tokens/saveable-workspace-context.interface.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, state, css, type PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbFormContext } from '@umbraco-cms/backoffice/form';

@customElement('umb-editable-workspace')
export class UmbEditableWorkspaceElement extends UmbLitElement {
	readonly #formContext = new UmbFormContext(this);

	@state()
	_routes: UmbRoute[] = [];

	public set api(api: UmbSaveableWorkspaceContext) {
		this.observe(api.routes.routes, (routes) => (this._routes = routes));

		new UmbExtensionsApiInitializer(this, umbExtensionsRegistry, 'workspaceContext', [api]);
	}

	protected firstUpdated(_changedProperties: PropertyValueMap<unknown> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);

		this.#formContext.setFormElement(this.shadowRoot!.querySelector<HTMLFormElement>('form'));
	}

	render() {
		return html`<uui-form>
			<form>
				<uui-input name="tester" required></uui-input>
				<umb-router-slot .routes="${this._routes}"></umb-router-slot>
			</form>
		</uui-form>`;
	}

	static styles = [
		css`
			form {
				display: contents;
			}
		`,
	];
}

export default UmbEditableWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editable-workspace': UmbEditableWorkspaceElement;
	}
}
