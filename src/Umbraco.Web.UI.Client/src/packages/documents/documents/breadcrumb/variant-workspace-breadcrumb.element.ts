import type { UmbDocumentTreeItemModel } from '../tree/index.js';
import { UmbDocumentTreeRepository } from '../tree/index.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

@customElement('umb-variant-workspace-breadcrumb')
export class UmbVariantWorkspaceBreadcrumbElement extends UmbLitElement {
	#workspaceContext?: any;
	#treeRepository = new UmbDocumentTreeRepository(this);

	@state()
	_isNew = false;

	@state()
	_name: string = '';

	@state()
	_ancestors: UmbDocumentTreeItemModel[] = [];

	constructor() {
		super();
		this.consumeContext(UMB_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			//this.observe(this.#workspaceContext.isNew, (value) => (this._isNew = value), 'breadcrumbWorkspaceIsNewObserver');
			this.#requestAncestors();
			this.#observeName();
		});
	}

	#observeName() {
		this.observe(
			this.#workspaceContext.splitView.activeVariantsInfo,
			(value) => {
				if (!value) return;
				const variantId = UmbVariantId.Create(value[0]);
				this._name = this.#workspaceContext.getName(variantId);
			},
			'breadcrumbWorkspaceActiveVariantNameObserver',
		);
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
						html`<uui-breadcrumb-item href="/section/content/workspace/document/edit/${item.unique}"
							>${item.name}</uui-breadcrumb-item
						>`,
				)}
				<uui-breadcrumb-item>${this._name}</uui-breadcrumb-item>
			</uui-breadcrumbs>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbVariantWorkspaceBreadcrumbElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-variant-workspace-breadcrumb': UmbVariantWorkspaceBreadcrumbElement;
	}
}
