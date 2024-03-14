import type { UmbScriptTreeItemModel } from '../../tree/index.js';
import { UmbScriptTreeRepository } from '../../tree/index.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-workspace-breadcrumb')
export class UmbWorkspaceBreadcrumbElement extends UmbLitElement {
	#workspaceContext?: any;
	#treeRepository = new UmbScriptTreeRepository(this);

	@state()
	_isNew = false;

	@state()
	_name: string = '';

	@state()
	_ancestors: UmbScriptTreeItemModel[] = [];

	constructor() {
		super();
		this.consumeContext(UMB_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(this.#workspaceContext.isNew, (value) => (this._isNew = value), 'breadcrumbWorkspaceIsNewObserver');
			this.observe(this.#workspaceContext.name, (value) => (this._name = value), 'breadcrumbWorkspaceNameObserver');

			this.#requestAncestors();
		});
	}

	async #requestAncestors() {
		const unique = this.#workspaceContext?.getUnique();
		if (!unique) throw new Error('Unique is not available');
		const { data } = await this.#treeRepository.requestTreeItemAncestors({ descendantUnique: unique });

		if (data) {
			this._ancestors = data;
		}
	}

	render() {
		return html`
			<uui-breadcrumbs>
				${this._ancestors.map(
					(item) =>
						html`<uui-breadcrumb-item href="/section/settings/workspace/script/edit/${item.unique}"
							>${item.name}</uui-breadcrumb-item
						>`,
				)}
				<uui-breadcrumb-item>${this._name}</uui-breadcrumb-item>
			</uui-breadcrumbs>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbWorkspaceBreadcrumbElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-breadcrumb': UmbWorkspaceBreadcrumbElement;
	}
}
