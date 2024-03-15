import { UmbScriptTreeRepository } from '../../tree/index.js';
import { html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbUniqueTreeItemModel } from '@umbraco-cms/backoffice/tree';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
@customElement('umb-workspace-breadcrumb')
export class UmbWorkspaceBreadcrumbElement extends UmbLitElement {
	#workspaceContext?: any;
	#treeRepository = new UmbScriptTreeRepository(this);

	@state()
	_isNew = false;

	@state()
	_name: string = '';

	@state()
	_ancestors: UmbUniqueTreeItemModel[] = [];

	@state()
	_workspaceBasePath?: string;

	constructor() {
		super();
		this.consumeContext(UMB_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(this.#workspaceContext.name, (value) => (this._name = value), 'breadcrumbWorkspaceNameObserver');
			this.#requestAncestors();
			this.#constructWorkspaceBasePath();
		});
	}

	async #requestAncestors() {
		/* TODO: implement breadcrumb for new items
		 We currently miss the parent item name for new items. We need to align with backend
		 how to solve it */
		const isNew = this.#workspaceContext?.getIsNew();
		if (isNew === true) return;

		const unique = this.#workspaceContext?.getUnique();

		if (!unique) throw new Error('Unique is not available');
		const { data } = await this.#treeRepository.requestTreeItemAncestors({ descendantUnique: unique });

		if (data) {
			this._ancestors = data;
		}
	}

	async #constructWorkspaceBasePath() {
		// TODO: temp solution to construct the base path.
		const sectionContext = await this.getContext(UMB_SECTION_CONTEXT);
		this._workspaceBasePath = `section/${sectionContext?.getPathname()}/workspace/${this.#workspaceContext!.getEntityType()}/edit`;
	}

	#getHref(ancestor: UmbUniqueTreeItemModel) {
		return ancestor.isFolder ? undefined : `${this._workspaceBasePath}/${ancestor.unique}`;
	}

	render() {
		return html`
			<uui-breadcrumbs>
				${this._ancestors.map(
					(ancestor) =>
						html`<uui-breadcrumb-item href="${ifDefined(this.#getHref(ancestor))}"
							>${ancestor.name}</uui-breadcrumb-item
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
